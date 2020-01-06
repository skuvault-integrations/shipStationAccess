using System;
using System.Net;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;
using ShipStationAccess.V2.Exceptions;

namespace ShipStationAccess.V2.Misc
{
	public static class ActionPolicies
	{
		public static ActionPolicy Submit
		{
			get { return _shipStationSumbitPolicy; }
		}

		private static readonly ExceptionHandler _exceptionHandler = delegate( Exception x )
		{
			if( x is ShipStationLabelException )
				return false;
			var webX = x as WebException;
			if( webX == null )
				return true;
			return webX.Response.GetHttpStatusCode() != HttpStatusCode.Unauthorized;
		};

		private static readonly ActionPolicy _shipStationSumbitPolicy = ActionPolicy.With( _exceptionHandler ).Retry( 10, ( ex, i ) =>
		{
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {retryCounter} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync SubmitAsync
		{
			get { return _shipStationSumbitAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationSumbitAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( 10, async ( ex, i ) =>
		{
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {retryCounter} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicy Get
		{
			get { return _shipStationGetPolicy; }
		}

		private static readonly ActionPolicy _shipStationGetPolicy = ActionPolicy.With( _exceptionHandler ).Retry( 10, ( ex, i ) =>
		{
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {retryCounter} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync GetAsync
		{
			get { return _shipStationGetAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationGetAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( 10, async ( ex, i ) =>
		{
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {retryCounter} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );
	}
}