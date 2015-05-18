using System;
using System.Globalization;
using ServiceStack.Text;

namespace ShipStationAccess.V2.Services
{
	public static class JsonSerialization
	{
		private static readonly TimeZoneInfo _pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById( "Pacific Standard Time" );

		static JsonSerialization()
		{
			JsConfig.ExcludeTypeInfo = true;
			JsConfig.DateHandler = DateHandler.ISO8601;
			JsConfig.AlwaysUseUtc = true;
			JsConfig.AssumeUtc = true;
			JsConfig.ConvertObjectTypesIntoStringDictionary = true;
			JsConfig< DateTime >.SerializeFn = SerializeDateTime;
			JsConfig< DateTime? >.SerializeFn = SerializeDateTime;
		}

		public static string SerializeToJson( this object @object )
		{
			return JsonSerializer.SerializeToString( @object );
		}

		public static T DeserializeJson< T >( this string jsonContent )
		{
			return JsonSerializer.DeserializeFromString< T >( jsonContent );
		}

		public static DateTime FromJsonToDateTime( this string jsonDate )
		{
			return DateTime.Parse( jsonDate.Trim( '"' ) ).PstToUtc();
		}

		#region Custom serialization
		private static string SerializeDateTime( DateTime? dateTimeNullable )
		{
			if( !dateTimeNullable.HasValue )
				return string.Empty;

			return SerializeDateTime( dateTimeNullable.Value );
		}

		private static string SerializeDateTime( DateTime utcTime )
		{
			if( utcTime.Kind == DateTimeKind.Unspecified || utcTime.Kind == DateTimeKind.Local )
				utcTime = DateTime.SpecifyKind( utcTime, DateTimeKind.Utc );

			if( utcTime == DateTime.MinValue || utcTime == DateTime.MaxValue || utcTime == default( DateTime ) )
				return utcTime.ToString( CultureInfo.InvariantCulture );

			return TimeZoneInfo.ConvertTime( utcTime, TimeZoneInfo.Utc, _pacificTimeZone ).ToString( CultureInfo.InvariantCulture );
		}
		#endregion
	}
}