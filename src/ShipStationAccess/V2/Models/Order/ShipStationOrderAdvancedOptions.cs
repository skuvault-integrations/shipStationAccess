using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderAdvancedOptions
	{
		[ DataMember( Name = "warehouseId" ) ]
		public long WarehouseId{ get; set; }

		[ DataMember( Name = "nonMachinable" ) ]
		public bool NonMachinable{ get; set; }

		[ DataMember( Name = "saturdayDelivery" ) ]
		public bool SaturdayDelivery{ get; set; }

		[ DataMember( Name = "containsAlcohol" ) ]
		public bool ContainsAlcohol{ get; set; }

		[ DataMember( Name = "storeId" ) ]
		public long StoreId{ get; set; }

		[ DataMember( Name = "customField1" ) ]
		public string CustomField1{ get; set; }

		[ DataMember( Name = "customField2" ) ]
		public string CustomField2{ get; set; }

		[ DataMember( Name = "customField3" ) ]
		public string CustomField3{ get; set; }

		[ DataMember( Name = "source" ) ]
		public string Source{ get; set; }

		[ DataMember( Name = "billToParty" ) ]
		public string BillToParty{ get; set; }

		[ DataMember( Name = "billToAccount" ) ]
		public string BillToAccount{ get; set; }

		[ DataMember( Name = "billToPostalCode" ) ]
		public string BillToPostalCode{ get; set; }

		[ DataMember( Name = "billToCountryCode" ) ]
		public string BillToCountryCode{ get; set; }
	}
}