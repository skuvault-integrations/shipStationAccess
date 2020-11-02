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
using NUnit.Framework;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.WarehouseLocation;

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