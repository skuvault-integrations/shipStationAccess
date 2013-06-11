using System;
using System.Net;
using System.Threading.Tasks;
using ShipStationAccess.DataService;
using ShipStationAccess.ShipStationApi;

namespace ShipStationAccess
{
	/// <summary>
	/// ShipStation service connecting directly to ShipStation servers.
	/// </summary>
	/// <seealso href="http://api.shipstation.com/MainPage.ashx"/>
	public class ShipStationServices : IShipStationServices
	{
		private readonly ShipStationEntities _context;

		public ShipStationServices( string userName, string password )
		{
			this._context = CreateContext( userName, password );
		}

		private ShipStationEntities CreateContext( string userName, string password )
		{
			return new ShipStationEntities( new Uri( "https://data.shipstation.com/1.1" ) )
				{ Credentials = new NetworkCredential( userName, password ) };
		}

		#region Queries
		public IDataServiceQuery< Store > Stores
		{
			get { return new DataServiceQueryWrapper< Store >( this._context.Stores ); }
		}

		public IDataServiceQuery< UserInfo > UserInfos
		{
			get { return new DataServiceQueryWrapper< UserInfo >( this._context.UserInfos ); }
		}

		public IDataServiceQuery< Order > Orders
		{
			get { return new DataServiceQueryWrapper< Order >( this._context.Orders ); }
		}

		public IDataServiceQuery< OrderItem > OrderItems
		{
			get { return new DataServiceQueryWrapper< OrderItem >( this._context.OrderItems ); }
		}

		public IDataServiceQuery< Shipment > Shipments
		{
			get { return new DataServiceQueryWrapper< Shipment >( this._context.Shipments ); }
		}

		public IDataServiceQuery< Carrier > Carriers
		{
			get { return new DataServiceQueryWrapper< Carrier >( this._context.Carriers ); }
		}

		public IDataServiceQuery< Customer > Customers
		{
			get { return new DataServiceQueryWrapper< Customer >( this._context.Customers ); }
		}

		public IDataServiceQuery< CustomerUser > CustomerUsers
		{
			get { return new DataServiceQueryWrapper< CustomerUser >( this._context.CustomerUsers ); }
		}

		public IDataServiceQuery< CustomsItem > CustomsItems
		{
			get { return new DataServiceQueryWrapper< CustomsItem >( this._context.CustomsItems ); }
		}

		public IDataServiceQuery< EmailTemplate > EmailTemplates
		{
			get { return new DataServiceQueryWrapper< EmailTemplate >( this._context.EmailTemplates ); }
		}

		public IDataServiceQuery< Marketplace > Marketplaces
		{
			get { return new DataServiceQueryWrapper< Marketplace >( this._context.Marketplaces ); }
		}

		public IDataServiceQuery< OrderStatus > OrderStatuses
		{
			get { return new DataServiceQueryWrapper< OrderStatus >( this._context.OrderStatuses ); }
		}

		public IDataServiceQuery< PackageType > PackageTypes
		{
			get { return new DataServiceQueryWrapper< PackageType >( this._context.PackageTypes ); }
		}

		public IDataServiceQuery< ShipmentItem > ShipmentItems
		{
			get { return new DataServiceQueryWrapper< ShipmentItem >( this._context.ShipmentItems ); }
		}

		public IDataServiceQuery< ShippingService > ShippingServices
		{
			get { return new DataServiceQueryWrapper< ShippingService >( this._context.ShippingServices ); }
		}

		public IDataServiceQuery< Warehouse > Warehouses
		{
			get { return new DataServiceQueryWrapper< Warehouse >( this._context.Warehouses ); }
		}

		public IDataServiceQuery< OrderFilter > OrderFilters
		{
			get { return new DataServiceQueryWrapper< OrderFilter >( this._context.OrderFilters ); }
		}

		public IDataServiceQuery< Product > Products
		{
			get { return new DataServiceQueryWrapper< Product >( this._context.Products ); }
		}

		public IDataServiceQuery< ShippingProvider > ShippingProviders
		{
			get { return new DataServiceQueryWrapper< ShippingProvider >( this._context.ShippingProviders ); }
		}
		#endregion

		#region Add
		public void AddToStores( Store store )
		{
			this._context.AddToStores( store );
		}

		public void AddToUserInfos( UserInfo userInfo )
		{
			this._context.AddToUserInfos( userInfo );
		}

		public void AddToOrders( Order order )
		{
			this._context.AddToOrders( order );
		}

		public void AddToOrderItems( OrderItem orderItem )
		{
			this._context.AddToOrderItems( orderItem );
		}

		public void AddToShipments( Shipment shipment )
		{
			this._context.AddToShipments( shipment );
		}

		public void AddToCarriers( Carrier carrier )
		{
			this._context.AddToCarriers( carrier );
		}

		public void AddToCustomers( Customer customer )
		{
			this._context.AddToCustomers( customer );
		}

		public void AddToCustomerUsers( CustomerUser customerUser )
		{
			this._context.AddToCustomerUsers( customerUser );
		}

		public void AddToCustomsItems( CustomsItem customsItem )
		{
			this._context.AddToCustomsItems( customsItem );
		}

		public void AddToEmailTemplates( EmailTemplate emailTemplate )
		{
			this._context.AddToEmailTemplates( emailTemplate );
		}

		public void AddToMarketplaces( Marketplace marketplace )
		{
			this._context.AddToMarketplaces( marketplace );
		}

		public void AddToOrderStatuses( OrderStatus orderStatus )
		{
			this._context.AddToOrderStatuses( orderStatus );
		}

		public void AddToPackageTypes( PackageType packageType )
		{
			this._context.AddToPackageTypes( packageType );
		}

