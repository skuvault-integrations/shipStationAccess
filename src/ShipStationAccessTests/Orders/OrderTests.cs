using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Netco.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.WarehouseLocation;
using ShipStationAccess.V2.Services;

namespace ShipStationAccessTests.Orders
{
	[ Explicit ]
	public class OrderTests : BaseTest
	{
		private const string TestOrderWithShipments = "564221696";
		private const string TestOrderWithFulfillments = "576752152";
		private readonly DateTime TestOrderWithShipmentsCreatedDate = new DateTime( 2020, 6, 10 );
		private readonly DateTime TestOrderWithFulfillmentsCreatedDate = new DateTime( 2020, 7, 20 );

		[Test]
		public void DeserializeOrderWithNullablePaymentDateTest()
		{
			var json = "{\"orders\":["
			           +
			           "{\"orderId\":156695469,\"orderNumber\":\"100267\",\"orderKey\":\"manual-916d07f350db46f4aeb8608ffec2b265\",\"orderDate\":\"2015-07-28T10:58:54.5730000\",\"createDate\":\"2015-07-28T10:58:54.5730000\",\"modifyDate\":\"2015-08-26T07:23:39.6100000\",\"paymentDate\":null,\"orderStatus\":\"shipped\",\"customerUsername\":\"1@1\",\"customerEmail\":\"1@1\",\"billTo\":{\"name\":\"meme\",\"company\":null,\"street1\":null,\"street2\":null,\"street3\":null,\"city\":null,\"state\":null,\"postalCode\":null,\"country\":null,\"phone\":null,\"residential\":null,\"addressVerified\":null},\"shipTo\":{\"name\":\"meme\",\"company\":\"\",\"street1\":\"11751 S DIXIE HWY\",\"street2\":\"\",\"street3\":null,\"city\":\"SONORA\",\"state\":\"KY\",\"postalCode\":\"42776-9739\",\"country\":\"US\",\"phone\":\"12707662520\",\"residential\":true,\"addressVerified\":\"Address validated successfully\"},\"items\":[{\"orderItemId\":212535867,\"lineItemKey\":null,\"sku\":\"testSku1\",\"name\":\"test\",\"imageUrl\":null,\"weight\":null,\"quantity\":1,\"unitPrice\":2.00,\"taxAmount\":null,\"shippingAmount\":null,\"warehouseLocation\":\"A1 (100), A1 (12)\",\"options\":[],\"productId\":14841724,\"fulfillmentSku\":null,\"adjustment\":false,\"upc\":null,\"createDate\":\"2015-07-28T10:58:54.573\",\"modifyDate\":\"2015-07-28T10:58:54.573\"}],\"orderTotal\":2.00,\"amountPaid\":0.00,\"taxAmount\":0.00,\"shippingAmount\":0.00,\"customerNotes\":null,\"internalNotes\":null,\"gift\":false,\"giftMessage\":null,\"paymentMethod\":null,\"requestedShippingService\":null,\"carrierCode\":null,\"serviceCode\":null,\"packageCode\":null,\"confirmation\":\"none\",\"shipDate\":\"2015-08-26\",\"holdUntilDate\":null,\"weight\":{\"value\":0.00,\"units\":\"ounces\"},\"dimensions\":null,\"insuranceOptions\":{\"provider\":null,\"insureShipment\":false,\"insuredValue\":0.0},\"internationalOptions\":{\"contents\":null,\"customsItems\":null,\"nonDelivery\":null},\"advancedOptions\":{\"warehouseId\":18911,\"nonMachinable\":false,\"saturdayDelivery\":false,\"containsAlcohol\":false,\"mergedOrSplit\":false,\"parentId\":null,\"storeId\":28400,\"customField1\":null,\"customField2\":null,\"customField3\":null,\"source\":null,\"billToParty\":null,\"billToAccount\":null,\"billToPostalCode\":null,\"billToCountryCode\":null},\"tagIds\":null,\"userId\":null},"
			           + "],\"total\":146,\"page\":1,\"pages\":2}";
			var orders = json.DeserializeJson<ShipStationOrders>();
			orders.Orders.Count.Should().Be(1);
			orders.Orders[0].PaymentDate.Should().Be(null);
		}

