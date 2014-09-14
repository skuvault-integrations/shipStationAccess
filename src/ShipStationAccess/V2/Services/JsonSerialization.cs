using System;
using System.Globalization;
using ServiceStack.Text;

namespace ShipStationAccess.V2.Services
{
	public static class JsonSerialization
	{
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

		#region Custom serialization
		private static string SerializeDateTime( DateTime? dateTimeNullable )
		{
			if( !dateTimeNullable.HasValue )
				return string.Empty;

			return SerializeDateTime( dateTimeNullable.Value );
		}

		private static string SerializeDateTime( DateTime dateTime )
		{
			if( dateTime.Kind == DateTimeKind.Unspecified )
				dateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Utc );
			return dateTime.ToString( "yyy-MM-ddThh:mm+00:00", CultureInfo.InvariantCulture );
		}
		#endregion
	}
}