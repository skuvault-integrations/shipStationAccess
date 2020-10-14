using System;

namespace ShipStationAccess.V2.Exceptions
{
	public class ShipStationUnRecoverableException : Exception
	{
		public ShipStationUnRecoverableException() { }
		public ShipStationUnRecoverableException( string message ) : base( message ) { }
		public ShipStationUnRecoverableException( string message, Exception innerException ) : base( message, innerException) { }
	}
}