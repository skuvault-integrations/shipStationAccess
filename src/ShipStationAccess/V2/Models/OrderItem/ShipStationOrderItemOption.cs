using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.OrderItem
{
	[ DataContract ]
	public sealed class ShipStationOrderItemOption
	{
		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "value" ) ]
		public string Value{ get; set; }
	}
}