using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.Order;

namespace ShipStationAccess.V2
{
	public interface IShipStationService
	{
		IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo );
		Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		void UpdateOrder( ShipStationOrder order );
		Task UpdateOrderAsync( ShipStationOrder order );
	}
}