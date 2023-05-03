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
using ShipStationAccess.V2.Exceptions;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;

namespace ShipStationAccess.V2.Services
{ 
	public interface IWebRequestServices
	{
		string GetApiKey();
		T GetResponse< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null );
		Task< T > GetResponseAsync< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null );
		void PostData( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null );
		Task PostDataAsync( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null );
		T PostDataAndGetResponse< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		Task< T > PostDataAndGetResponseAsync< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		T PostDataAndGetResponseWithShipstationHeader< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		bool CanSkipException( WebException e );

		DateTime? LastNetworkActivityTime { get; }
	}

	internal sealed class WebRequestServices : IWebRequestServices
	{
		private readonly ShipStationCredentials _credentials;

		public HttpClient HttpClient { get; private set; }
		public DateTime? LastNetworkActivityTime { get; private set; }

		public const int TooManyRequestsErrorCode = 429;
		public const int DefaultThrottlingWaitTimeInSeconds = 60;
		public const int MaxHttpRequestTimeoutInMinutes = 30;

		public string GetApiKey()
		{
			return _credentials.ApiKey;
		}

		public WebRequestServices( ShipStationCredentials credentials )
		{
			this._credentials = credentials;
			
			this.HttpClient = new HttpClient();
			this.HttpClient.Timeout = TimeSpan.FromMinutes( MaxHttpRequestTimeoutInMinutes );
			SetAuthorizationHeader();

			this.InitSecurityProtocol();
		}

		public bool CanSkipException( WebException e )
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
			return GetResponseAsync< T >( command, commandParams, token, operationTimeout ).GetAwaiter().GetResult();
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
			var response = await GetRawResponseAsync( url, token, operationTimeout ).ConfigureAwait( false );
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
			return PostRawDataAsync( url, jsonContent, token, false, operationTimeout );
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
			return PostDataAndGetResponseAsync< T >( command, jsonContent, token, shouldGetExceptionMessage, operationTimeout ).GetAwaiter().GetResult();
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
			
			var response = await PostRawDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout );
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
				var data = PostRawDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout, useShipStationPartnerHeader: true ).Result;
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
		private async Task< string > GetRawResponseAsync( string url, CancellationToken token, int? operationTimeout = null )
		{
			using( var cts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
			{
				if ( operationTimeout != null )
					cts.CancelAfter( operationTimeout.Value );

				RefreshLastNetworkActivityTime();

				var response = await this.HttpClient.GetAsync( url, cts.Token ).ConfigureAwait( false );
				var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

				RefreshLastNetworkActivityTime();
				ThrowIfError( url, response, responseContent );

				ShipStationLogger.Log.Info( "[shipstation]\tResponse for apiKey '{apiKey}' and url '{uri}' with timeout '{operationTimeout}': {response}", this._credentials.ApiKey, url, operationTimeout ?? MaxHttpRequestTimeoutInMinutes * 60 * 1000, responseContent );

				return responseContent;
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
		private async Task< string > PostRawDataAsync( string url, string payload, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null, bool useShipStationPartnerHeader = false )
		{
			this.LogPostInfo( this._credentials.ApiKey, url, payload, operationTimeout );
			RefreshLastNetworkActivityTime();
						
			try
			{
				using( var cts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
				{
					if ( operationTimeout != null )
						cts.CancelAfter( operationTimeout.Value * 1000 );

					if ( useShipStationPartnerHeader )
						SetAuthorizationHeader( true );

					var requestContent = new StringContent( payload, Encoding.UTF8, "application/json" );

					var responseMessage = await this.HttpClient.PostAsync( url, requestContent, cts.Token ).ConfigureAwait( false );
					var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait( false );

					RefreshLastNetworkActivityTime();
					ThrowIfError( url, responseMessage, responseContent );

					this.LogUpdateInfo( this._credentials.ApiKey, url, responseMessage.StatusCode, payload, operationTimeout );
					return responseContent;
				}
			}
			catch( Exception ex )
			{
				RefreshLastNetworkActivityTime();
				var webException = WebExtensions.GetWebException( ex );
				if ( webException != null )
				{
					var serverResponseError = webException.Response?.GetResponseString() ?? string.Empty;
					this.LogPostError( this._credentials.ApiKey, url, webException.Response?.GetHttpStatusCode() ?? HttpStatusCode.InternalServerError, payload, serverResponseError, operationTimeout );
					if( shouldGetExceptionMessage )
						throw new Exception( this.GetExceptionMessageFromResponse( webException, serverResponseError ), ex );
				}

				throw;
			}
		}

		private void ThrowIfError( string url, HttpResponseMessage responseMessage, string responseContent )
		{
			var serverStatusCode = responseMessage.StatusCode;
			if( serverStatusCode == HttpStatusCode.Unauthorized )
			{
				ShipStationLogger.Log.Info( "[{Channel}] [{Version}]\tRequest to '{Url}' returned HTTP Error with the response content: '{ResponseContent}'. Request Headers: {RequestMessageHeaders}, Response Headers: {ResponseMessageHeaders}", 
					Constants.ChannelName, 
					Constants.VersionInfo,
					url,
					responseContent, 
					responseMessage?.RequestMessage?.Headers,
					responseMessage?.Headers );
				throw new ShipStationUnauthorizedException();
			}

			if( !this.IsRequestThrottled( responseMessage, responseContent, out int rateResetInSeconds ) )
				return;
			
			ShipStationLogger.Log.Info( "[{Channel}] [{Version}]\tResponse for apiKey '{ApiKey}' and url '{Uri}':\n{ResetInSeconds} - {IsThrottled}\n{Response}",
				Constants.ChannelName, 
				Constants.VersionInfo,
				this._credentials.ApiKey, 
				url, 
				rateResetInSeconds, 
				true, 
				responseContent );
			throw new ShipStationThrottleException( rateResetInSeconds );
		}

		private bool IsRequestThrottled( HttpResponseMessage responseMessage, string responseContent, out int rateResetInSeconds )
		{
			rateResetInSeconds = DefaultThrottlingWaitTimeInSeconds;
			bool isThrottled = (int)responseMessage.StatusCode == TooManyRequestsErrorCode
							|| ( responseContent != null && responseContent.Contains( "\"message\": \"Too Many Requests\"" ) );

			if ( !isThrottled )
				return false;

			if ( responseMessage.Headers.TryGetValues( "X-Rate-Limit-Reset", out IEnumerable< string > rateLimitHeaderValues ) )
			{
				var rateLimitHeaderValue = rateLimitHeaderValues.FirstOrDefault();
				if ( !string.IsNullOrWhiteSpace( rateLimitHeaderValue ) )
				{
					int.TryParse( rateLimitHeaderValue, out rateResetInSeconds );
				}
			}
				
			return true;
		}

		private string GetExceptionMessageFromResponse( WebException ex, string responseString )
		{
			dynamic obj = JsonConvert.DeserializeObject( responseString );
			try
			{
				var serverErrorText = string.Empty;
				if ( !string.IsNullOrWhiteSpace( obj.ExceptionMessage?.ToString() ) )
					serverErrorText = obj.ExceptionMessage.ToString().Trim().TrimEnd( '.' );

				return string.Format( "[Shipstation] {0} [Reason] {1}", ex.Message, serverErrorText );
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

		private void LogUpdateInfo( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, int? operationTimeout )
		{
			ShipStationLogger.Log.Info( "[shipstation]\tPOSTing call for the apiKey '{apiKey}' and url '{url}' with timeout '{operationTimeout}' has been completed with code '{code}'.\n{content}", apiKey, url, operationTimeout ?? MaxHttpRequestTimeoutInMinutes * 60 * 1000, Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostInfo( string apiKey, string url, string jsonContent, int? operationTimeout )
		{
			ShipStationLogger.Log.Info( "[shipstation]\tPOSTed data for the apiKey '{apiKey}' and url '{url}' with timeout '{operationTimeout}':\n{jsonContent}", apiKey, url, operationTimeout ?? MaxHttpRequestTimeoutInMinutes * 60 * 1000, jsonContent );
		}

		private void LogPostError( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, WebException x )
		{
			ShipStationLogger.Log.Error( "[shipstation]\tERROR POSTing data for the apiKey '{apiKey}', url '{url}', code '{message}' and response '{code}':\n{content}", apiKey, url, x.Response.GetResponseString(), Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostError( string apiKey, string url, HttpStatusCode statusCode, string jsonContent, string responseString, int? operationTimeout )
		{
			ShipStationLogger.Log.Error( "[shipstation]\tERROR POSTing data for the apiKey '{apiKey}', url '{url}', timeout '{operationTimeout}', code '{message}' and response '{code}':\n{content}", apiKey, url, operationTimeout ?? MaxHttpRequestTimeoutInMinutes * 60 * 1000, responseString, Convert.ToInt32( statusCode ), jsonContent );
		}

		/// <summary>
		///	This method is used to update service's last network activity time.
		///	It's called every time before making API request to server or after handling the response.
		/// </summary>
		private void RefreshLastNetworkActivityTime()
		{
			this.LastNetworkActivityTime = DateTime.UtcNow;
		}
		#endregion
	}
}