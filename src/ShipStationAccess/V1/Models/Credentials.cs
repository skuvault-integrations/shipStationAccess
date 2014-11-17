using CuttingEdge.Conditions;

namespace ShipStationAccess.V1.Models
{
	public class ShipStationCredentials
	{
		public string UserName{ get; private set; }
		public string Password{ get; private set; }
		public string PartnerKey{ get; private set; }

		public ShipStationCredentials( string userName, string password, string partnerKey )
		{
			Condition.Requires( userName, "userName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( password, "password" ).IsNotNullOrWhiteSpace();

			this.UserName = userName;
			this.Password = password;
			this.PartnerKey = partnerKey;
		}
	}
}