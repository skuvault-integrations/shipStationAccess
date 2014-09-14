using CuttingEdge.Conditions;

namespace ShipStationAccess.V1.Models
{
	public class ShipStationCredentials
	{
		public string UserName{ get; private set; }
		public string Password{ get; private set; }

		public ShipStationCredentials( string userName, string password )
		{
			Condition.Requires( userName, "userName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( password, "password" ).IsNotNullOrWhiteSpace();

			this.UserName = userName;
			this.Password = password;
		}
	}
}