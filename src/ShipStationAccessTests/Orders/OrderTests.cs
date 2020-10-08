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
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.WarehouseLocation;

namespace ShipStationAccessTests.Orders
{
	public class OrderTests
	{
		private readonly IShipStationFactory ShipStationFactory = new ShipStationFactory();
		private ShipStationCredentials _credentials;
		private string _testOrderWithShipments = "564221696";
		private string _testOrderWithFulfillments = "576752152";

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ShipStationCredentials.csv";
			Log.Logger = new LoggerConfiguration()
				.Destructure.ToMaximumDepth( 100 )
				.MinimumLevel.Verbose()
				.WriteTo.Console().CreateLogger();
			NetcoLogger.LoggerFactory = new SerilogLoggerFactory( Log.Logger );

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( testConfig != null )
				this._credentials = new ShipStationCredentials( testConfig.ApiKey, testConfig.ApiSecret );
		}

		[ Test ]
		public void GetOrders()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: true );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GivenTooSmallTimeout_WhenGetOrdersIsCalled_ThenExceptionIsReturned()
		{
			var timeouts = new ShipStationOperationsTimeouts();
			var tinyTimeout = new ShipStationOperationTimeout( 10 );
			timeouts.Set( ShipStationOperationEnum.ListOrders, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderShipments, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderFulfillments, tinyTimeout );

			var service = this.ShipStationFactory.CreateServiceV2( this._credentials, timeouts );

			var ex = Assert.Throws< WebException >( () => {
				var orders = service.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
			} );
			
			ex.Should().NotBeNull();
			ex.Status.Should().Be( WebExceptionStatus.Timeout );
		}

		[ Test ]
		public void GivenTooSmallTimeout_WhenGetOrdersAsyncIsCalled_ThenExceptionIsReturned()
		{
			var timeouts = new ShipStationOperationsTimeouts();
			var tinyTimeout = new ShipStationOperationTimeout( 10 );
			timeouts.Set( ShipStationOperationEnum.ListOrders, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderShipments, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderFulfillments, tinyTimeout );

			var service = this.ShipStationFactory.CreateServiceV2( this._credentials, timeouts );

			var ex = Assert.Throws< WebException >( async () => {
				var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
			} );
			
			ex.Should().NotBeNull();
			ex.Status.Should().Be( WebExceptionStatus.Timeout );
		}

		[ Test ]
		public async Task GetOrdersWithoutShipmentsAndFulfillmentsAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None, getShipmentsAndFulfillments: false );

			orders.Count().Should().BeGreaterThan( 0 );
			orders.Any( o => o.Shipments != null ).Should().Be( false );
			orders.Any( o => o.Fulfillments != null ).Should().Be( false );
		}

		[ Test ]
		public async Task GetOrderShipmentsAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orderShipments = await service.GetOrderShipmentsByIdAsync( this._testOrderWithShipments, CancellationToken.None );

			orderShipments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrderFulfillmentsAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orderFulfillments = await service.GetOrderFulfillmentsByIdAsync( this._testOrderWithFulfillments, CancellationToken.None );

			orderFulfillments.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetTags()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var tags = service.GetTags( CancellationToken.None );

			tags.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetTagsAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var tags = await service.GetTagsAsync( CancellationToken.None );

			tags.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetShippingLabelAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var order = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment && or.OrderNumber == 100339.ToString() );

			if( order == null )
				Assert.Fail( "No order found to update" );
			var label = service.CreateAndGetShippingLabelAsync( order.AdvancedOptions.StoreId.ToString(), order.CarrierCode, order.ServiceCode, order.PackageCode, order.Confirmation, DateTime.UtcNow, null, null, CancellationToken.None );
			label.Should().NotBeNull();
		}

		[ Test ]
		public void UpdateOrder()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
			{
				Assert.Fail( "No order found to update" );
				return;
			}

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			service.UpdateOrder( orderToChange, CancellationToken.None );
		}

		[ Test ]
		public async Task UpdateOrderAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -90 ), DateTime.UtcNow, CancellationToken.None );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
				return;

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			await service.UpdateOrderAsync( orderToChange, CancellationToken.None );
		}

		[ Test ]
		public void UpdateOrderOnGetOrders()
		{
			var rand = new Random();
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			Func< ShipStationOrder, ShipStationOrder > updateOrderLocation = o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				service.UpdateOrder( o, CancellationToken.None );
				return o;
			};
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -2 ), DateTime.UtcNow, CancellationToken.None, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task UpdateOrderOnGetOrdersAsync()
		{
			var rand = new Random();
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			Func< ShipStationOrder, Task< ShipStationOrder > > updateOrderLocation = async o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				await service.UpdateOrderAsync( o, CancellationToken.None );
				return o;
			};
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -7 ), DateTime.UtcNow, CancellationToken.None, true, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void UpdateOrderItemsWarehouseLocations()
		{
			var numbers = new List< string > { "100274", "100275" };
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
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

			service.UpdateOrderItemsWarehouseLocations( warehouseLocations, CancellationToken.None );
		}

		[ Test ]
		public async Task UpdateOrderItemsWarehouseLocationsAsync()
		{
			var numbers = new List< string > { "100274", "100275" };
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None );
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
			await service.UpdateOrderItemsWarehouseLocationsAsync( warehouseLocations, CancellationToken.None );
		}

		[ Test ]
		public void GetStores()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = service.GetStores( CancellationToken.None );

			stores.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetStoresAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = await service.GetStoresAsync( CancellationToken.None );

			stores.Count().Should().BeGreaterThan( 0 );
		}
	}
}