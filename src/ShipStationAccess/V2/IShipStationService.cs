using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.TagList;

namespace ShipStationAccess.V2
{
	public interface IShipStationService
	{
		IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, ShipStationOrder > processOrder = null );
		Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null );

		void UpdateOrder( ShipStationOrder order );
		Task UpdateOrderAsync( ShipStationOrder order );

		IEnumerable< ShipStationStore > GetStores();
		Task< IEnumerable< ShipStationStore > > GetStoresAsync();

		IEnumerable< ShipStationTag > GetTags();
		Task < IEnumerable< ShipStationTag > > GetTagsAsync();
	}
}