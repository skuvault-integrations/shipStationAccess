using System;
using System.Collections.Generic;
using System.Linq;
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
using ShipStationAccess.V2.Services;

namespace ShipStationAccessTests.Orders
{
	public class OrderTests
	{
		private readonly IShipStationFactory ShipStationFactory = new ShipStationFactory();
		private ShipStationCredentials _credentials;

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
		public void DeserializationTest()
		{
			var json = "{\"orders\":[],\"total\":2,\"page\":1,\"pages\":3}";
			var orders = json.DeserializeJson< ShipStationOrders >();
			orders.TotalPages.Should().Be( 3 );
			orders.CurrentPageNumber.Should().Be( 1 );
			orders.TotalOrders.Should().Be( 2 );
		}

		[ Test ]
		public void GetOrders()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetOrdersAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		public async Task TrottlingTest()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var endDate = DateTime.UtcNow; //new DateTime( 2015, 06, 01, 22, 45, 00, DateTimeKind.Utc );

			var orders = service.GetOrders( endDate.AddDays( -1 ), endDate );

			var tasks = new List< Task >();

			foreach( var i in Enumerable.Range( 0, 500 ) )
			{
				tasks.Add( service.GetOrdersAsync( endDate.AddDays( -1 ), endDate ) );
			}

			await Task.WhenAll( tasks );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void UpdateOrder()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
				return;

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			service.UpdateOrder( orderToChange );
		}

		[ Test ]
		public async Task UpdateOrderAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -90 ), DateTime.UtcNow );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
				return;

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			await service.UpdateOrderAsync( orderToChange );
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
				service.UpdateOrder( o );
				return o;
			};
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -2 ), DateTime.UtcNow, updateOrderLocation );

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
				await service.UpdateOrderAsync( o );
				return o;
			};
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -2 ), DateTime.UtcNow, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetStores()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = service.GetStores();

			stores.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetStoresAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = await service.GetStoresAsync();

			stores.Count().Should().BeGreaterThan( 0 );
		}
	}
}