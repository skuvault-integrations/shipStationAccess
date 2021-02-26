namespace ShipStationAccess.V2.Models.Command
{
	public sealed class ShipStationCommand
	{
		public static readonly ShipStationCommand Unknown = new ShipStationCommand( string.Empty );
		public static readonly ShipStationCommand GetOrders = new ShipStationCommand( "/Orders/List" );
		public static readonly ShipStationCommand GetOrder = new ShipStationCommand( "/Orders" );
		public static readonly ShipStationCommand GetTags = new ShipStationCommand( "/Accounts/ListTags" );
		public static readonly ShipStationCommand CreateUpdateOrder = new ShipStationCommand( "/Orders/CreateOrder" );
		public static readonly ShipStationCommand UpdateOrderItemsWarehouseLocation = new ShipStationCommand( "/Orders/UpdateWarehouseLocation" );
		public static readonly ShipStationCommand GetStores = new ShipStationCommand( "/Stores" );
		public static readonly ShipStationCommand GetShippingLabel = new ShipStationCommand( "/Orders/CreateLabelForOrder" );
		public static readonly ShipStationCommand Register = new ShipStationCommand( "/Integratedapp/Register" );
		public static readonly ShipStationCommand GetOrderShipments = new ShipStationCommand( "/Shipments" );
		public static readonly ShipStationCommand GetOrderFulfillments = new ShipStationCommand( "/Fulfillments" );

		private ShipStationCommand( string command )
		{
			this.Command = command;
		}

		public string Command{ get; private set; }
	}
}