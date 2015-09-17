namespace ShipStationAccess.V2.Models.Command
{
	internal sealed class ShipStationParam
	{
		public static readonly ShipStationParam Unknown = new ShipStationParam( string.Empty );
		public static readonly ShipStationParam OrdersModifiedDateFrom = new ShipStationParam( "modifyDateStart" );
		public static readonly ShipStationParam OrdersModifiedDateTo = new ShipStationParam( "modifyDateEnd" );
		public static readonly ShipStationParam OrdersCreatedDateFrom = new ShipStationParam( "createDateStart" );
		public static readonly ShipStationParam OrdersCreatedDateTo = new ShipStationParam( "createDateEnd" );
		public static readonly ShipStationParam OrdersStatus = new ShipStationParam( "orderStatus" );
		public static readonly ShipStationParam PageSize = new ShipStationParam( "pageSize" );
		public static readonly ShipStationParam Page = new ShipStationParam( "page" );
		public static readonly ShipStationParam OrderId = new ShipStationParam( "orderId" );
		public static readonly ShipStationParam CarrierCode = new ShipStationParam( "carrierCode" );
		public static readonly ShipStationParam ServiceCode = new ShipStationParam( "serviceCode" );
		public static readonly ShipStationParam Confirmation = new ShipStationParam( "confirmation" );
		public static readonly ShipStationParam ShipDate = new ShipStationParam( "shipDate" );


		private ShipStationParam( string name )
		{
			this.Name = name;
		}

		public string Name{ get; private set; }
	}
}