using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;

namespace ShipStationAccess.V2.Services
{
	internal sealed class WebRequestServices
	{
		private readonly ShipStationCredentials _credentials;

		public WebRequestServices( ShipStationCredentials credentials )
		{
			this._credentials = credentials;
		}

		public T GetResponse< T >( ShipStationCommand command, string commandParams )
		{
			while( true )
			{
				var request = this.CreateGetServiceRequest( string.Concat( this._credentials.Host, command.Command, commandParams ) );
				var resetDelay = 0;
				try
				{
					using( var response = request.GetResponse() )
					{
						var shipStationResponse = ProcessResponse( response );
						if( !shipStationResponse.IsThrottled )
							return this.ParseResponse< T >( shipStationResponse.Data );

						resetDelay = shipStationResponse.ResetInSeconds;
					}
				}
				catch( WebException x )
				{
					var response = x.Response;
					var statusCode = Convert.ToInt32( response.GetHttpStatusCode() );
					if( statusCode == 429 )
						resetDelay = GetLimitReset( response );
				}

				this.CreateDelay( resetDelay ).Wait();
			}
		}

		public async Task< T > GetResponseAsync< T >( ShipStationCommand command, string commandParams )
		{
			while( true )
			{
				var request = this.CreateGetServiceRequest( string.Concat( this._credentials.Host, command.Command, commandParams ) );
				var resetDelay = 0;
				try
				{
					using( var response = await request.GetResponseAsync() )
					{
						var shipStationResponse = ProcessResponse( response );
						if( !shipStationResponse.IsThrottled )
							return this.ParseResponse< T >( shipStationResponse.Data );

						resetDelay = shipStationResponse.ResetInSeconds;
					}
				}
				catch( WebException x )
				{
					var response = x.Response;
					var statusCode = Convert.ToInt32( response.GetHttpStatusCode() );
					if( statusCode == 429 )
						resetDelay = GetLimitReset( response );
				}

				await this.CreateDelay( resetDelay );
			}
		}

		public void PostData( ShipStationCommand command, string jsonContent )
		{
			var request = this.CreateServicePostRequest( command, jsonContent );
//			this.LogPostInfo( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, jsonContent );

			try
			{
				using( var response = ( HttpWebResponse )request.GetResponse() )
					this.LogUpdateInfo( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, response.StatusCode, jsonContent );
			}
			catch( WebException x )
			{
				this.LogPostError( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, x.Response.GetHttpStatusCode(), jsonContent, x );
				throw;
			}
		}

		public async Task PostDataAsync( ShipStationCommand command, string jsonContent )
		{
			var request = this.CreateServicePostRequest( command, jsonContent );
			this.LogPostInfo( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, jsonContent );

			try
			{
				using( var response = ( HttpWebResponse )await request.GetResponseAsync() )
					this.LogUpdateInfo( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, response.StatusCode, jsonContent );
			}
			catch( WebException x )
			{
				this.LogPostError( this._credentials.ApiKey, request.RequestUri.AbsoluteUri, x.Response.GetHttpStatusCode(), jsonContent, x );
				throw;
			}
		}

		private HttpWebRequest CreateGetServiceRequest( string url )
		{
			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			this.CreateRequestHeaders( request );

			return request;
		}

		private HttpWebRequest CreateServicePostRequest( ShipStationCommand command, string content )
		{
			var uri = new Uri( string.Concat( this._credentials.Host, command.Command ) );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Post;
			request.ContentType = "application/json";
			this.CreateRequestHeaders( request );

			using( var writer = new StreamWriter( request.GetRequestStream() ) )
				writer.Write( content );

			return request;
		}

		#region Misc
		private void CreateRequestHeaders( WebRequest request )
		{
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
		}

		private string CreateAuthenticationHeader()
		{
			var authInfo = string.Concat( this._credentials.ApiKey, ":", this._credentials.ApiSecret );

			return string.Concat( "Basic ", Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) ) );
		}

		private T ParseResponse< T >( string jsonData )
		{
			var result = default(T);

			if( !string.IsNullOrEmpty( jsonData ) )
				result = jsonData.DeserializeJson< T >();

			return result;
		}

		private Task CreateDelay( int seconds )
		{
			return Task.Delay( seconds * 1000 );
		}

		private ShipStationResponse ProcessResponse( WebResponse response )
		{
			using( var stream = response.GetResponseStream() )
			{
				var reader = new StreamReader( stream );
				var jsonResponse = reader.ReadToEnd();
				var resetInSeconds = GetLimitReset( response );

				var isThrottled = jsonResponse.Contains( "\"message\": \"Too Many Requests\"" );

				ShipStationLogger.Log.Trace( "[shipstation]\tResponse for apiKey '{apiKey}' and url '{uri}':\n{resetInSeconds} - {isThrottled}\n{response}",
					this._credentials.ApiKey, response.ResponseUri, resetInSeconds, isThrottled, jsonResponse );

				return new ShipStationResponse
				{
					Data = jsonResponse, ResetInSeconds = resetInSeconds,
					IsThrottled = isThrottled
				};
			}
		}

		private static int GetLimitReset( WebResponse response )
		{
			var resetInSecondsString = response.Headers.Get( "X-Rate-Limit-Reset" );
			var resetInSeconds = 0;
			if( !string.IsNullOrWhiteSpace( resetInSecondsString ) )
				int.TryParse( resetInSecondsString, out resetInSeconds );
			return resetInSeconds;
		}

		private class ShipStationResponse
		{
			internal string Data;
			internal int ResetInSeconds;
			internal bool IsThrottled;
		}

		private void LogUpdateInfo( string apiKey, string url, HttpStatusCode statusCode, string jsonContent )
		{
			ShipStationLogger.Log.Trace( "[shipstation]\tPOSTing call for the apiKey '{apiKey}' and url '{url}' has been completed with code '{code}'.\n{content}", apiKey, url, Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostInfo( string apiKey, string url, string jsonContent )
		{
			ShipStationLogger.Log.Trace( "[shipstation]\tPOSTed data for the apiKey '{apiKey}' and url '{url}':\n{jsonContent}", apiKey, url, jsonContent );
		}

		private void LogPostError( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, WebException x )
		{
			ShipStationLogger.Log.Trace( "[shipstation]\tERROR POSTing data for the apiKey '{apiKey}', url '{url}', code '{message}' and response '{code}':\n{content}", apiKey, url, x.Response.GetResponseString(), Convert.ToInt32( statusCode ), jsonContent );
		}
		#endregion
	}
}