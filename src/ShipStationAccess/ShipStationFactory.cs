using CuttingEdge.Conditions;

namespace ShipStationAccess
{
	public interface IShipStationFactory
	{
		V2.IShipStationService CreateServiceV2( V2.Models.ShipStationCredentials credentials );
	}

	public class ShipStationFactory : IShipStationFactory
	{
		public V2.IShipStationService CreateServiceV2( V2.Models.ShipStationCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			return new V2.ShipStationService( credentials );
		}
	}
}