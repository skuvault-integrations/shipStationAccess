using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.OrderItem
{
	[ DataContract ]
	public sealed class ShipStationOrderItem
	{
		[ DataMember( Name = "lineItemKey" ) ]
		public string LineItemKey{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "sku" ) ]
		public string Sku{ get; set; }

		[ DataMember( Name = "imageUrl" ) ]
		public string ImageUrl{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public ShipStationItemWeight Weight{ get; set; }

		[ DataMember( Name = "quantity" ) ]
		public int Quantity{ get; set; }

		[ DataMember( Name = "unitPrice" ) ]
		public decimal UnitPrice{ get; set; }

		[ DataMember( Name = "warehouseLocation" ) ]
		public string WarehouseLocation{ get; set; }

		[ DataMember( Name = "options" ) ]
		public IList< ShipStationOrderItemOption > Options{ get; set; }
	}
}