using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace ShipStationAccess.V2.Services
{
	public static class JsonSerialization
	{
		private static JsonSerializerSettings JsonSerializerSettings
		{
			get
			{
				var settings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					DefaultValueHandling = DefaultValueHandling.Include
				};

				settings.Converters.Add(new DateTimeConverter());
				settings.Converters.Add(new DateTimeNullConverter());
				return settings;
			}
		}

		public static string SerializeToJson(this object @object)
		{
			return JsonConvert.SerializeObject(@object, JsonSerializerSettings);
		}

		public static T DeserializeJson<T>(this string jsonContent)
		{
			return JsonConvert.DeserializeObject<T>(jsonContent, JsonSerializerSettings);
		}
	}
	public static class DateTimeSerializeHelper
	{
		private static readonly TimeZoneInfo _pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById( "Pacific Standard Time" );

		public static string SerializeDateTime( DateTime? dateTimeNullable )
		{
			if (!dateTimeNullable.HasValue)
				return string.Empty;

			return SerializeDateTime(dateTimeNullable.Value);
		}

		public static string SerializeDateTime(DateTime utcTime)
		{
			if (utcTime.Kind == DateTimeKind.Unspecified || utcTime.Kind == DateTimeKind.Local)
				utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

			if (utcTime == DateTime.MinValue || utcTime == DateTime.MaxValue || utcTime == default(DateTime))
				return utcTime.ToString(CultureInfo.InvariantCulture);

			var pstTime =
				TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, _pacificTimeZone)
					.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
			return pstTime;
		}

		public static DateTime? DeserializeDateTimeNullable(string pstStringTime)
		{
			var dateTime = DeserializeDateTime(pstStringTime);
			return dateTime == default(DateTime) ? (DateTime?) null : dateTime;
		}

		public static DateTime DeserializeDateTime(string pstStringTime)
		{
			if (string.IsNullOrWhiteSpace(pstStringTime))
				return default(DateTime);

			DateTime pstTime;
			if (!DateTime.TryParse(pstStringTime, out pstTime))
				return pstTime;

			if (pstTime == DateTime.MinValue || pstTime == DateTime.MaxValue || pstTime == default(DateTime))
				return pstTime;

			var pacificTimeZone = _pacificTimeZone;

			if (pacificTimeZone.IsInvalidTime(pstTime) || pacificTimeZone.IsAmbiguousTime(pstTime))
				pstTime = pstTime.AddHours(1);

			if (pstTime.Kind != DateTimeKind.Unspecified)
				pstTime = DateTime.SpecifyKind(pstTime, DateTimeKind.Unspecified);
			var utcDate = TimeZoneInfo.ConvertTime(pstTime, pacificTimeZone, TimeZoneInfo.Utc);

			return utcDate;
		}
	}

	public class DateTimeConverter: JsonConverter
	{
		public override bool CanConvert( Type objectType )
		{
			return ( objectType == typeof( DateTime ) );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			var date = ( DateTime )value;
			writer.WriteValue( DateTimeSerializeHelper.SerializeDateTime( date ) );
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var dataString = reader.Value.ToString();
			var date = DateTimeSerializeHelper.DeserializeDateTime( dataString );

			return date;
		}
	}

	public class DateTimeNullConverter: JsonConverter
	{
		public override bool CanConvert( Type objectType )
		{
			return ( objectType == typeof( DateTime? ) );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			var date = ( DateTime? )value;
			writer.WriteValue( DateTimeSerializeHelper.SerializeDateTime( date ) );
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			if( reader.Value == null )
				return null;
			var dataString = reader.Value.ToString();
			var date = DateTimeSerializeHelper.DeserializeDateTime( dataString );

			return date;
		}
	}
}