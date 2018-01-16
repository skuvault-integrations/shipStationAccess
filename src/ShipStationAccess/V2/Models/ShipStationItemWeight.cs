using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models
{
	[ DataContract ]
	public sealed class ShipStationItemWeight
	{
		[ DataMember( Name = "value" ) ]
		public decimal Value{ get; set; }

		[ DataMember( Name = "units" ) ]
		public string Units{ get; set; }
		public ShipStationItemWeight( decimal value, string units )
		{
			this.Value = value;
			this.Units = units;
		}
	}
}