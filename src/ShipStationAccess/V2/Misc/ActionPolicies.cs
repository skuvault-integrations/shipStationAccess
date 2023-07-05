using System;
using System.Net;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Utils;
using ShipStationAccess.V2.Exceptions;

namespace ShipStationAccess.V2.Misc
{
	internal static class ActionPolicies
	{
		private const int MaxRetryAttempts = 10;

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

		public static ActionPolicy Submit => _shipStationSubmitPolicy;

		private static readonly ActionPolicy _shipStationSubmitPolicy = ActionPolicy.With( _exceptionHandler ).Retry( MaxRetryAttempts, ( ex, i ) =>
		{
			var delay = GetDelay( i, ex );
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {RetryCounter} time, delay {DelayInSeconds} seconds", i, delay.TotalSeconds );
			SystemUtil.Sleep( delay );
		} );

		public static ActionPolicyAsync SubmitAsync => _shipStationSubmitAsyncPolicy;

		private static readonly ActionPolicyAsync _shipStationSubmitAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( MaxRetryAttempts, async ( ex, i ) =>
		{
			var delay = GetDelay( i, ex );
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API submit call for the {RetryCounter} time, delay {DelayInSeconds} seconds", i, delay.TotalSeconds );
			await Task.Delay( delay );
		} );

		public static ActionPolicy Get => _shipStationGetPolicy;

		private static readonly ActionPolicy _shipStationGetPolicy = ActionPolicy.With( _exceptionHandler ).Retry( MaxRetryAttempts, ( ex, i ) =>
		{
			var delay = GetDelay( i, ex );
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {RetryCounter} time, delay {DelayInSeconds} seconds", i, delay.TotalSeconds );
			SystemUtil.Sleep( delay );
		} );

		public static ActionPolicyAsync GetAsync => _shipStationGetAsyncPolicy;

		private static readonly ActionPolicyAsync _shipStationGetAsyncPolicy = ActionPolicyAsync.With( _exceptionHandler ).RetryAsync( MaxRetryAttempts, async ( ex, i ) =>
		{
			var delay = GetDelay( i, ex );
			ShipStationLogger.Log.Error( ex, "Retrying ShipStation API get call for the {RetryCounter} time, delay {DelayInSeconds} seconds", i, delay.TotalSeconds );
			await Task.Delay( delay );
		} );

		private static TimeSpan GetDelay( int i, Exception ex )
		{
			var delay = TimeSpan.FromSeconds( 0.5 + i );
			if( TryGetDelayFromException( ex, out int rateLimitReset ) )
			{
				delay = TimeSpan.FromSeconds( rateLimitReset );
			}

			return delay;
		}

		private static bool TryGetDelayFromException( Exception ex, out int rateLimitReset )
		{
			rateLimitReset = 0;
			if ( ex is ShipStationThrottleException shipStationThrottleException )
			{
				rateLimitReset = shipStationThrottleException.ResetInSeconds;
				return true;
			}

			return false;
		}
	}
}