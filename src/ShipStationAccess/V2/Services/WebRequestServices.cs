using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;
using ServiceStack;
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
			T result;
			var request = this.CreateGetServiceRequest( string.Concat( this._credentials.Host, command.Command, commandParams ) );
			using( var response = request.GetResponse() )
				result = ParseResponse< T >( response );
			return result;
		}

		public async Task< T > GetResponseAsync< T >( ShipStationCommand command, string commandParams )
		{
			T result;
			var request = this.CreateGetServiceRequest( string.Concat( this._credentials.Host, command.Command, commandParams ) );
			using( var response = await request.GetResponseAsync() )
				result = ParseResponse< T >( response );
			return result;
		}

		public void PostData( ShipStationCommand command, string jsonContent )
		{
			var request = this.CreateServicePostRequest( command, jsonContent );
				using( var response = ( HttpWebResponse )request.GetResponse() )
					this.LogUpdateInfo( request.RequestUri.AbsoluteUri, response.StatusCode, jsonContent );
		}

		public async Task PostDataAsync( ShipStationCommand command, string jsonContent )
		{
			var request = this.CreateServicePostRequest( command, jsonContent );
			using( var response = ( HttpWebResponse )await request.GetResponseAsync() )
				this.LogUpdateInfo( request.RequestUri.AbsoluteUri, response.StatusCode, jsonContent );
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
			request.Headers.Add( "X-Mashape-Key", this._credentials.MashapeKey );
			request.Headers.Add( "x-shipstation-partner", this._credentials.PartnerKey );
		}

		private string CreateAuthenticationHeader()
		{
			var authInfo = string.Concat( this._credentials.ApiKey, ":", this._credentials.ApiSecret );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private T ParseResponse< T >( WebResponse response )
		{
			var result = default( T );

			using( var stream = response.GetResponseStream() )
			{
				var reader = new StreamReader( stream );
				var jsonResponse = reader.ReadToEnd();

				this.Log().Trace( "[shipstation]\tResponse\t{0} - {1}", response.ResponseUri, jsonResponse );

				if( !String.IsNullOrEmpty( jsonResponse ) )
					result = jsonResponse.FromJson< T >();
			}

			return result;
		}

		private void LogUpdateInfo( string url, HttpStatusCode statusCode, string jsonContent )
		{
			this.Log().Trace( "[shipstation]\tPOST call for the url '{0}' has been completed with code '{1}'.\n{2}", url, statusCode, jsonContent );
		}
		#endregion
	}
}