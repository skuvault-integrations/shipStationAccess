using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationCustomItems
	{
		[ DataMember( Name = "description" ) ]
		public string Description{ get; set; }

		[ DataMember( Name = "quantity" ) ]
		public int Quantity{ get; set; }

		[ DataMember( Name = "value" ) ]
		public double Value{ get; set; }

		[ DataMember( Name = "harmonizedTariffCode" ) ]
		public string HarmonizedTariffCode{ get; set; }

		[ DataMember( Name = "countryOfOrigin" ) ]
		public string CountryOfOrigin{ get; set; }
	}
}