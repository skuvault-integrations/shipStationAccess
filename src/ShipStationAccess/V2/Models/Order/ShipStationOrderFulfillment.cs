using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderFulfillments
	{
		[ DataMember( Name = "fulfillments" ) ]
		public IEnumerable< ShipStationOrderFulfillment > Fulfillments { get; set; }

		[ DataMember( Name = "total" ) ]
		public int Total { get; set; }

		[ DataMember( Name = "page" ) ]
		public int Page { get; set; }

		[ DataMember( Name = "pages" ) ]
		public int Pages { get; set; }
	}

	[ DataContract ]
	public sealed class ShipStationOrderFulfillment
	{
		[ DataMember( Name = "fulfillmentId" ) ]
		public long Id { get; set; }

		[ DataMember( Name = "orderId" ) ]
		public long OrderId { get; set; }

		[ DataMember( Name = "trackingNumber" ) ]
		public string TrackingNumber { get; set; }

		[ DataMember( Name = "createDate" ) ]
		public DateTime CreateDate { get; set; }

		[ DataMember( Name = "shipDate" ) ]
		public DateTime ShipDate { get; set; }

		[ DataMember( Name = "deliveryDate" ) ]
		public DateTime? DeliveryDate { get; set; }

		[ DataMember( Name = "carrierCode" ) ]
		public string CarrierCode { get; set; }

		[ DataMember( Name = "voided" ) ]
		public bool Voided { get; set; }
	}
}