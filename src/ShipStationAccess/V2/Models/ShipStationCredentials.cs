using System;

namespace ShipStationAccess.V2.Models
{
	public sealed class ShipStationCredentials
	{
		public string ApiKey{ get; private set; }
		public string ApiSecret{ get; private set; }
		public string Host{ get; private set; }
		public string PartnerKey{ get; internal set; }

		public ShipStationCredentials( string apiKey, string apiSecret, string partnerKey = null )
		{
			if( string.IsNullOrWhiteSpace( apiKey ) )
			{
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( apiKey ) );
			}

			if( string.IsNullOrWhiteSpace( apiSecret ) )
			{
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( apiSecret ) );
			}

			this.ApiKey = apiKey;
			this.ApiSecret = apiSecret;
			this.PartnerKey = partnerKey;
			this.Host = "https://ssapi.shipstation.com";
		}
	}
}