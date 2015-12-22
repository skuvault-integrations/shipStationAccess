using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Store
{
	[ DataContract ]
	public class ShipStationStore
	{
		[ DataMember( Name = "storeId" ) ]
		public long StoreId{ get; set; }

		[ DataMember( Name = "marketplaceId" ) ]
		public int MarketplaceId{ get; set; }

		[ DataMember( Name = "marketplaceName" ) ]
		public string MarketplaceName{ get; set; }
	}
}