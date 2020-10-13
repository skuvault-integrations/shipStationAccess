using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;

namespace ShipStationAccess.V2.Services
{
	internal sealed class WebRequestServices
	{
		private readonly ShipStationCredentials _credentials;

		public HttpClient HttpClient { get; private set; }
		public DateTime? LastNetworkActivityTime { get; private set; }

		public const int TooManyRequestsErrorCode = 429;

		public string GetApiKey()
		{
			return _credentials.ApiKey;
		}

		public WebRequestServices( ShipStationCredentials credentials )
		{
			this._credentials = credentials;
			
			this.HttpClient = new HttpClient();
			SetAuthorizationHeader();

			this.InitSecurityProtocol();
		}

		public static bool CanSkipException( WebException e )
		{
			var errorResponse = e.Response as HttpWebResponse;
			return errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError;
		}

		/// <summary>
		///	Get response from ShipStation's endpoint
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <param name="commandParams"></param>
		/// <param name="token"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public T GetResponse< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null )
		{
			T result = default( T );

			try
			{
				result = GetResponseAsync< T >( command, commandParams, token, operationTimeout ).Result;
			}
			catch( AggregateException ex )
			{
				throw ex.InnerException;
			}

			return result;
		}

		/// <summary>
		///	Get response from ShipStation's endpoint async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <param name="commandParams"></param>
		/// <param name="token"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public async Task< T > GetResponseAsync< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null )
		{
			var url = string.Concat( this._credentials.Host, command.Command, commandParams );
			var response = await GetDataAsync( url, token, operationTimeout ).ConfigureAwait( false );
			if ( !string.IsNullOrWhiteSpace( response ) )
				return this.ParseResponse< T >( response );

			return default( T );
		}
		
		/// <summary>
		///	Post data to ShipStation's endpoint
		/// </summary>
		/// <param name="command"></param>
		/// <param name="jsonContent"></param>
		/// <param name="token"></param>
		/// <param name="operationTimeout"></param>
		public void PostData( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null )
		{
			PostDataAsync( command, jsonContent, token, operationTimeout ).Wait();
		}

		/// <summary>
		///	Post data to ShipStation's endpoint async
		/// </summary>
		/// <param name="command"></param>
		/// <param name="jsonContent"></param>
		/// <param name="token"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public Task PostDataAsync( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null )
		{
			var url = string.Concat( this._credentials.Host, command.Command );
			return PostDataAsync( url, jsonContent, token, false, operationTimeout );
		}

		/// <summary>
		///	Post data to ShipStation's endpoint and read response
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <param name="jsonContent"></param>
		/// <param name="token"></param>
		/// <param name="shouldGetExceptionMessage"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public T PostDataAndGetResponse< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null )
		{
			T result = default( T );

			try
			{
				result = PostDataAndGetResponseAsync< T >( command, jsonContent, token, shouldGetExceptionMessage, operationTimeout ).Result;
			}
			catch( AggregateException ex )
			{
				throw ex.InnerException;
			}

			return result;
		}

		/// <summary>
		///	Post data to ShipStation's endpoint and read response async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <param name="jsonContent"></param>
		/// <param name="token"></param>
		/// <param name="shouldGetExceptionMessage"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public async Task< T > PostDataAndGetResponseAsync< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null )
		{
			var url = string.Concat( this._credentials.Host, command.Command );
			
			var response = await PostDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout );
			if ( !string.IsNullOrWhiteSpace( response ) )
				return this.ParseResponse< T >( response );

			return default( T );
		}

		/// <summary>
		///	Post data to ShipStation's endpoint with specific partner header value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <param name="jsonContent"></param>
		/// <param name="token"></param>
		/// <param name="shouldGetExceptionMessage"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		public T PostDataAndGetResponseWithShipstationHeader< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null )
		{
			var url = string.Concat( this._credentials.Host, command.Command );
			int numberRequest = 0;
			while( numberRequest < 20 )
			{
				numberRequest++;
				var data = PostDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout, true ).Result;
				if ( !string.IsNullOrWhiteSpace( data ) )
					return this.ParseResponse< T >( data );
			}

