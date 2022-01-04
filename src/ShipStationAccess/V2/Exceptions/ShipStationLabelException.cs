using System;

namespace ShipStationAccess.V2.Exceptions
{
	public class ShipStationLabelException: ShipStationUnrecoverableException
	{
		public ShipStationLabelException()
		{
		}

		public ShipStationLabelException( string message )
			: base( message )
		{
		}

		public ShipStationLabelException( string message, Exception inner )
			: base( message, inner )
		{
		}
	}
}
