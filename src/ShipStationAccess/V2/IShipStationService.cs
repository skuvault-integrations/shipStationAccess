using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Register;
using ShipStationAccess.V2.Models.ShippingLabel;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.TagList;
using ShipStationAccess.V2.Models.WarehouseLocation;
using ShipStationAccess.V2.Services;

namespace ShipStationAccess.V2
{
	public interface IShipStationService
	{
		IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, CancellationToken token, Func< ShipStationOrder, ShipStationOrder > processOrder = null );
		Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token, bool getShipmentsAndFulfillments = false, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null, Action< IEnumerable< ReadError > > handleSkippedOrders = null );
		IEnumerable< ShipStationOrder > GetOrders( string storeId, string orderNumber, CancellationToken token );

		ShipStationOrder GetOrderById( string orderId, CancellationToken token );
		Task< ShipStationOrder > GetOrderByIdAsync( string orderId, CancellationToken token );
		Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByIdAsync( string orderId, CancellationToken token );
		Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByIdAsync( string orderId, CancellationToken token );
		
		void UpdateOrder( ShipStationOrder order, CancellationToken token );
		Task UpdateOrderAsync( ShipStationOrder order, CancellationToken token );

		void UpdateOrderItemsWarehouseLocation( ShipStationWarehouseLocation warehouseLocation, CancellationToken token );
		Task UpdateOrderItemsWarehouseLocationAsync( ShipStationWarehouseLocation warehouseLocation, CancellationToken token );

		void UpdateOrderItemsWarehouseLocations( ShipStationWarehouseLocations warehouseLocations, CancellationToken token );
		Task UpdateOrderItemsWarehouseLocationsAsync( ShipStationWarehouseLocations warehouseLocations, CancellationToken token );

		IEnumerable< ShipStationStore > GetStores( CancellationToken token );
		Task< IEnumerable< ShipStationStore > > GetStoresAsync( CancellationToken token );

		IEnumerable< ShipStationTag > GetTags( CancellationToken token );
		Task< IEnumerable< ShipStationTag > > GetTagsAsync( CancellationToken token );

		Task < ShipStationShippingLabel > CreateAndGetShippingLabelAsync( string shipStationOrderId, string carrierCode, string serviceCode, string packageCode, string confirmation, DateTime shipDate, string weight, string weightUnit, CancellationToken token, bool isTestLabel = false );

		ShipStationRegisterResponse Register( ShipStationRegister register, CancellationToken token );

		DateTime LastActivityTime { get; }
	}
}