		public void AddToShipmentItems( ShipmentItem shipmentItem )
		{
			this._context.AddToShipmentItems( shipmentItem );
		}

		public void AddToShippingServices( ShippingService shippingService )
		{
			this._context.AddToShippingServices( shippingService );
		}

		public void AddToWarehouses( Warehouse warehouse )
		{
			this._context.AddToWarehouses( warehouse );
		}

		public void AddToOrderFilters( OrderFilter orderFilter )
		{
			this._context.AddToOrderFilters( orderFilter );
		}

		public void AddToProducts( Product product )
		{
			this._context.AddToProducts( product );
		}

		public void AddToShippingProviders( ShippingProvider shippingProvider )
		{
			this._context.AddToShippingProviders( shippingProvider );
		}
		#endregion

		#region Update
		public void UpdateStore( Store store )
		{
			this._context.UpdateObject( store );
		}

		public void UpdateUserInfo( UserInfo userInfo )
		{
			this._context.UpdateObject( userInfo );
		}

		public void UpdateOrder( Order order )
		{
			this._context.UpdateObject( order );
		}

		public void UpdateOrderItem( OrderItem orderItem )
		{
			this._context.UpdateObject( orderItem );
		}

		public void UpdateShipment( Shipment shipment )
		{
			this._context.UpdateObject( shipment );
		}

		public void UpdateCarrier( Carrier carrier )
		{
			this._context.UpdateObject( carrier );
		}

		public void UpdateCustomer( Customer customer )
		{
			this._context.UpdateObject( customer );
		}

		public void UpdateCustomerUser( CustomerUser customerUser )
		{
			this._context.UpdateObject( customerUser );
		}

		public void UpdateCustomsItem( CustomsItem customsItem )
		{
			this._context.UpdateObject( customsItem );
		}

		public void UpdateEmailTemplate( EmailTemplate emailTemplate )
		{
			this._context.UpdateObject( emailTemplate );
		}

		public void UpdateMarketplace( Marketplace marketplace )
		{
			this._context.UpdateObject( marketplace );
		}

		public void UpdateOrderStatus( OrderStatus orderStatus )
		{
			this._context.UpdateObject( orderStatus );
		}

		public void UpdatePackageType( PackageType packageType )
		{
			this._context.UpdateObject( packageType );
		}

		public void UpdateShipmentItem( ShipmentItem shipmentItem )
		{
			this._context.UpdateObject( shipmentItem );
		}

		public void UpdateShippingService( ShippingService shippingService )
		{
			this._context.UpdateObject( shippingService );
		}

		public void UpdateWarehouse( Warehouse warehouse )
		{
			this._context.UpdateObject( warehouse );
		}

		public void UpdateOrderFilter( OrderFilter orderFilter )
		{
			this._context.UpdateObject( orderFilter );
		}

		public void UpdateProduct( Product product )
		{
			this._context.UpdateObject( product );
		}

		public void UpdateShippingProvider( ShippingProvider shippingProvider )
		{
			this._context.UpdateObject( shippingProvider );
		}
		#endregion

		#region Delete
		public void DeleteStore( Store store )
		{
			this._context.DeleteObject( store );
		}

		public void DeleteUserInfo( UserInfo userInfo )
		{
			this._context.DeleteObject( userInfo );
		}

		public void DeleteOrder( Order order )
		{
			this._context.DeleteObject( order );
		}

		public void DeleteOrderItem( OrderItem orderItem )
		{
			this._context.DeleteObject( orderItem );
		}

		public void DeleteShipment( Shipment shipment )
		{
			this._context.DeleteObject( shipment );
		}

		public void DeleteCarrier( Carrier carrier )
		{
			this._context.DeleteObject( carrier );
		}

		public void DeleteCustomer( Customer customer )
		{
			this._context.DeleteObject( customer );
		}

		public void DeleteCustomerUser( CustomerUser customerUser )
		{
			this._context.DeleteObject( customerUser );
		}

		public void DeleteCustomsItem( CustomsItem customsItem )
		{
			this._context.DeleteObject( customsItem );
		}

		public void DeleteEmailTemplate( EmailTemplate emailTemplate )
		{
			this._context.DeleteObject( emailTemplate );
		}

		public void DeleteMarketplace( Marketplace marketplace )
		{
			this._context.DeleteObject( marketplace );
		}

		public void DeleteOrderStatus( OrderStatus orderStatus )
		{
			this._context.DeleteObject( orderStatus );
		}

		public void DeletePackageType( PackageType packageType )
		{
			this._context.DeleteObject( packageType );
		}

		public void DeleteShipmentItem( ShipmentItem shipmentItem )
		{
			this._context.DeleteObject( shipmentItem );
		}

		public void DeleteShippingService( ShippingService shippingService )
		{
			this._context.DeleteObject( shippingService );
		}

		public void DeleteWarehouse( Warehouse warehouse )
		{
			this._context.DeleteObject( warehouse );
		}

		public void DeleteOrderFilter( OrderFilter orderFilter )
		{
			this._context.DeleteObject( orderFilter );
		}

		public void DeleteProduct( Product product )
		{
			this._context.DeleteObject( product );
		}

		public void DeleteShippingProvider( ShippingProvider shippingProvider )
		{
			this._context.DeleteObject( shippingProvider );
		}
		#endregion

		public void SaveChanges()
		{
			this._context.SaveChanges();
		}

		public Task SaveChangesAsync()
		{
			return Task.Factory.FromAsync( this.BeginSaveChanges, this.EndSaveChanges, null );
		}

		public IAsyncResult BeginSaveChanges( AsyncCallback callback, object state )
		{
			return this._context.BeginSaveChanges( callback, state );
		}

		public void EndSaveChanges( IAsyncResult asyncResult )
		{
			this._context.EndSaveChanges( asyncResult );
		}
	}
}