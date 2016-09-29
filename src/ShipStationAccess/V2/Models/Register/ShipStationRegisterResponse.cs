using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Register
{
	[ DataContract ]
	public sealed class ShipStationRegisterResponse
	{
		[ DataMember( Name = "sellerIntegrationId" ) ]
		public long SellerIntegrationId{ get; set; }
	}
}
