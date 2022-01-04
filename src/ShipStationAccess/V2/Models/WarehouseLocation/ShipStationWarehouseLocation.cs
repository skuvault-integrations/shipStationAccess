using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.WarehouseLocation
{
	[ DataContract ]
	public sealed class ShipStationWarehouseLocation
	{
		[ DataMember( Name = "warehouseLocation" ) ]
		public string WarehouseLocation{ get; set; }

		[ DataMember( Name = "orderItemIds" ) ]
		public List< long > OrderItemIds{ get; set; }
	}
}