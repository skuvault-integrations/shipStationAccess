using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;
using Netco.Logging;
using Netco.Utils;

namespace ShipStationAccess.V2.Misc
{
	public static class ActionPolicies
	{
		public static ActionPolicy Submit
		{
			get { return _shipStationSumbitPolicy; }
		}

		private static readonly ActionPolicy _shipStationSumbitPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			typeof( ActionPolicies ).Log().Trace( ex, "Retrying ShipStation API submit call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync SubmitAsync
		{
			get { return _shipStationSumbitAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationSumbitAsyncPolicy = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			typeof( ActionPolicies ).Log().Trace( ex, "Retrying ShipStation API submit call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicy Get
		{
			get { return _shipStationGetPolicy; }
		}

		private static readonly ActionPolicy _shipStationGetPolicy = ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
		{
			typeof( ActionPolicies ).Log().Trace( ex, "Retrying ShipStation API get call for the {0} time", i );
			SystemUtil.Sleep( TimeSpan.FromSeconds( 0.5 + i ) );
		} );

		public static ActionPolicyAsync GetAsync
		{
			get { return _shipStationGetAsyncPolicy; }
		}

		private static readonly ActionPolicyAsync _shipStationGetAsyncPolicy = ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
		{
			typeof( ActionPolicies ).Log().Trace( ex, "Retrying ShipStation API get call for the {0} time", i );
			await Task.Delay( TimeSpan.FromSeconds( 0.5 + i ) );
		} );
	}
}