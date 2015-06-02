using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
using NUnit.Framework;
using ShipStationAccess;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Order;

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
			NetcoLogger.LoggerFactory = new ConsoleLoggerFactory();

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( testConfig != null )
				this._credentials = new ShipStationCredentials( testConfig.ApiKey, testConfig.ApiSecret );
		}

		[ Test ]
		public void GetOrders()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -1 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );;
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
				ShipStationLogger.Log.Trace( "************************************ {0} Run {1}", DateTime.Now, i );
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