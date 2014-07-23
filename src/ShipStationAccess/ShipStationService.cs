using System;
using System.Data.Services.Common;
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
	public class ShipStationService : IShipStationService
	{
		public ShipStationEntities Context { get; private set; }

		public ShipStationService( string userName, string password )
		{
			this.Context = CreateContext( userName, password );
		}

		private ShipStationEntities CreateContext( string userName, string password )
		{
			return new ShipStationEntities( new Uri( "https://data.shipstation.com/1.3" ), DataServiceProtocolVersion.V3 ) { Credentials = new NetworkCredential( userName, password ), Timeout = 600 };
		}

		#region Queries

		public IDataServiceQuery< Store > Stores
		{
			get { return new DataServiceQueryWrapper< Store >( this.Context.Stores ); }
		}

		public IDataServiceQuery< UserInfo > UserInfos
		{
			get { return new DataServiceQueryWrapper< UserInfo >( this.Context.UserInfos ); }
		}

		public IDataServiceQuery< Order > Orders
		{
			get { return new DataServiceQueryWrapper< Order >( this.Context.Orders ); }
		}

		public IDataServiceQuery< OrderItem > OrderItems
		{
			get { return new DataServiceQueryWrapper< OrderItem >( this.Context.OrderItems ); }
		}

		public IDataServiceQuery< Shipment > Shipments
		{
			get { return new DataServiceQueryWrapper< Shipment >( this.Context.Shipments ); }
		}

		public IDataServiceQuery< Carrier > Carriers
		{
			get { return new DataServiceQueryWrapper< Carrier >( this.Context.Carriers ); }
		}

		public IDataServiceQuery< Customer > Customers
		{
			get { return new DataServiceQueryWrapper< Customer >( this.Context.Customers ); }
		}

		public IDataServiceQuery< CustomerUser > CustomerUsers
		{
			get { return new DataServiceQueryWrapper< CustomerUser >( this.Context.CustomerUsers ); }
		}

		public IDataServiceQuery< CustomsItem > CustomsItems
		{
			get { return new DataServiceQueryWrapper< CustomsItem >( this.Context.CustomsItems ); }
		}

		public IDataServiceQuery< EmailTemplate > EmailTemplates
		{
			get { return new DataServiceQueryWrapper< EmailTemplate >( this.Context.EmailTemplates ); }
		}

		public IDataServiceQuery< Marketplace > Marketplaces
		{
			get { return new DataServiceQueryWrapper< Marketplace >( this.Context.Marketplaces ); }
		}

		public IDataServiceQuery< OrderStatus > OrderStatuses
		{
			get { return new DataServiceQueryWrapper< OrderStatus >( this.Context.OrderStatuses ); }
		}

		public IDataServiceQuery< PackageType > PackageTypes
		{
			get { return new DataServiceQueryWrapper< PackageType >( this.Context.PackageTypes ); }
		}

		public IDataServiceQuery< ShipmentItem > ShipmentItems
		{
			get { return new DataServiceQueryWrapper< ShipmentItem >( this.Context.ShipmentItems ); }
		}

		public IDataServiceQuery< ShippingService > ShippingServices
		{
			get { return new DataServiceQueryWrapper< ShippingService >( this.Context.ShippingServices ); }
		}

		public IDataServiceQuery< Warehouse > Warehouses
		{
			get { return new DataServiceQueryWrapper< Warehouse >( this.Context.Warehouses ); }
		}

		public IDataServiceQuery< OrderFilter > OrderFilters
		{
			get { return new DataServiceQueryWrapper< OrderFilter >( this.Context.OrderFilters ); }
		}

		public IDataServiceQuery< Product > Products
		{
			get { return new DataServiceQueryWrapper< Product >( this.Context.Products ); }
		}

		public IDataServiceQuery< ShippingProvider > ShippingProviders
		{
			get { return new DataServiceQueryWrapper< ShippingProvider >( this.Context.ShippingProviders ); }
		}
		#endregion

		#region Add
		public void AddToStores( Store store )
		{
			this.Context.AddToStores( store );
		}

		public void AddToUserInfos( UserInfo userInfo )
		{
			this.Context.AddToUserInfos( userInfo );
		}

		public void AddToOrders( Order order )
		{
			this.Context.AddToOrders( order );
		}

		public void AddToOrderItems( OrderItem orderItem )
		{
			this.Context.AddToOrderItems( orderItem );
		}

		public void AddToShipments( Shipment shipment )
		{
			this.Context.AddToShipments( shipment );
		}

		public void AddToCarriers( Carrier carrier )
		{
			this.Context.AddToCarriers( carrier );
		}

		public void AddToCustomers( Customer customer )
		{
			this.Context.AddToCustomers( customer );
		}

		public void AddToCustomerUsers( CustomerUser customerUser )
		{
			this.Context.AddToCustomerUsers( customerUser );
		}

		public void AddToCustomsItems( CustomsItem customsItem )
		{
			this.Context.AddToCustomsItems( customsItem );
		}

		public void AddToEmailTemplates( EmailTemplate emailTemplate )
		{
			this.Context.AddToEmailTemplates( emailTemplate );
		}

		public void AddToMarketplaces( Marketplace marketplace )
		{
			this.Context.AddToMarketplaces( marketplace );
		}

		public void AddToOrderStatuses( OrderStatus orderStatus )
		{
			this.Context.AddToOrderStatuses( orderStatus );
		}

		public void AddToPackageTypes( PackageType packageType )
		{
			this.Context.AddToPackageTypes( packageType );
		}

		public void AddToShipmentItems( ShipmentItem shipmentItem )
		{
			this.Context.AddToShipmentItems( shipmentItem );
		}

		public void AddToShippingServices( ShippingService shippingService )
		{
			this.Context.AddToShippingServices( shippingService );
		}

		public void AddToWarehouses( Warehouse warehouse )
		{
			this.Context.AddToWarehouses( warehouse );
		}

		public void AddToOrderFilters( OrderFilter orderFilter )
		{
			this.Context.AddToOrderFilters( orderFilter );
		}

		public void AddToProducts( Product product )
		{
			this.Context.AddToProducts( product );
		}

		public void AddToShippingProviders( ShippingProvider shippingProvider )
		{
			this.Context.AddToShippingProviders( shippingProvider );
		}
		#endregion

		#region Update
		public void UpdateStore( Store store )
		{
			this.Context.UpdateObject( store );
		}

		public void UpdateUserInfo( UserInfo userInfo )
		{
			this.Context.UpdateObject( userInfo );
		}

		public void UpdateOrder( Order order )
		{
			this.Context.UpdateObject( order );
		}

		public void UpdateOrderItem( OrderItem orderItem )
		{
			this.Context.UpdateObject( orderItem );
		}

		public void UpdateShipment( Shipment shipment )
		{
			this.Context.UpdateObject( shipment );
		}

		public void UpdateCarrier( Carrier carrier )
		{
			this.Context.UpdateObject( carrier );
		}

		public void UpdateCustomer( Customer customer )
		{
			this.Context.UpdateObject( customer );
		}

		public void UpdateCustomerUser( CustomerUser customerUser )
		{
			this.Context.UpdateObject( customerUser );
		}

		public void UpdateCustomsItem( CustomsItem customsItem )
		{
			this.Context.UpdateObject( customsItem );
		}

		public void UpdateEmailTemplate( EmailTemplate emailTemplate )
		{
			this.Context.UpdateObject( emailTemplate );
		}

		public void UpdateMarketplace( Marketplace marketplace )
		{
			this.Context.UpdateObject( marketplace );
		}

		public void UpdateOrderStatus( OrderStatus orderStatus )
		{
			this.Context.UpdateObject( orderStatus );
		}

		public void UpdatePackageType( PackageType packageType )
		{
			this.Context.UpdateObject( packageType );
		}

		public void UpdateShipmentItem( ShipmentItem shipmentItem )
		{
			this.Context.UpdateObject( shipmentItem );
		}

		public void UpdateShippingService( ShippingService shippingService )
		{
			this.Context.UpdateObject( shippingService );
		}

		public void UpdateWarehouse( Warehouse warehouse )
		{
			this.Context.UpdateObject( warehouse );
		}

		public void UpdateOrderFilter( OrderFilter orderFilter )
		{
			this.Context.UpdateObject( orderFilter );
		}

		public void UpdateProduct( Product product )
		{
			this.Context.UpdateObject( product );
		}

		public void UpdateShippingProvider( ShippingProvider shippingProvider )
		{
			this.Context.UpdateObject( shippingProvider );
		}
		#endregion

		#region Delete
		public void DeleteStore( Store store )
		{
			this.Context.DeleteObject( store );
		}

		public void DeleteUserInfo( UserInfo userInfo )
		{
			this.Context.DeleteObject( userInfo );
		}

		public void DeleteOrder( Order order )
		{
			this.Context.DeleteObject( order );
		}

		public void DeleteOrderItem( OrderItem orderItem )
		{
			this.Context.DeleteObject( orderItem );
		}

		public void DeleteShipment( Shipment shipment )
		{
			this.Context.DeleteObject( shipment );
		}

		public void DeleteCarrier( Carrier carrier )
		{
			this.Context.DeleteObject( carrier );
		}

		public void DeleteCustomer( Customer customer )
		{
			this.Context.DeleteObject( customer );
		}

		public void DeleteCustomerUser( CustomerUser customerUser )
		{
			this.Context.DeleteObject( customerUser );
		}

		public void DeleteCustomsItem( CustomsItem customsItem )
		{
			this.Context.DeleteObject( customsItem );
		}

		public void DeleteEmailTemplate( EmailTemplate emailTemplate )
		{
			this.Context.DeleteObject( emailTemplate );
		}

		public void DeleteMarketplace( Marketplace marketplace )
		{
			this.Context.DeleteObject( marketplace );
		}

		public void DeleteOrderStatus( OrderStatus orderStatus )
		{
			this.Context.DeleteObject( orderStatus );
		}

		public void DeletePackageType( PackageType packageType )
		{
			this.Context.DeleteObject( packageType );
		}

		public void DeleteShipmentItem( ShipmentItem shipmentItem )
		{
			this.Context.DeleteObject( shipmentItem );
		}

		public void DeleteShippingService( ShippingService shippingService )
		{
			this.Context.DeleteObject( shippingService );
		}

		public void DeleteWarehouse( Warehouse warehouse )
		{
			this.Context.DeleteObject( warehouse );
		}

		public void DeleteOrderFilter( OrderFilter orderFilter )
		{
			this.Context.DeleteObject( orderFilter );
		}

		public void DeleteProduct( Product product )
		{
			this.Context.DeleteObject( product );
		}

		public void DeleteShippingProvider( ShippingProvider shippingProvider )
		{
			this.Context.DeleteObject( shippingProvider );
		}
		#endregion

		public void SaveChanges()
		{
			this.Context.SaveChanges();
		}

		public Task SaveChangesAsync()
		{
			return Task.Factory.FromAsync( this.BeginSaveChanges, this.EndSaveChanges, null );
		}

		public IAsyncResult BeginSaveChanges( AsyncCallback callback, object state )
		{
			return this.Context.BeginSaveChanges( callback, state );
		}

		public void EndSaveChanges( IAsyncResult asyncResult )
		{
			this.Context.EndSaveChanges( asyncResult );
		}
	}
}