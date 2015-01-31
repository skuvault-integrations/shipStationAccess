using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderInternationalOptions
	{
		[ DataMember( Name = "contents" ) ]
		public string Contents{ get; set; }

		[ DataMember( Name = "customsItems" ) ]
		public ShipStationCustomItems CustomsItems{ get; set; }
	}
}