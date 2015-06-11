using CuttingEdge.Conditions;

namespace ShipStationAccess.V2.Models
{
	public sealed class ShipStationCredentials
	{
		public string ApiKey{ get; private set; }
		public string ApiSecret{ get; private set; }
		public string Host{ get; private set; }

		public ShipStationCredentials( string apiKey, string apiSecret )
		{
			Condition.Requires( apiKey, "apiKey" ).IsNotNullOrWhiteSpace();
			Condition.Requires( apiSecret, "ApiSecret" ).IsNotNullOrWhiteSpace();

			this.ApiKey = apiKey;
			this.ApiSecret = apiSecret;
			this.Host = "https://ssapi.shipstation.com";
		}
	}
}