		[Test]
		public void DeserializeOrderWithManyCustomsItemsTest()
		{
			var json = "{\"orders\":["
			           +
			           "{\"orderId\":156799376,\"orderNumber\":\"104778\",\"orderKey\":\"104778\",\"orderDate\":\"2015-07-28T23:12:50.2500000\",\"createDate\":\"2015-07-28T23:13:55.6130000\",\"modifyDate\":\"2015-07-28T23:50:23.7370000\",\"paymentDate\":\"2015-08-26T07:23:39.6100000\",\"orderStatus\":\"cancelled\",\"customerUsername\":null,\"customerEmail\":null,\"billTo\":{\"name\":\"A B\",\"company\":\"\",\"street1\":\"line1\",\"street2\":\"\",\"street3\":null,\"city\":\"Silent Hill\",\"state\":\"\",\"postalCode\":\"444455511\",\"country\":\"NL\",\"phone\":\"\",\"residential\":null,\"addressVerified\":null},\"shipTo\":{\"name\":\"A B\",\"company\":\"\",\"street1\":\"line1\",\"street2\":\"\",\"street3\":null,\"city\":\"Silent Hill\",\"state\":\"\",\"postalCode\":\"444455511\",\"country\":\"NL\",\"phone\":\"\",\"residential\":false,\"addressVerified\":\"Address not yet validated\"},\"items\":[{\"orderItemId\":212701021,\"lineItemKey\":\"32077214\",\"sku\":\"testSku1\",\"name\":\"Test Product Sync\",\"imageUrl\":null,\"weight\":{\"value\":384.00,\"units\":\"ounces\"},\"quantity\":1,\"unitPrice\":1.00,\"taxAmount\":null,\"shippingAmount\":null,\"warehouseLocation\":\"A1 (100), A1 (12)\",\"options\":[],\"productId\":14841724,\"fulfillmentSku\":null,\"adjustment\":false,\"upc\":null,\"createDate\":\"2015-07-28T23:13:55.613\",\"modifyDate\":\"2015-07-28T23:13:55.613\"}],\"orderTotal\":1.00,\"amountPaid\":0.00,\"taxAmount\":0.00,\"shippingAmount\":0.00,\"customerNotes\":null,\"internalNotes\":null,\"gift\":false,\"giftMessage\":null,\"paymentMethod\":null,\"requestedShippingService\":null,\"carrierCode\":null,\"serviceCode\":null,\"packageCode\":null,\"confirmation\":\"none\",\"shipDate\":\"2015-07-28\",\"holdUntilDate\":null,\"weight\":{\"value\":384.00,\"units\":\"ounces\"},\"dimensions\":null,\"insuranceOptions\":{\"provider\":null,\"insureShipment\":false,\"insuredValue\":0.0},\"internationalOptions\":{\"contents\":\"merchandise\",\"customsItems\":[{\"customsItemId\":15187335,\"description\":\"Test Product Sync\",\"quantity\":1,\"value\":1.00,\"harmonizedTariffCode\":null,\"countryOfOrigin\":\"US\"}],\"nonDelivery\":\"return_to_sender\"},\"advancedOptions\":{\"warehouseId\":18911,\"nonMachinable\":false,\"saturdayDelivery\":false,\"containsAlcohol\":false,\"mergedOrSplit\":false,\"parentId\":null,\"storeId\":28400,\"customField1\":null,\"customField2\":null,\"customField3\":null,\"source\":null,\"billToParty\":null,\"billToAccount\":null,\"billToPostalCode\":null,\"billToCountryCode\":null},\"tagIds\":null,\"userId\":null},"
			           + "],\"total\":146,\"page\":1,\"pages\":2}";
			var orders = json.DeserializeJson<ShipStationOrders>();
			orders.Orders.Count.Should().Be(1);
			orders.Orders[0].InternationalOptions.CustomsItems.Count.Should().BeGreaterThan(0);
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
			var serverStub = this.PrepareShipStationServerStub( totalOrders, ordersPosWithErrors );
			var service = new ShipStationService( serverStub, this.SyncRunContext, new ShipStationTimeouts() );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: true );

			orders.Count().Should().Be( totalOrders - ordersPosWithErrors.Length );
		}

		[ Test ]
		public async Task GivenShipStationOrdersServiceWithInternalErrors_WhenDownloadOrdersAsyncCalled_ThenOrderPageWasSkipped()
		{
			var ordersPosWithErrors = new int[] { 62, 84 };
			var totalExpectedOrders = 100;
			var serverStub = PrepareShipStationServerStub( totalExpectedOrders, ordersPosWithErrors );
			var service = new ShipStationService( serverStub, this.SyncRunContext, new ShipStationTimeouts() );
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
			var orderShipments = await this._shipStationService.GetOrderShipmentsByIdAsync( TestOrderWithShipments, CancellationToken.None );

			orderShipments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrderFulfillmentsAsync()
		{
			var orderFulfillments = await this._shipStationService.GetOrderFulfillmentsByIdAsync( TestOrderWithFulfillments, CancellationToken.None );

			orderFulfillments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		[ Explicit ]
		public async Task GetOrderShipmentsByCreatedDateAsync_ShouldReturnOrdersWithShipments()
		{
			var orderShipments = await this._shipStationService.GetOrderShipmentsByCreatedDateAsync( TestOrderWithShipmentsCreatedDate, CancellationToken.None );

			orderShipments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		[ Explicit ]
		public async Task GetOrderFulfillmentsByCreatedDateAsync_ShouldReturnOrdersWithFullfilments()
		{
			var orderFulfillments = await this._shipStationService.GetOrderFulfillmentsByCreatedDateAsync( TestOrderWithFulfillmentsCreatedDate, CancellationToken.None );

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

			Assert.That( order, Is.Not.Null, "No order found to update" );
			var label = this._shipStationService.CreateAndGetShippingLabelAsync( order.AdvancedOptions.StoreId.ToString(), order.CarrierCode, order.ServiceCode, order.PackageCode, order.Confirmation, DateTime.UtcNow, null, null, CancellationToken.None ).Result;
			Assert.That( label, Is.Not.Null );
		}

		[ Test ]
		public void UpdateOrder()
		{
			var orders = this._shipStationService.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
			{
				Assert.Ignore( "No order found to update" );
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
				Assert.Ignore( "No order found to update" );
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
				Assert.Ignore( "No order found to update" );
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