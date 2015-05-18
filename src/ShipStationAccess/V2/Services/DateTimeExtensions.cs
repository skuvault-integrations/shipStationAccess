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

		public static DateTime PstToUtc( this DateTime pstTime, bool force = true )
		{
			if( pstTime == DateTime.MinValue || pstTime == DateTime.MaxValue || pstTime == default( DateTime ) )
				return pstTime;

			if( !force && pstTime.Kind == DateTimeKind.Utc )
				return pstTime;

			var pacificTimeZone = _pacificTimeZone;

			if( pacificTimeZone.IsInvalidTime( pstTime ) || pacificTimeZone.IsAmbiguousTime( pstTime ) )
			{
				pstTime = pstTime.AddHours( 1 );
			}

			if( pstTime.Kind != DateTimeKind.Unspecified )
				pstTime = DateTime.SpecifyKind( pstTime, DateTimeKind.Unspecified );
			var utcDate = TimeZoneInfo.ConvertTime( pstTime, pacificTimeZone, TimeZoneInfo.Utc );

			if( pacificTimeZone.IsDaylightSavingTime( utcDate ) )
				utcDate -= TimeSpan.FromHours( 1 );

			return utcDate;
		}
	}
}