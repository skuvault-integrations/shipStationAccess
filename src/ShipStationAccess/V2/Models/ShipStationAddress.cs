using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models
{
	[ DataContract ]
	public sealed class ShipStationAddress
	{
		[ DataMember( Name = "name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "company" ) ]
		public string Company{ get; set; }

		[ DataMember( Name = "street1" ) ]
		public string Street1{ get; set; }

		[ DataMember( Name = "street2" ) ]
		public string Street2{ get; set; }

		[ DataMember( Name = "street3" ) ]
		public string Street3{ get; set; }

		[ DataMember( Name = "city" ) ]
		public string City{ get; set; }

		[ DataMember( Name = "state" ) ]
		public string State{ get; set; }

		[ DataMember( Name = "postalCode" ) ]
		public string PostalCode{ get; set; }

		[ DataMember( Name = "country" ) ]
		public string Country{ get; set; }

		[ DataMember( Name = "phone" ) ]
		public string Phone{ get; set; }

		[ DataMember( Name = "residential" ) ]
		public bool Residential{ get; set; }
	}
}