			throw new Exception( "More 20 attempts" );
		}

		/// <summary>
		///	Get data from ShipStation's endpoint async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="token"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		private async Task< string > GetDataAsync( string url, CancellationToken token, int? operationTimeout = null )
		{
			while( true )
			{
				var resetDelay = 0;
				try
				{
					RefreshLastNetworkActivityTime();

					using( var cts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
					{
						if ( operationTimeout != null )
							cts.CancelAfter( operationTimeout.Value );

						var response = await this.HttpClient.GetAsync( url, cts.Token ).ConfigureAwait( false );
						var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

						RefreshLastNetworkActivityTime();

						var rateLimitHeaderValue = GetRateLimitHeaderValue( response );

						var shipStationResponse = ProcessResponse( url, responseContent, rateLimitHeaderValue );
						if( !shipStationResponse.IsThrottled )
							return shipStationResponse.Data;

						resetDelay = shipStationResponse.ResetInSeconds;
					}
				}
				catch( HttpRequestException ex )
				{
					RefreshLastNetworkActivityTime();

					var isRequestThrottled = false;
					if ( ex.InnerException != null && ex.InnerException is WebException )
					{
						var webEx = (WebException)ex.InnerException;

						var response = webEx.Response;
						if ( response != null )
						{
							var statusCode = Convert.ToInt32( response.GetHttpStatusCode() );
							switch( statusCode )
							{
								case (int)HttpStatusCode.NotFound:
									if( url.Contains( ShipStationCommand.GetOrder.Command ) )
										return null;

									throw;
								case TooManyRequestsErrorCode:
									resetDelay = GetLimitReset( response );
									isRequestThrottled = true;
									break;
								default:
									throw;
							}
						}
					}

					if ( !isRequestThrottled )
						throw;
				}

				await this.CreateDelay( resetDelay ).ConfigureAwait( false );
			}
		}

		/// <summary>
		///	Post data to ShipStation's API endpoint async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <param name="shouldGetExceptionMessage"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		private async Task< string > PostDataAsync( string url, string payload, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null, bool useShipStationPartnerHeader = false )
		{
			while( true )
			{
				this.LogPostInfo( this._credentials.ApiKey, url, payload );
				var resetDelay = 0;
				try
				{
					using( var cts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
					{
						RefreshLastNetworkActivityTime();
						
						if ( operationTimeout != null )
							cts.CancelAfter( operationTimeout.Value * 1000 );

						if ( useShipStationPartnerHeader )
							SetAuthorizationHeader( true );

						var requestContent = new StringContent( payload, Encoding.UTF8, "application/json" );

						var responseMessage = await this.HttpClient.PostAsync( url, requestContent, cts.Token ).ConfigureAwait( false );
						var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait( false );
						var rateLimitHeaderValue = GetRateLimitHeaderValue( responseMessage );
						RefreshLastNetworkActivityTime();

						var shipStationResponse = this.ProcessResponse( url, responseContent, rateLimitHeaderValue );
						if( !shipStationResponse.IsThrottled )
						{
							this.LogUpdateInfo( this._credentials.ApiKey, url, responseMessage.StatusCode, payload );
							return shipStationResponse.Data;
						}

						resetDelay = shipStationResponse.ResetInSeconds;
					}
				}
				catch( HttpRequestException e )
				{
					RefreshLastNetworkActivityTime();

					var isRequestThrottled = false;

					if ( e.InnerException != null && e.InnerException is WebException )
					{
						var ex = (WebException)e.InnerException;
						if ( ex.Response != null )
						{
							var responseString = ex.Response.GetResponseString();
							this.LogPostError( this._credentials.ApiKey, url, ex.Response.GetHttpStatusCode(), payload, responseString );
							var response = ex.Response;
							var statusCode = Convert.ToInt32( response.GetHttpStatusCode() );

							if ( statusCode == TooManyRequestsErrorCode )
							{
								resetDelay = GetLimitReset( response );
								isRequestThrottled = true;
							}
							else
							{
								if( shouldGetExceptionMessage )
									throw new Exception( this.GetExceptionMessageFromResponse( ex, responseString ), ex );
							}
						}
					}
					
					if ( !isRequestThrottled )
						throw;
				}

				await this.CreateDelay( resetDelay ).ConfigureAwait( false );
			}
		}

		private string GetExceptionMessageFromResponse( WebException ex, string responseString )
		{
			dynamic obj = JsonConvert.DeserializeObject( responseString );
			try
			{
				return string.Format( "[Shipstation] {0} [Reason] {1}", ex.Message, obj.ExceptionMessage.ToString().Trim().TrimEnd( '.' ) );
			}
			catch( RuntimeBinderException )
			{
				return ex.Message;
			}
		}

		#region Misc
		private void InitSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
		}

		private void SetAuthorizationHeader( bool useShipStationPartnerHeader = false )
		{
			this.HttpClient.DefaultRequestHeaders.Remove( "Authorization" );
			this.HttpClient.DefaultRequestHeaders.Add( "Authorization", this.CreateAuthenticationHeader() );

			if( !string.IsNullOrEmpty( this._credentials.PartnerKey ) )
			{
				if ( useShipStationPartnerHeader )
				{
					this.HttpClient.DefaultRequestHeaders.Remove( "x-shipstation-partner" );
					this.HttpClient.DefaultRequestHeaders.Add( "x-shipstation-partner", this._credentials.PartnerKey );
				}
				else
				{
					this.HttpClient.DefaultRequestHeaders.Remove( "x-partner" );
					this.HttpClient.DefaultRequestHeaders.Add( "x-partner", this._credentials.PartnerKey );
				}
			}
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

		private ShipStationResponse ProcessResponse( string url, string jsonResponse, string rateLimitHeaderValue )
		{
			var resetInSeconds = GetLimitReset( rateLimitHeaderValue );
			var isThrottled = jsonResponse.Contains( "\"message\": \"Too Many Requests\"" );

			ShipStationLogger.Log.Info( "[shipstation]\tResponse for apiKey '{apiKey}' and url '{uri}':\n{resetInSeconds} - {isThrottled}\n{response}",
				this._credentials.ApiKey, url, resetInSeconds, isThrottled, jsonResponse );

			return new ShipStationResponse
			{
				Data = jsonResponse, 
				ResetInSeconds = resetInSeconds,
				IsThrottled = isThrottled
			};
		}

		private static int GetLimitReset( WebResponse response )
		{
			var resetInSecondsString = response.Headers.Get( "X-Rate-Limit-Reset" );
			var resetInSeconds = 0;
			if( !string.IsNullOrWhiteSpace( resetInSecondsString ) )
				int.TryParse( resetInSecondsString, out resetInSeconds );
			return resetInSeconds;
		}

		private static string GetRateLimitHeaderValue( HttpResponseMessage response )
		{
			var rateLimitHeaderValue = string.Empty;
			if ( response.Headers.TryGetValues( "X-Rate-Limit-Reset", out IEnumerable<string> rateLimitHeaderValues ) )
				rateLimitHeaderValue = rateLimitHeaderValues.FirstOrDefault();

			return rateLimitHeaderValue;
		}

		private static int GetLimitReset( string rateLimitHeaderValue )
		{
			var resetInSeconds = 0;
			if( !string.IsNullOrWhiteSpace( rateLimitHeaderValue ) )
				int.TryParse( rateLimitHeaderValue, out resetInSeconds );
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
			ShipStationLogger.Log.Info( "[shipstation]\tPOSTing call for the apiKey '{apiKey}' and url '{url}' has been completed with code '{code}'.\n{content}", apiKey, url, Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostInfo( string apiKey, string url, string jsonContent )
		{
			ShipStationLogger.Log.Info( "[shipstation]\tPOSTed data for the apiKey '{apiKey}' and url '{url}':\n{jsonContent}", apiKey, url, jsonContent );
		}

		private void LogPostError( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, WebException x )
		{
			ShipStationLogger.Log.Error( "[shipstation]\tERROR POSTing data for the apiKey '{apiKey}', url '{url}', code '{message}' and response '{code}':\n{content}", apiKey, url, x.Response.GetResponseString(), Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostError( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, string responseString )
		{
			ShipStationLogger.Log.Error( "[shipstation]\tERROR POSTing data for the apiKey '{apiKey}', url '{url}', code '{message}' and response '{code}':\n{content}", apiKey, url, responseString, Convert.ToInt32( statusCode ), jsonContent );
		}

		private void RefreshLastNetworkActivityTime()
		{
			this.LastNetworkActivityTime = DateTime.UtcNow;
		}
		#endregion
	}
}