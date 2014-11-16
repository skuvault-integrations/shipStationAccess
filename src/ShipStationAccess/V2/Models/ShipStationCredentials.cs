using CuttingEdge.Conditions;

namespace ShipStationAccess.V2.Models
{
	public sealed class ShipStationCredentials
	{
		public string ApiKey{ get; private set; }
		public string ApiSecret{ get; private set; }
		public string MashapeKey{ get; private set; }
		public string Host{ get; private set; }
		public string PartnerKey{ get; private set; }

		public ShipStationCredentials( string apiKey, string apiSecret, string partnerKey )
		{
			Condition.Requires( apiKey, "apiKey" ).IsNotNullOrWhiteSpace();
			Condition.Requires( apiSecret, "ApiSecret" ).IsNotNullOrWhiteSpace();

			this.ApiKey = apiKey;
			this.ApiSecret = apiSecret;
			this.MashapeKey = "mtoHg8NaKLmshIo69qYeENy89Rtdp1yhs7RjsnuVTdqEW0hlIP";
			this.Host = "https://shipstation.p.mashape.com";
			this.PartnerKey = partnerKey;
		}
	}
}