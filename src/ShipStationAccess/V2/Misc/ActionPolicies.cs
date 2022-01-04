using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;
using ShipStationAccess.V2.Exceptions;

namespace ShipStationAccess.V2.Misc
{
	public static class ActionPolicies
	{
		public const int MaxRetryAttempts = 10;

		public static ActionPolicy Submit
		{
			get { return _shipStationSubmitPolicy; }
		}

		/// <summary>
		///	Returns true if request with provided exception should be retried
		/// </summary>
		private static readonly ExceptionHandler _exceptionHandler = delegate( Exception x )
		{
			if( x is ShipStationUnrecoverableException
				|| x is TaskCanceledException )
				return false;
			var webX = x as WebException;
			if( webX?.Response == null )
				return true;
			return webX.Response.GetHttpStatusCode() != HttpStatusCode.Unauthorized;
		};

		private static readonly ActionPolicy _shipStationSubmitPolicy = ActionPolicy.With( _exceptionHandler ).Retry( MaxRetryAttempts, ( ex, i ) =>
		{
			var delay = TimeSpan.FromSeconds( 0.5 + i );
			if ( TryGetDelayFromException( ex, out int rateLimitReset ) )
			{
				delay = TimeSpan.FromSeconds( rateLimitReset );
			}
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {retryCounter} time, delay {delayInSeconds} seconds", i, delay.TotalSeconds );
			SystemUtil.Sleep( delay );
		} );

		public static ActionPolicyAsync SubmitAsync
		{
			get { return _shipStationSubmitAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationSubmitAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( MaxRetryAttempts, async ( ex, i ) =>
		{
			var delay = TimeSpan.FromSeconds( 0.5 + i );
			if ( TryGetDelayFromException( ex, out int rateLimitReset ) )
			{
				delay = TimeSpan.FromSeconds( rateLimitReset );
			}
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {retryCounter} time, delay {delayInSeconds} seconds", i, delay.TotalSeconds );
			await Task.Delay( delay );
		} );

		public static ActionPolicy Get
		{
			get { return _shipStationGetPolicy; }
		}

		private static readonly ActionPolicy _shipStationGetPolicy = ActionPolicy.With( _exceptionHandler ).Retry( MaxRetryAttempts, ( ex, i ) =>
		{
			var delay = TimeSpan.FromSeconds( 0.5 + i );
			if ( TryGetDelayFromException( ex, out int rateLimitReset ) )
			{
				delay = TimeSpan.FromSeconds( rateLimitReset );
			}
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {retryCounter} time, delay {delayInSeconds} seconds", i, delay.TotalSeconds );
			SystemUtil.Sleep( delay );
		} );

		public static ActionPolicyAsync GetAsync
		{
			get { return _shipStationGetAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationGetAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( MaxRetryAttempts, async ( ex, i ) =>
		{
			var delay = TimeSpan.FromSeconds( 0.5 + i );
			if ( TryGetDelayFromException( ex, out int rateLimitReset ) )
			{
				delay = TimeSpan.FromSeconds( rateLimitReset );
			}

			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {retryCounter} time, delay {delayInSeconds} seconds", i, delay.TotalSeconds );
			await Task.Delay( delay );
		} );

		private static bool TryGetDelayFromException( Exception ex, out int rateLimitReset )
		{
			rateLimitReset = 0;
			if ( ex is ShipStationThrottleException )
			{
				rateLimitReset = ((ShipStationThrottleException)ex ).ResetInSeconds;
				return true;
			}

			return false;
		}
	}
}