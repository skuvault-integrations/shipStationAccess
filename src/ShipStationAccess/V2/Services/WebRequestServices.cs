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
	internal sealed class WebRequestServices : IWebRequestServices
	{
		private const int TooManyRequestsErrorCode = 429;
		private const int DefaultThrottlingWaitTimeInSeconds = 60;
		private const int MaxHttpRequestTimeoutInMinutes = 30;
		private const int MaxPostDataAttemptsCount = 20;

		private readonly ShipStationCredentials _credentials;
		private readonly SyncRunContext _syncRunContext;

		private HttpClient HttpClient{ get; }
		public DateTime? LastNetworkActivityTime { get; private set; }


		public WebRequestServices( ShipStationCredentials credentials, SyncRunContext syncRunContext )
		{
			this._credentials = credentials;
			this._syncRunContext = syncRunContext;

			this.HttpClient = new HttpClient();
			this.HttpClient.Timeout = TimeSpan.FromMinutes( MaxHttpRequestTimeoutInMinutes );
			this.SetAuthorizationHeader();

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
			return this.GetResponseAsync< T >( command, commandParams, token, operationTimeout ).GetAwaiter().GetResult();
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
			var response = await this.GetRawResponseAsync( url, token, operationTimeout ).ConfigureAwait( false );
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
			this.PostDataAsync( command, jsonContent, token, operationTimeout ).Wait();
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
			return this.PostRawDataAsync( url, jsonContent, token, false, operationTimeout );
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
			return this.PostDataAndGetResponseAsync< T >( command, jsonContent, token, shouldGetExceptionMessage, operationTimeout ).GetAwaiter().GetResult();
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
			
			var response = await this.PostRawDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout );
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
			while( numberRequest < MaxPostDataAttemptsCount )
			{
				numberRequest++;
				var data = this.PostRawDataAsync( url, jsonContent, token, shouldGetExceptionMessage, operationTimeout, useShipStationPartnerHeader: true ).Result;
				if ( !string.IsNullOrWhiteSpace( data ) )
					return this.ParseResponse< T >( data );
			}

			throw new Exception( $"More {MaxPostDataAttemptsCount} attempts" );
		}

		/// <summary>
		///	Get data from ShipStation's endpoint async
		/// </summary>
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

				this.RefreshLastNetworkActivityTime();

				var response = await this.HttpClient.GetAsync( url, cts.Token ).ConfigureAwait( false );
				var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait( false );

				this.RefreshLastNetworkActivityTime();
				this.ThrowIfError( url, response, responseContent );

				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Response for url '{Uri}' with timeout '{OperationTimeout}': {Response}", 
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(WebRequestServices),
					nameof(this.GetRawResponseAsync),
					url, GetOperationTimeout( operationTimeout ), responseContent );

				return responseContent;
			}
		}

		/// <summary>
		///	Post data to ShipStation's API endpoint async
		/// </summary>
		/// <param name="url"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <param name="shouldGetExceptionMessage"></param>
		/// <param name="operationTimeout"></param>
		/// <returns></returns>
		private async Task< string > PostRawDataAsync( string url, string payload, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null, bool useShipStationPartnerHeader = false )
		{
			this.LogPostInfo( url, payload, operationTimeout );
			this.RefreshLastNetworkActivityTime();

			try
			{
				using( var cts = CancellationTokenSource.CreateLinkedTokenSource( token ) )
				{
					if ( operationTimeout != null )
						cts.CancelAfter( operationTimeout.Value * 1000 );

					if ( useShipStationPartnerHeader )
						this.SetAuthorizationHeader( true );

					var requestContent = new StringContent( payload, Encoding.UTF8, "application/json" );

					var responseMessage = await this.HttpClient.PostAsync( url, requestContent, cts.Token ).ConfigureAwait( false );
					var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait( false );

					this.RefreshLastNetworkActivityTime();
					this.ThrowIfError( url, responseMessage, responseContent );

					this.LogUpdateInfo( url, responseMessage.StatusCode, payload, operationTimeout );
					return responseContent;
				}
			}
			catch( Exception ex )
			{
				this.RefreshLastNetworkActivityTime();
				var webException = WebExtensions.GetWebException( ex );
				if ( webException != null )
				{
					var serverResponseError = webException.Response?.GetResponseString() ?? string.Empty;
					this.LogPostError( url, webException.Response?.GetHttpStatusCode() ?? HttpStatusCode.InternalServerError, payload, serverResponseError, operationTimeout );
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
				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Request to '{Url}' returned HTTP Error with the response content: '{ResponseContent}'. Request Headers: {@RequestMessageHeaders}, Response Headers: {@ResponseMessageHeaders}",
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(WebRequestServices),
					nameof(this.ThrowIfError),
					url, responseContent, responseMessage?.RequestMessage?.Headers, responseMessage?.Headers );
				throw new ShipStationUnauthorizedException();
			}

			if( !this.IsRequestThrottled( responseMessage, responseContent, out var rateResetInSeconds ) )
				return;

			ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Response indicates throttling. Url '{Uri}':\nRateResetInSeconds: {ResetInSeconds}\nResponse: {Response}",
				Constants.ChannelName,
				Constants.VersionInfo,
				this._syncRunContext.TenantId,
				this._syncRunContext.ChannelAccountId,
				this._syncRunContext.CorrelationId,
				nameof(WebRequestServices),
				nameof(this.ThrowIfError),
				url, rateResetInSeconds, responseContent );
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

		private void LogUpdateInfo( string url, HttpStatusCode statusCode, string jsonContent, int? operationTimeout )
		{
			ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "POSTing call for the url '{Url}' with timeout '{OperationTimeout}' has been completed with code '{Code}'.\n{content}",
				Constants.ChannelName,
				Constants.VersionInfo, 
				this._syncRunContext.TenantId,
				this._syncRunContext.ChannelAccountId,
				this._syncRunContext.CorrelationId,
				nameof(WebRequestServices),
				nameof(this.LogUpdateInfo),
				url, GetOperationTimeout( operationTimeout ), Convert.ToInt32( statusCode ), jsonContent );
		}

		private void LogPostInfo( string url, string jsonContent, int? operationTimeout )
		{
			ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "POSTed data for the url '{Url}' with timeout '{OperationTimeout}':\n{JsonContent}", 
				Constants.ChannelName,
				Constants.VersionInfo, 
				this._syncRunContext.TenantId,
				this._syncRunContext.ChannelAccountId,
				this._syncRunContext.CorrelationId,
				nameof(WebRequestServices),
				nameof(this.LogPostInfo),
				url, GetOperationTimeout( operationTimeout ), jsonContent );
		}

		private void LogPostError( string url, HttpStatusCode statusCode, string jsonContent, string responseString, int? operationTimeout )
		{
			ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "ERROR POSTing data for the  url '{Url}', timeout '{OperationTimeout}', Error '{ErrorCode}' '{ErrorMessage}'. Request ':\n{Content}",
				Constants.ChannelName,
				Constants.VersionInfo, 
				this._syncRunContext.TenantId,
				this._syncRunContext.ChannelAccountId,
				this._syncRunContext.CorrelationId,
				nameof(WebRequestServices),
				nameof(this.LogPostError),
				url, GetOperationTimeout( operationTimeout ), Convert.ToInt32( statusCode ), responseString, jsonContent );
		}

		/// <summary>
		///	This method is used to update service's last network activity time.
		///	It's called every time before making API request to server or after handling the response.
		/// </summary>
		private void RefreshLastNetworkActivityTime()
		{
			this.LastNetworkActivityTime = DateTime.UtcNow;
		}

		private static int GetOperationTimeout( int? operationTimeout ) => operationTimeout ?? MaxHttpRequestTimeoutInMinutes * 60 * 1000;
		#endregion
	}
}