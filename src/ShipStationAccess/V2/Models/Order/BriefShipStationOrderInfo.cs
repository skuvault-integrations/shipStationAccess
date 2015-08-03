using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.OrderItem;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public class BriefShipStationOrderInfo
	{
		[ DataMember( Name = "orderKey" ) ]
		public string OrderKey{ get; set; }

		[ DataMember( Name = "createDate" ) ]
		public DateTime CreateDate{ get; set; }

		[ DataMember( Name = "orderId" ) ]
		public long OrderId{ get; set; }

		[ DataMember( Name = "items" ) ]
		public IList< ShipStationOrderItem > Items{ get; set; }

		[ DataMember( Name = "orderNumber" ) ]
		public string OrderNumber{ get; set; }

		[ DataMember( Name = "orderDate" ) ]
		public DateTime OrderDate{ get; set; }

		[ DataMember( Name = "orderStatus" ) ]
		public ShipStationOrderStatusEnum OrderStatus{ get; set; }

		[ DataMember( Name = "shipTo" ) ]
		public ShipStationAddress ShippingAddress{ get; set; }

		[ DataMember( Name = "billTo" ) ]
		public ShipStationAddress BillingAddress{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public ShipStationItemWeight Weight { set; get; }

		public BriefShipStationOrderInfo()
		{
		}

		public static BriefShipStationOrderInfo From( ShipStationOrder order )
		{
			return new BriefShipStationOrderInfo
			{
				Items = order.Items,
				OrderId = order.OrderId,
				BillingAddress = order.BillingAddress,
				OrderDate = order.OrderDate,
				OrderNumber = order.OrderNumber,
				ShippingAddress = order.ShippingAddress,
				OrderStatus = order.OrderStatus,
				CreateDate = order.CreateDate,
				OrderKey = order.OrderKey
			};
		}
	}
}