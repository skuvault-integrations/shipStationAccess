using CuttingEdge.Conditions;
using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Services;

namespace ShipStationAccess
{
	public class ShipStationFactory: IShipStationFactory
	{
		private string PartnerKey{ get; }

		public ShipStationFactory( string partnerKey = null )
		{
			this.PartnerKey = partnerKey;
		}

		public IShipStationService CreateServiceV2( ShipStationCredentials credentials, SyncRunContext syncRunContext )
		{
			return CreateServiceV2( credentials, syncRunContext, new ShipStationTimeouts() );
		}

		public IShipStationService CreateServiceV2( ShipStationCredentials credentials, SyncRunContext syncRunContext, ShipStationTimeouts operationsTimeouts )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			if( string.IsNullOrWhiteSpace( credentials.PartnerKey ) )
				credentials.PartnerKey = this.PartnerKey;

			var webRequestServices = new WebRequestServices( credentials, syncRunContext );
			return new ShipStationService( webRequestServices, syncRunContext, operationsTimeouts );
		}
	}
}