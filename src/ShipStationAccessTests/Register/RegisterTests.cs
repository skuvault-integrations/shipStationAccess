using System;
using System.Linq;
using System.Threading;
using LINQtoCSV;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using NUnit.Framework;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Register;

namespace ShipStationAccessTests.Orders
{
	public class RegisterTests
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
		public void Register()
		{
			try
			{
				var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
				service.Register( new ShipStationRegister( "skuvault", this._credentials.ApiSecret, this._credentials.ApiKey ), CancellationToken.None );
			}
			catch( Exception ex )
			{
				Assert.Fail( ex.Message );
			}
		}
	}
}