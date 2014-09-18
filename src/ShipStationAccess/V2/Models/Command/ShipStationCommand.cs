namespace ShipStationAccess.V2.Models.Command
{
	internal sealed class ShipStationCommand
	{
		public static readonly ShipStationCommand Unknown = new ShipStationCommand( string.Empty );
		public static readonly ShipStationCommand GetOrders = new ShipStationCommand( "/Orders/List" );
		public static readonly ShipStationCommand CreateUpdateOrder = new ShipStationCommand( "/Orders/CreateOrder" );
		public static readonly ShipStationCommand GetStores = new ShipStationCommand( "/Stores" );

		private ShipStationCommand( string command )
		{
			this.Command = command;
		}

		public string Command{ get; private set; }
	}
}