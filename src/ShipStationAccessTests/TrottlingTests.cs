using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LINQtoCSV;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using NUnit.Framework;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2.Models;

namespace ShipStationAccessTests
{
	public class TrottlingTests
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
	}
}