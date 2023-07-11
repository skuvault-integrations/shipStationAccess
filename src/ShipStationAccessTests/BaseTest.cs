using System;
using LINQtoCSV;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2.Models;
using System.Linq;
using NUnit.Framework;
using ShipStationAccess.V2;
using SkuVault.Integrations.Core.Common;

namespace ShipStationAccessTests
{
	public abstract class BaseTest
	{
		protected ShipStationCredentials _credentials;
		protected ShipStationFactory _shipStationFactory;
		protected IShipStationService _shipStationService;
		protected SyncRunContext SyncRunContext => new SyncRunContext( 1, 2, "correlationId" );

		[ SetUp ]
		protected void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ShipStationCredentials.csv";
			Log.Logger = new LoggerConfiguration()
				.Destructure.ToMaximumDepth( 100 )
				.MinimumLevel.Verbose()
				.WriteTo.Console().CreateLogger();
			NetcoLogger.LoggerFactory = new SerilogLoggerFactory( Log.Logger );

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( testConfig == null )
				throw new InvalidOperationException( "Can't read ShipStationCredentials" );

			this._shipStationFactory = new ShipStationFactory( testConfig.PartnerKey );
			this._credentials = new ShipStationCredentials( testConfig.ApiKey, testConfig.ApiSecret );
			this._shipStationService = this._shipStationFactory.CreateServiceV2( this._credentials, this.SyncRunContext );
		}
	}
}