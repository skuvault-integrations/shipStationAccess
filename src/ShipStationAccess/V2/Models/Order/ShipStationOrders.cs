using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrders
	{
		[ DataMember( Name = "orders" ) ]
		public IList< ShipStationOrder > Orders{ get; set; }

		[ DataMember( Name = "total" ) ]
		public int TotalOrders{ get; set; }

		[ DataMember( Name = "page" ) ]
		public int CurrentPageNumber{ get; set; }

		[ DataMember( Name = "pages" ) ]
		public int TotalPages{ get; set; }

		public ShipStationOrders()
		{
			this.Orders = new List< ShipStationOrder >();
		}
	}
}