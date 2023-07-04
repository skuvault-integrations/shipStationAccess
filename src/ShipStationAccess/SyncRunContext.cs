using System;
using System.Collections.Generic;
using System.Linq;

namespace ShipStationAccess
{
	// ToDo: TD-185 Migrate ShipStation integration to .NET Standard
	// Need to remove this class and get it from the SkuVault.Integrations.Core
	// https://github.com/skuvault-integrations/integration-core/blob/master/src/SkuVault.Integrations.Core/Common/SyncRunContext.cs
	/// <summary>
	/// This is used to pass tenantId and ChannelAccountId from v1 into access, to then be used as keys for logging
	/// </summary>
	public class SyncRunContext
	{
		public long TenantId{ get; }
		public long? ChannelAccountId{ get; }
		public string CorrelationId{ get; }

		public SyncRunContext( long tenantId, long? channelAccountId, string correlationId )
		{
			TenantId = tenantId;
			ChannelAccountId = channelAccountId;
			CorrelationId = correlationId;
			ValidationHelper.ThrowOnValidationErrors< SyncRunContext >( GetValidationErrors() );
		}

		private IEnumerable< string > GetValidationErrors()
		{
			var validationErrors = new List< string >();
			if( TenantId == default )
			{
				validationErrors.Add( $"{nameof(TenantId)} is 0" );
			}

			if( string.IsNullOrWhiteSpace( CorrelationId ) )
			{
				validationErrors.Add( $"{nameof(CorrelationId)} is empty" );
			}

			return validationErrors;
		}

		/// <summary>
		/// Performs validation related actions on elements registered for validation
		/// </summary>
		private static class ValidationHelper
		{
			/// <summary>
			/// If error list contains error, throws <see cref="ArgumentException"/> with these errors
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="validationErrors"></param>
			/// <exception cref="ArgumentException"></exception>
			public static void ThrowOnValidationErrors< T >( IEnumerable< string > validationErrors )
			{
				validationErrors = validationErrors as List< string > ?? validationErrors.ToList();
				if( validationErrors.Any() )
				{
					throw new ArgumentException( string.Join( "; ", validationErrors ), nameof(T) );
				}
			}
		}
	}
}