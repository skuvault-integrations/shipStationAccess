namespace ShipStationAccess.V2.Models.Command
{
	internal sealed class ShipStationParam
	{
		public static readonly ShipStationParam Unknown = new ShipStationParam( string.Empty );
		public static readonly ShipStationParam OrdersModifiedDateFrom = new ShipStationParam( "modifyDateStart" );
		public static readonly ShipStationParam OrdersModifiedDateTo = new ShipStationParam( "modifyDateEnd" );
		public static readonly ShipStationParam OrdersCreatedDateFrom = new ShipStationParam( "orderDateStart" );
		public static readonly ShipStationParam OrdersCreatedDateTo = new ShipStationParam( "orderDateEnd" );
		public static readonly ShipStationParam PageSize = new ShipStationParam( "pageSize" );
		public static readonly ShipStationParam Page = new ShipStationParam( "page" );

		private ShipStationParam( string name )
		{
			this.Name = name;
		}

		public string Name{ get; private set; }
	}
}