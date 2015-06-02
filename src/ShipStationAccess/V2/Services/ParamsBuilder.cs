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
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				ShipStationParam.OrdersCreatedDateFrom.Name, startDate.ToString( "s" ),
				ShipStationParam.OrdersCreatedDateTo.Name, endDate.ToString( "s" ));
			return endpoint;
		}

		public static string CreateModifiedOrdersParams( DateTime startDate, DateTime endDate )
		{
			var endpoint = string.Format( "?{0}={1}&{2}={3}",
				ShipStationParam.OrdersModifiedDateFrom.Name, startDate.ToString( "s" ),
				ShipStationParam.OrdersModifiedDateTo.Name, endDate.ToString( "s" ));
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