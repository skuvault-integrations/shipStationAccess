using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;

namespace ShipStationAccess
{
	public interface IShipStationFactory
	{
		IShipStationService CreateServiceV2( ShipStationCredentials credentials, SyncRunContext syncRunContext, ShipStationTimeouts operationsTimeouts );
		IShipStationService CreateServiceV2( ShipStationCredentials credentials, SyncRunContext syncRunContext );
	}
}