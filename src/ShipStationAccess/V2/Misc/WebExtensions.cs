using System;
using System.IO;
using System.Net;

namespace ShipStationAccess.V2.Misc
{
	public static class WebExtensions
	{
		public static WebException GetWebException( Exception ex )
		{
			if ( ex is WebException )
				return (WebException)ex;

			if ( ex is AggregateException )
			{
				var aggregateException = (AggregateException)ex;
				foreach( var innerException in aggregateException.InnerExceptions )
				{
					var webEx = GetWebException( innerException );
					if ( webEx != null )
						return webEx;
				}
			}

			if ( ex.InnerException != null )
			{
				return GetWebException( ex.InnerException );
			}

			return null;
		}

		public static HttpStatusCode GetHttpStatusCode( this WebResponse response )
		{
			return ( ( HttpWebResponse )response ).StatusCode;
		}

		public static string GetResponseString( this WebResponse response )
		{
			using( var reader = new StreamReader( response.GetResponseStream() ) )
			{
				var message = reader.ReadToEnd();
				return message;
			}
		}
		
		public static Tuple< Uri, HttpStatusCode, string > GetWebExceptionInfo( this WebException x )
		{
			var response = x.Response;
			return Tuple.Create( response.ResponseUri, response.GetHttpStatusCode(), response.GetResponseString() ) ;
		}
	}
}