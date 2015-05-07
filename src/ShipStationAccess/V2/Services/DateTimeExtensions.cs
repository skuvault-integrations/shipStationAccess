using System;

namespace ShipStationAccess.V2.Services
{
	public static class DateTimeExtensions
	{
		private static readonly TimeZoneInfo _pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById( "Pacific Standard Time" );
		public static DateTime UtcToPst( this DateTime utcTime )
		{
			if( utcTime == DateTime.MinValue || utcTime == DateTime.MaxValue || utcTime == default( DateTime ) )
				return utcTime;

			var pacificTimeZone = _pacificTimeZone;
			return TimeZoneInfo.ConvertTime( utcTime, TimeZoneInfo.Utc, pacificTimeZone );
		}
	}
}