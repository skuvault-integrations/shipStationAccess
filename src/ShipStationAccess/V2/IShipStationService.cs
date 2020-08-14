using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Register;
using ShipStationAccess.V2.Models.ShippingLabel;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.TagList;
using ShipStationAccess.V2.Models.WarehouseLocation;

namespace ShipStationAccess.V2
{
	public interface IShipStationService
	{
		IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, ShipStationOrder > processOrder = null );
		Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null );
		IEnumerable< ShipStationOrder > GetOrders( string storeId, string orderNumber );

		ShipStationOrder GetOrderById( string orderId );
		Task< ShipStationOrder > GetOrderByIdAsync( string orderId );
		Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByIdAsync( string orderId );
		Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByIdAsync( string orderId );
		
		void UpdateOrder( ShipStationOrder order );
		Task UpdateOrderAsync( ShipStationOrder order );

		void UpdateOrderItemsWarehouseLocation( ShipStationWarehouseLocation warehouseLocation );
		Task UpdateOrderItemsWarehouseLocationAsync( ShipStationWarehouseLocation warehouseLocation );

		void UpdateOrderItemsWarehouseLocations( ShipStationWarehouseLocations warehouseLocations );
		Task UpdateOrderItemsWarehouseLocationsAsync( ShipStationWarehouseLocations warehouseLocations );

		IEnumerable< ShipStationStore > GetStores();
		Task< IEnumerable< ShipStationStore > > GetStoresAsync();

		IEnumerable< ShipStationTag > GetTags();
		Task< IEnumerable< ShipStationTag > > GetTagsAsync();

		Task < ShipStationShippingLabel > CreateAndGetShippingLabelAsync( string shipStationOrderId, string carrierCode, string serviceCode, string packageCode, string confirmation, DateTime shipDate, string weight, string weightUnit, bool isTestLabel = false );

		ShipStationRegisterResponse Register( ShipStationRegister register );
	}
}