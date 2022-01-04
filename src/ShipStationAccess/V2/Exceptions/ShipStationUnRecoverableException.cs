using System;

namespace ShipStationAccess.V2.Exceptions
{
	public class ShipStationUnrecoverableException : Exception
	{
		public ShipStationUnrecoverableException() { }
		public ShipStationUnrecoverableException( string message ) : base( message ) { }
		public ShipStationUnrecoverableException( string message, Exception innerException ) : base( message, innerException) { }
	}
}