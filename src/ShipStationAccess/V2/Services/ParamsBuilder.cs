using System;
using System.Text;
using ShipStationAccess.V2.Models.Command;

namespace ShipStationAccess.V2.Services
{
	internal static class ParamsBuilder
	{
		public static readonly string EmptyParams = string.Empty;

		public static string CreateNewOrdersParams( DateTime startDate, DateTime endDate )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}&{4}={5}",
				ShipStationParam.OrdersCreatedDateFrom.Name, DateTime.SpecifyKind( startDate, DateTimeKind.Utc ).ToString( "yyyy-M-d hh:mm" ),
				ShipStationParam.OrdersCreatedDateTo.Name, DateTime.SpecifyKind( endDate, DateTimeKind.Utc ).ToString( "yyyy-M-d hh:mm" ),
				ShipStationParam.OrdersStatus.Name, "unselected" );
			return endpoint;
		}

		public static string CreateModifiedOrdersParams( DateTime startDate, DateTime endDate )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}&{4}={5}",
				ShipStationParam.OrdersModifiedDateFrom.Name, DateTime.SpecifyKind( startDate, DateTimeKind.Utc ).ToString( "yyyy-M-d hh:mm" ),
				ShipStationParam.OrdersModifiedDateTo.Name, DateTime.SpecifyKind( endDate, DateTimeKind.Utc ).ToString( "yyyy-M-d hh:mm" ),
				ShipStationParam.OrdersStatus.Name, "unselected" );
			return endpoint;
		}

		public static string CreateGetNextPageParams( ShipStationCommandConfig config )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				ShipStationParam.PageSize.Name, config.PageSize,
				ShipStationParam.Page.Name, config.Page );
			return endpoint;
		}

		public static string ConcatParams( this string mainEndpoint, params string[] endpoints )
		{
			var result = new StringBuilder( mainEndpoint );

			foreach( var endpoint in endpoints )
				result.Append( endpoint.Replace( "?", "&" ) );

			return result.ToString();
		}
	}
}