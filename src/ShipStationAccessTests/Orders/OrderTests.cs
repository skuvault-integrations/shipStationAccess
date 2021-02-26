using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LINQtoCSV;
using Netco.Extensions;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.WarehouseLocation;
using ShipStationAccess.V2.Services;

namespace ShipStationAccessTests.Orders
{
	public class OrderTests : BaseTest
	{
		private ShipStationCredentials _credentials;
		private string _testOrderWithShipments = "564221696";
		private string _testOrderWithFulfillments = "576752152";

		private IShipStationService _shipStationService;

		[ SetUp ]
		public void Init()
		{
			this._credentials = base.ReadCredentials();
			if ( _credentials != null )
				_shipStationService = this.ShipStationFactory.CreateServiceV2( this._credentials );
		}

		[ Test ]
		public void GetOrders()
		{
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersAsync()
		{
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: true );

			orders.Count().Should().BeGreaterThan( 0 );
		}
		
		[ Test ]
		public async Task GetOrdersWithoutShipmentsAndFulfillmentsAsync()
		{
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: false );

			orders.Count().Should().BeGreaterThan( 0 );
			orders.Any( o => o.Shipments != null ).Should().Be( false );
			orders.Any( o => o.Fulfillments != null ).Should().Be( false );
		}

		[ Test ]
		public async Task GivenShipStationOrdersServiceWithInternalErrors_WhenGetOrdersAsyncCalled_ThenOrdersReturnedWithoutSkippedOrder()
		{
			var totalOrders = 120;
			var ordersPosWithErrors = new int[] { 17, 50, 67, 119 };
			var serverStub = PrepareShipStationServerStub( totalOrders, ordersPosWithErrors );
			var service = new ShipStationService( this._credentials, new ShipStationTimeouts(), serverStub );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: true );

