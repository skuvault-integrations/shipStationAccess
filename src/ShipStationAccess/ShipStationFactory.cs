using CuttingEdge.Conditions;
using ShipStationAccess.V2;
using ShipStationAccess.V2.Models;

namespace ShipStationAccess
{
	public interface IShipStationFactory
	{
		IShipStationService CreateServiceV2( ShipStationCredentials credentials );
	}

	public class ShipStationFactory: IShipStationFactory
	{
		private string _partnerKey{ get; set; }

		public ShipStationFactory( string partnerKey = null )
		{
			this._partnerKey = partnerKey;
		}

		public IShipStationService CreateServiceV2( ShipStationCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			credentials.PartnerKey = this._partnerKey;
			return new ShipStationService( credentials );
		}
	}
}