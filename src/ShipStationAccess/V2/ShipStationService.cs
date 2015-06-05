using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Netco.Extensions;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Services;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService: IShipStationService
	{
		private readonly WebRequestServices _webRequestServices;
		private const int RequestMaxLimit = 100;

		public ShipStationService( ShipStationCredentials credentials )
		{
			this._webRequestServices = new WebRequestServices( credentials );
		}

		#region Get Orders
		public IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, ShipStationOrder > processOrder = null )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var processedOrderIds = new HashSet< long >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var hasOrders = true;

			Action< ShipStationOrders > processOrders = sorders =>
			{
				foreach( var order in sorders.Orders )
				{
					var curOrder = order;
					if( processedOrderIds.Contains( curOrder.OrderId ) )
						continue;

					if( processOrder != null )
						curOrder = processOrder( curOrder );
					orders.Add( curOrder );
					processedOrderIds.Add( curOrder.OrderId );
				}
			};

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );
				pagesCount++;

				ActionPolicies.Get.Do( () =>
				{
					var newOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					processOrders( newOrdersWithinPage );

					var modifiedOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );
					processOrders( modifiedOrdersWithinPage );

					hasOrders = newOrdersWithinPage.Orders.Any() || modifiedOrdersWithinPage.Orders.Any();
				} );
			} while( hasOrders );

			this.FindMarketplaceIds( orders );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var processedOrderIds = new HashSet< long >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var hasOrders = true;

			Func< ShipStationOrders, Task > processOrders = async sorders =>
			{
				var processedOrders = await sorders.Orders.ProcessInBatchAsync( 50, async o =>
				{
					var curOrder = o;
					if( processedOrderIds.Contains( curOrder.OrderId ) )
						return null;

					if( processOrder != null )
						curOrder = await processOrder( curOrder );

					return curOrder;
				} );

				foreach( var order in processedOrders )
				{
					orders.Add( order );
					processedOrderIds.Add( order.OrderId );
				}
			};

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );
				pagesCount++;

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var newOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					await processOrders( newOrdersWithinPage );
					var modifiedOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );
					await processOrders( modifiedOrdersWithinPage );

					hasOrders = newOrdersWithinPage.Orders.Any() || modifiedOrdersWithinPage.Orders.Any();
				} );
			} while( hasOrders );

			await this.FindMarketplaceIdsAsync( orders );

			return orders;
		}
		#endregion

		#region Update Orders
		public void UpdateOrder( ShipStationOrder order )
		{
			if( !order.IsValid() )
				return;

			ActionPolicies.Submit.Do( () =>
			{
				try
				{
					this._webRequestServices.PostData( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson() );
				}
				catch( WebException x )
				{
					if( x.Response.GetHttpStatusCode() == HttpStatusCode.InternalServerError )
					{
						ShipStationLogger.Log.Trace( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
					}
					else
						throw;
				}
			} );
		}

		public async Task UpdateOrderAsync( ShipStationOrder order )
		{
			if( !order.IsValid() )
				return;

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				try
				{
					await this._webRequestServices.PostDataAsync( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson() );
				}
				catch( WebException x )
				{
					if( x.Response.GetHttpStatusCode() == HttpStatusCode.InternalServerError )
					{
						ShipStationLogger.Log.Trace( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
					}
					else
						throw;
				}
			} );
		}
		#endregion

		#region Get Stores
		public IEnumerable< ShipStationStore > GetStores()
		{
			var stores = new List< ShipStationStore >();
			ActionPolicies.Submit.Do( () =>
			{
				stores = this._webRequestServices.GetResponse< List< ShipStationStore > >( ShipStationCommand.GetStores, ParamsBuilder.EmptyParams );
			} );
			return stores;
		}

		public async Task< IEnumerable< ShipStationStore > > GetStoresAsync()
		{
			var stores = new List< ShipStationStore >();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				stores = await this._webRequestServices.GetResponseAsync< List< ShipStationStore > >( ShipStationCommand.GetStores, ParamsBuilder.EmptyParams );
			} );
			return stores;
		}
		#endregion

		#region Misc
		private void FindMarketplaceIds( IEnumerable< ShipStationOrder > orders )
		{
			var stores = this.GetStores();

			foreach( var order in orders )
			{
				var store = stores.FirstOrDefault( s => s.StoreId == order.AdvancedOptions.StoreId );
				if( store != null )
					order.MarketplaceId = store.MarketplaceId;
			}
		}

		private async Task FindMarketplaceIdsAsync( IEnumerable< ShipStationOrder > orders )
		{
			var stores = await this.GetStoresAsync();

			foreach( var order in orders )
			{
				var store = stores.FirstOrDefault( s => s.StoreId == order.AdvancedOptions.StoreId );
				if( store != null )
					order.MarketplaceId = store.MarketplaceId;
			}
		}
		#endregion
	}
}