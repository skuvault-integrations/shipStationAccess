using System.Threading.Tasks;
using ShipStationAccess.DataService;
using ShipStationAccess.ShipStationApi;

namespace ShipStationAccess
{
	public interface IShipStationService
	{
		ShipStationEntities Context { get; }

		IDataServiceQuery< Store > Stores { get; }
		IDataServiceQuery< UserInfo > UserInfos { get; }

		IDataServiceQuery< Order > Orders { get; }
		IDataServiceQuery< OrderItem > OrderItems { get; }
		IDataServiceQuery< Shipment > Shipments { get; }
		IDataServiceQuery< Carrier > Carriers { get; }
		IDataServiceQuery< Customer > Customers { get; }
		IDataServiceQuery< CustomerUser > CustomerUsers { get; }
		IDataServiceQuery< CustomsItem > CustomsItems { get; }
		IDataServiceQuery< EmailTemplate > EmailTemplates { get; }
		IDataServiceQuery< Marketplace > Marketplaces { get; }
		IDataServiceQuery< OrderStatus > OrderStatuses { get; }
		IDataServiceQuery< PackageType > PackageTypes { get; }
		IDataServiceQuery< ShipmentItem > ShipmentItems { get; }
		IDataServiceQuery< ShippingService > ShippingServices { get; }
		IDataServiceQuery< Warehouse > Warehouses { get; }
		IDataServiceQuery< OrderFilter > OrderFilters { get; }
		IDataServiceQuery< Product > Products { get; }
		IDataServiceQuery< ShippingProvider > ShippingProviders { get; }

		void AddToStores( Store store );
		void AddToUserInfos( UserInfo userInfo );
		void AddToOrders( Order order );
		void AddToOrderItems( OrderItem orderItem );
		void AddToShipments( Shipment shipment );
		void AddToCarriers( Carrier carrier );
		void AddToCustomers( Customer customer );
		void AddToCustomerUsers( CustomerUser customerUser );
		void AddToCustomsItems( CustomsItem customsItem );
		void AddToEmailTemplates( EmailTemplate emailTemplate );
		void AddToMarketplaces( Marketplace marketplace );
		void AddToOrderStatuses( OrderStatus orderStatus );
		void AddToPackageTypes( PackageType packageType );
		void AddToShipmentItems( ShipmentItem shipmentItem );
		void AddToShippingServices( ShippingService shippingService );
		void AddToWarehouses( Warehouse warehouse );
		void AddToOrderFilters( OrderFilter orderFilter );
		void AddToProducts( Product product );
		void AddToShippingProviders( ShippingProvider shippingProvider );

		void UpdateStore( Store store );
		void UpdateUserInfo( UserInfo userInfo );
		void UpdateOrder( Order order );
		void UpdateOrderItem( OrderItem orderItem );
		void UpdateShipment( Shipment shipment );
		void UpdateCarrier( Carrier carrier );
		void UpdateCustomer( Customer customer );
		void UpdateCustomerUser( CustomerUser customerUser );
		void UpdateCustomsItem( CustomsItem customsItem );
		void UpdateEmailTemplate( EmailTemplate emailTemplate );
		void UpdateMarketplace( Marketplace marketplace );
		void UpdateOrderStatus( OrderStatus orderStatus );
		void UpdatePackageType( PackageType packageType );
		void UpdateShipmentItem( ShipmentItem shipmentItem );
		void UpdateShippingService( ShippingService shippingService );
		void UpdateWarehouse( Warehouse warehouse );
		void UpdateOrderFilter( OrderFilter orderFilter );
		void UpdateProduct( Product product );
		void UpdateShippingProvider( ShippingProvider shippingProvider );

		void DeleteStore( Store store );
		void DeleteUserInfo( UserInfo userInfo );
		void DeleteOrder( Order order );
		void DeleteOrderItem( OrderItem orderItem );
		void DeleteShipment( Shipment shipment );
		void DeleteCarrier( Carrier carrier );
		void DeleteCustomer( Customer customer );
		void DeleteCustomerUser( CustomerUser customerUser );
		void DeleteCustomsItem( CustomsItem customsItem );
		void DeleteEmailTemplate( EmailTemplate emailTemplate );
		void DeleteMarketplace( Marketplace marketplace );
		void DeleteOrderStatus( OrderStatus orderStatus );
		void DeletePackageType( PackageType packageType );
		void DeleteShipmentItem( ShipmentItem shipmentItem );
		void DeleteShippingService( ShippingService shippingService );
		void DeleteWarehouse( Warehouse warehouse );
		void DeleteOrderFilter( OrderFilter orderFilter );
		void DeleteProduct( Product product );
		void DeleteShippingProvider( ShippingProvider shippingProvider );

		void SaveChanges();
		Task SaveChangesAsync();
	}
}