			orders.Count().Should().Be( totalOrders - ordersPosWithErrors.Length );
		}

		[ Test ]
		public async Task GivenShipStationOrdersServiceWithInternalErrors_WhenDownloadOrdersAsyncCalled_ThenOrderPageWasSkipped()
		{
			var ordersPosWithErrors = new int[] { 62, 84 };
			var totalExpectedOrders = 100;
			var serverStub = PrepareShipStationServerStub( totalExpectedOrders, ordersPosWithErrors );
			var service = new ShipStationService( this._credentials, new ShipStationTimeouts(), serverStub );
			var createdOrdersResponse = await service.GetCreatedOrdersAsync( DateTime.UtcNow.AddDays( -30 ), DateTime.UtcNow, CancellationToken.None );

			createdOrdersResponse.ReadErrors.Count.Should().Be( ordersPosWithErrors.Length );
			for ( int i = 0; i < ordersPosWithErrors.Length; i++ )
			{
				createdOrdersResponse.ReadErrors[ i ].Page.Should().Be( ordersPosWithErrors[ i ] + 1 );
				createdOrdersResponse.ReadErrors[ i ].PageSize.Should().Be( 1 );
			}

			createdOrdersResponse.TotalEntitiesExpected.Should().Be( totalExpectedOrders );
		}

		private IWebRequestServices PrepareShipStationServerStub( int totalOrders, int[] ordersPosWithErrors )
		{
			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseAsync< List< ShipStationStore > >( Arg.Any< ShipStationCommand >(), Arg.Any< string >(), Arg.Any< CancellationToken >(), Arg.Any< int? >() ).Returns( ( x ) =>
			{
				return new List< ShipStationStore >()
				{
					{
						new ShipStationStore()
						{
							StoreId = 1, 
							MarketplaceId = 1,
							MarketplaceName = "SkuVault"
						}
					}
				};
			} );
			var ordersResponse = new List< ShipStationOrder >();
			for( int i = 1; i <= totalOrders; i++ )
			{
				ordersResponse.Add( new ShipStationOrder()
				{
					OrderId = i,
					AdvancedOptions = new ShipStationOrderAdvancedOptions(){ 
						StoreId = 1 
					} 
				} );
			}

			stubWebRequestService.GetResponseAsync< ShipStationOrders >( Arg.Any< ShipStationCommand >(), Arg.Any< string >(), Arg.Any< CancellationToken >(), Arg.Any< int? >() )
				.Returns( ( x ) => {
					GetPageAndPageSizeFromUrl( (string)x[ 1 ], out var requestedPage, out var requestedPageSize );
					var firstOrderIndexOnPage = ( requestedPage - 1 ) * requestedPageSize;
					if ( firstOrderIndexOnPage > totalOrders )
						return new ShipStationOrders();

					var isRequestedOrdersOutOfRange = ( firstOrderIndexOnPage + requestedPageSize ) > totalOrders;
					return new ShipStationOrders()
					{
						TotalOrders = ordersResponse.Count,
						TotalPages = (int)Math.Ceiling( ordersResponse.Count * 1.0 / requestedPageSize ),
						Orders = ordersResponse.GetRange( firstOrderIndexOnPage, isRequestedOrdersOutOfRange ? totalOrders - firstOrderIndexOnPage : requestedPageSize  )
					};
			} );
			stubWebRequestService.CanSkipException( Arg.Any< WebException >() ).Returns( true );
			stubWebRequestService.GetResponseAsync< ShipStationOrders >( Arg.Any< ShipStationCommand >(), 
				Arg.Is< string >( param => ShouldServerReturnInternalError( param, ordersPosWithErrors ) ), Arg.Any< CancellationToken >(), Arg.Any< int? >() )
				.Throws( new WebException() );

			return stubWebRequestService;
		}

		private bool ShouldServerReturnInternalError( string param, int[] ordersPosWithErrors )
		{
			this.GetPageAndPageSizeFromUrl( param, out var requestedPage, out var requestedPageSize );

			foreach( var orderWithErrorIndex in ordersPosWithErrors )
			{
				if ( orderWithErrorIndex >= ( requestedPage - 1 ) * requestedPageSize 
					&& orderWithErrorIndex < requestedPage * requestedPageSize )
				{
					return true;
				}
			}

			return false;
		}

		private void GetPageAndPageSizeFromUrl( string url, out int page, out int pageSize )
		{
			var urlParams = url.Split( new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries );
			page = 0;
			pageSize = 0;

			foreach( var urlParam in urlParams )
			{
				var pair = urlParam.Split( new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
				var paramName = pair[ 0 ];
				var paramValue = pair[ 1 ];

				switch( paramName )
				{
					case "pageSize":
					{
						pageSize = int.Parse( paramValue );
						break;
					}
					case "page":
					{
						page = int.Parse( paramValue );
						break;
					}
				}
			}
		}

		[ Test ]
		public async Task GetOrderShipmentsAsync()
		{
			var orderShipments = await this._shipStationService.GetOrderShipmentsByIdAsync( this._testOrderWithShipments, CancellationToken.None );

			orderShipments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrderFulfillmentsAsync()
		{
			var orderFulfillments = await this._shipStationService.GetOrderFulfillmentsByIdAsync( this._testOrderWithFulfillments, CancellationToken.None );

			orderFulfillments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GivenNotExistingOrderId_WhenGetOrderAsyncIsCalled_ThenNullResponseIsExpected()
		{
			var order = await this._shipStationService.GetOrderByIdAsync( "123452", CancellationToken.None );
			order.Should().BeNull();
		}

		[ Test ]
		public async Task GiveExistingOrderId_WhenGetOrderAsyncIsCalled_ThenOrderIsExpected()
		{
			var existingOrderId = "592317819";
			var order = await this._shipStationService.GetOrderByIdAsync( existingOrderId, CancellationToken.None );
			order.Should().NotBeNull();
		}
		
		[ Test ]
		public async Task WhenGetOrdersAsyncIsCalled_ThenModifiedLastActivityTimeIsExpected()
		{
			var lastActivityTimeBeforeMakingAnyRequest = this._shipStationService.LastActivityTime;
			
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: true );

			var lastActivityTimeAfterMakingRequests = this._shipStationService.LastActivityTime;
			lastActivityTimeAfterMakingRequests.Should().BeAfter( lastActivityTimeBeforeMakingAnyRequest );
		}

		[ Test ]
		public void GetTags()
		{
			var tags = this._shipStationService.GetTags( CancellationToken.None );

			tags.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetTagsAsync()
		{
			var tags = await this._shipStationService.GetTagsAsync( CancellationToken.None );

			tags.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetShippingLabelAsync()
		{
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var order = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment && or.OrderNumber == 100339.ToString() );

			if( order == null )
				Assert.Fail( "No order found to update" );
			var label = this._shipStationService.CreateAndGetShippingLabelAsync( order.AdvancedOptions.StoreId.ToString(), order.CarrierCode, order.ServiceCode, order.PackageCode, order.Confirmation, DateTime.UtcNow, null, null, CancellationToken.None ).Result;
			label.Should().NotBeNull();
		}

		[ Test ]
		public void UpdateOrder()
		{
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
			{
				Assert.Fail( "No order found to update" );
				return;
			}

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			this._shipStationService.UpdateOrder( orderToChange, CancellationToken.None );
		}

		[ Test ]
		public async Task UpdateOrderAsync()
		{
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow, CancellationToken.None );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
				return;

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			await this._shipStationService.UpdateOrderAsync( orderToChange, CancellationToken.None );
		}

		[ Test ]
		public void UpdateOrderOnGetOrders()
		{
			var rand = new Random();
			Func< ShipStationOrder, ShipStationOrder > updateOrderLocation = o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				this._shipStationService.UpdateOrder( o, CancellationToken.None );
				return o;
			};
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -2 ), DateTime.UtcNow, CancellationToken.None, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task UpdateOrderOnGetOrdersAsync()
		{
			var rand = new Random();
			Func< ShipStationOrder, Task< ShipStationOrder > > updateOrderLocation = async o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				await this._shipStationService.UpdateOrderAsync( o, CancellationToken.None );
				return o;
			};
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -7 ), DateTime.UtcNow, CancellationToken.None, true, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void UpdateOrderItemsWarehouseLocations()
		{
			var numbers = new List< string > { "100274", "100275" };
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var ordersToChange = orders.Select( o => o ).Where( or => or.IsValid() && numbers.Contains( or.OrderNumber ) ).ToList();
			if( ordersToChange.Count == 0 )
			{
				Assert.Fail( "No order found to update" );
				return;
			}

			var warehouseLocations = new ShipStationWarehouseLocations();
			foreach( var orderToCahnge in ordersToChange )
			{
				warehouseLocations.AddItems( "AA22(30)", orderToCahnge.Items.Select( x => x.OrderItemId ) );
			}

			this._shipStationService.UpdateOrderItemsWarehouseLocations( warehouseLocations, CancellationToken.None );
		}

		[ Test ]
		public async Task UpdateOrderItemsWarehouseLocationsAsync()
		{
			var numbers = new List< string > { "100274", "100275" };
			var orders = await this._shipStationService.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var ordersToChange = orders.Select( o => o ).Where( or => or.IsValid() && numbers.Contains( or.OrderNumber ) ).ToList();
			if( ordersToChange.Count == 0 )
			{
				Assert.Fail( "No order found to update" );
				return;
			}

			var warehouseLocations = new ShipStationWarehouseLocations();
			foreach( var orderToCahnge in ordersToChange )
			{
				warehouseLocations.AddItems( "AA25(35),DD(1)", orderToCahnge.Items.Select( x => x.OrderItemId ) );
			}
			await this._shipStationService.UpdateOrderItemsWarehouseLocationsAsync( warehouseLocations, CancellationToken.None );
		}

		[ Test ]
		public void GetStores()
		{
			var stores = this._shipStationService.GetStores( CancellationToken.None );

			stores.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetStoresAsync()
		{
			var stores = await this._shipStationService.GetStoresAsync( CancellationToken.None );

			stores.Count().Should().BeGreaterThan( 0 );
		}
	}
}