using Netco.Logging;

namespace ShipStationAccess.V2.Misc
{
	public class ShipStationLogger
	{
		public static ILogger Log{ get; private set; }

		static ShipStationLogger()
		{
			Log = NetcoLogger.GetLogger( "ShipStationLogger" );
		}
	}
}