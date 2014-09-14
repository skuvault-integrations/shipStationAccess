using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	internal sealed class ShipStationOrders
	{
		[ DataMember( Name = "orders" ) ]
		public IList< ShipStationOrder > Orders{ get; set; }

		public ShipStationOrders()
		{
			this.Orders = new List< ShipStationOrder >();
		}
	}
}