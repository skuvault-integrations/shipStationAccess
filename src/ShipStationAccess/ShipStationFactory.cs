namespace ShipStationAccess
{
	public interface IShipStationFactory
	{
		IShipStationService CreateService( string userName, string password );
	}

	public class ShipStationFactory : IShipStationFactory
	{
		public IShipStationService CreateService( string userName, string password )
		{
			return new ShipStationService( userName, password );
		}
	}
}