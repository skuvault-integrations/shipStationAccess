using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderDimensions
	{
		[ DataMember( Name = "units" ) ]
		public string units{ get; set; }

		[ DataMember( Name = "length" ) ]
		public decimal Length{ get; set; }

		[ DataMember( Name = "width" ) ]
		public decimal Width{ get; set; }

		[ DataMember( Name = "height" ) ]
		public decimal Height{ get; set; }
	}
}