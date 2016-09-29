using System;

namespace ShipStationAccess.V2.Exceptions
{
	public class ShipStationRegisterException: Exception
	{
		public ShipStationRegisterException()
		{
		}

		public ShipStationRegisterException( string message )
			: base( message )
		{
		}

		public ShipStationRegisterException( string message, Exception inner )
			: base( message, inner )
		{
		}
	}
}
