using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ShipStationAccess.V2.Models.OrderItem;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrder
	{
		[ DataMember( Name = "orderId" ) ]
		public long OrderId{ get; set; }

		[ DataMember( Name = "orderNumber" ) ]
		public string OrderNumber{ get; set; }

		[ DataMember( Name = "orderKey" ) ]
		public string OrderKey{ get; set; }

		[ DataMember( Name = "orderDate" ) ]
		public DateTime OrderDate{ get; set; }

		[ DataMember( Name = "paymentDate" ) ]
		public DateTime? PaymentDate{ get; set; }

		[ DataMember( Name = "shipDate" ) ]
		public DateTime? ShipDate{ get; set; }

		[ DataMember( Name = "amountPaid" ) ]
		public decimal AmountPaid{ get; set; }

		[ DataMember( Name = "orderStatus" ) ]
		public ShipStationOrderStatusEnum OrderStatus{ get; set; }

		[ DataMember( Name = "customerUsername" ) ]
		public string CustomerUsername{ get; set; }

		[ DataMember( Name = "customerEmail" ) ]
		public string CustomerEmail{ get; set; }

		[ DataMember( Name = "customerNotes" ) ]
		public string CustomerNotes{ get; set; }

		[ DataMember( Name = "internalNotes" ) ]
		public string InternalNotes{ get; set; }

		[ DataMember( Name = "gift" ) ]
		public bool Gift{ get; set; }
		
		[ DataMember( Name = "giftMessage" ) ]
		public string GiftMessage{ get; set; }

		[ DataMember( Name = "internalMessage" ) ]
		public string InternalMessage{ get; set; }

		[ DataMember( Name = "paymentMethod" ) ]
		public string PaymentMethod{ get; set; }

		[ DataMember( Name = "requestedShippingService" ) ]
		public string RequestedShippingService{ get; set; }

		[ DataMember( Name = "carrierCode" ) ]
		public string CarrierCode{ get; set; }

		[ DataMember( Name = "serviceCode" ) ]
		public string ServiceCode{ get; set; }

		[ DataMember( Name = "packageCode" ) ]
		public string PackageCode{ get; set; }

		[ DataMember( Name = "confirmation" ) ]
		public string Confirmation{ get; set; }

		[ DataMember( Name = "shipTo" ) ]
		public ShipStationAddress ShippingAddress{ get; set; }

		[ DataMember( Name = "billTo" ) ]
		public ShipStationAddress BillingAddress{ get; set; }

		[ DataMember( Name = "items" ) ]
		public IList< ShipStationOrderItem > Items{ get; set; }

		[ DataMember( Name = "taxAmount" ) ]
		public decimal TaxAmount{ get; set; }

		[ DataMember( Name = "shippingAmount" ) ]
		public decimal ShippingAmount{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public ShipStationItemWeight Weight{ set; get; }

		[ DataMember( Name = "dimensions" ) ]
		public ShipStationOrderDimensions Dimensions{ get; set; }

		[ DataMember( Name = "insuranceOptions" ) ]
		public ShipStationOrderInsuranceOptions InsuranceOptions{ get; set; }

		[ DataMember( Name = "internationalOptions" ) ]
		public ShipStationOrderInternationalOptions InternationalOptions{ get; set; }

		[ DataMember( Name = "advancedOptions" ) ]
		public ShipStationOrderAdvancedOptions AdvancedOptions{ get; set; }

		public int MarketplaceId{ get; set; }

		#region Equality members
		public bool Equals( ShipStationOrder other )
		{
			if( ReferenceEquals( null, other ) )
				return false;
			if( ReferenceEquals( this, other ) )
				return true;
			return this.OrderId.Equals( other.OrderId );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			if( obj.GetType() != this.GetType() )
				return false;
			return this.Equals( ( ShipStationOrder )obj );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = this.OrderId.GetHashCode();
				result = ( result * 397 ) ^ this.OrderDate.GetHashCode();
				result = ( result * 397 ) ^ this.PaymentDate.GetHashCode();
				result = ( result * 397 ) ^ this.OrderNumber.GetHashCode();

				return result;
			}
		}

		public static bool operator ==( ShipStationOrder left, ShipStationOrder right )
		{
			return Equals( left, right );
		}

		public static bool operator !=( ShipStationOrder left, ShipStationOrder right )
		{
			return !Equals( left, right );
		}
		#endregion
	}

	public static class ShipStationOrderExtensions
	{
		public static bool IsValid( this ShipStationOrder order )
		{
			if( order == null )
				return false;

			var shipTo = order.ShippingAddress;
			if( shipTo == null )
				return false;

			if( string.IsNullOrEmpty( shipTo.Name ) || string.IsNullOrWhiteSpace( shipTo.Street1 ) || string.IsNullOrWhiteSpace( shipTo.City ) || string.IsNullOrWhiteSpace( shipTo.Country ) )
				return false;

			var isInUs = shipTo.Country.Equals( "US", StringComparison.OrdinalIgnoreCase);
			if( isInUs && ( string.IsNullOrWhiteSpace( shipTo.PostalCode ) || string.IsNullOrWhiteSpace( shipTo.State ) ) )
				return false;

			return true;
		}
	}

	public enum ShipStationOrderStatusEnum
	{
		// ReSharper disable InconsistentNaming
		awaiting_payment = 1,
		awaiting_shipment = 2,
		shipped = 3,
		cancelled = 4,
		on_hold = 5,
		// ReSharper restore InconsistentNaming
	}
}