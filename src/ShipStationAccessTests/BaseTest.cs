using LINQtoCSV;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2.Models;
using System.Linq;

namespace ShipStationAccessTests
{
	public abstract class BaseTest
	{
		protected readonly IShipStationFactory ShipStationFactory = new ShipStationFactory();

		protected ShipStationCredentials ReadCredentials()
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
			{
				return new ShipStationCredentials( testConfig.ApiKey, testConfig.ApiSecret );
			}

			return null;
		}
	}
}