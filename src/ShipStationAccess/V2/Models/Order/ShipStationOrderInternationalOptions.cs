using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderInternationalOptions
	{
		[ DataMember( Name = "contents" ) ]
		public string Contents{ get; set; }

		[ DataMember( Name = "customsItems" ) ]
		public IList< ShipStationCustomItems > CustomsItems{ get; set; }

		[ DataMember( Name = "nonDelivery" ) ]
		public string NonDelivery{ get; set; }
	}
}