using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderShipments
	{
		[ DataMember( Name = "shipments" ) ]
		public IEnumerable< ShipStationOrderShipment > Shipments { get; set; }

		[ DataMember( Name = "total" ) ]
		public int Total { get; set; }

		[ DataMember( Name = "page" ) ]
		public int Page { get; set; }

		[ DataMember( Name = "pages" ) ]
		public int Pages { get; set; }
	}

	[ DataContract ]
	public sealed class ShipStationOrderShipment
	{
		[ DataMember( Name = "shipmentId" ) ]
		public long Id { get; set; }

		[ DataMember( Name = "orderId" ) ]
		public long OrderId { get; set; } 

		[ DataMember( Name = "createDate" ) ]
		public DateTime CreateDate { get; set; }

		[ DataMember( Name = "shipDate" ) ]
		public DateTime? ShipDate { get; set; }

		[ DataMember( Name = "shipmentCost" ) ]
		public decimal Cost { get; set; }

		[ DataMember( Name = "insuranceCost" ) ]
		public decimal InsuranceCost { get; set; }

		[ DataMember( Name = "trackingNumber" ) ]
		public string TrackingNumber { get; set; }

		[ DataMember( Name = "carrierCode" ) ]
		public string CarrierCode { get; set; }

		[ DataMember( Name = "serviceCode" ) ]
		public string ServiceCode { get; set; }

		[ DataMember( Name = "confirmation" ) ]
		public string Confirmation { get; set; }

		[ DataMember( Name = "voided" ) ]
		public bool Voided { get; set; }

		[ DataMember( Name = "weight" ) ]
		public ShipmentWeight Weight { get; set; }

		[ DataMember( Name = "dimensions" ) ]
		public ShipmentDimensions Dimensions { get; set; }

		[ DataMember( Name = "shipmentItems" ) ]
		public IEnumerable< ShipmentItem > Items { get; set; }
	}

	[ DataContract ]
	public sealed class ShipmentWeight
	{
		[ DataMember( Name = "value" ) ]
		public decimal Value { get; set; }

		[ DataMember( Name = "units" ) ]
		public string Units { get; set; }
	}

	[ DataContract ]
	public sealed class ShipmentDimensions
	{
		[ DataMember( Name = "units" ) ]
		public string Units { get; set; }

		[ DataMember( Name = "length" ) ]
		public decimal Length { get; set; }

		[ DataMember( Name = "width" ) ]
		public decimal Width { get; set; }

		[ DataMember( Name = "height" ) ]
		public decimal Height { get; set; }
	}

	[ DataContract ]
	public sealed class ShipmentItem
	{
		[ DataMember( Name = "sku" ) ]
		public string Sku { get; set; }

		[ DataMember( Name = "quantity" ) ]
		public int Quantity { get; set; }
	}
}