using System;

namespace ShipStationAccess.V2.Exceptions
{
	public class ShipStationThrottleException : Exception
	{
		public int ResetInSeconds { get; private set; }

		public ShipStationThrottleException( int resetInSeconds )
			: base()
		{
			this.ResetInSeconds = resetInSeconds;
		}
	}
}