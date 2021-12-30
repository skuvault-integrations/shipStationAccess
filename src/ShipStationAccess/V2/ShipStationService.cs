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
using ShipStationAccess.V2.Models.TagList;
using ShipStationAccess.V2.Services;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService: IShipStationService
	{
		private readonly WebRequestServices _webRequestServices;
		private const int RequestMaxLimit = 100;

		/// <summary>
		///	Last service's network activity time. Can be used to monitor service's state.
		/// </summary>
		public DateTime LastActivityTime
		{
			get
			{
				return this._webRequestServices.LastNetworkActivityTime ?? DateTime.UtcNow;
			}
		}
		
		public ShipStationService( ShipStationCredentials credentials )
		{
			this._webRequestServices = new WebRequestServices( credentials );
		}

		public IEnumerable< ShipStationTag > GetTags()
		{
			var tags = new List< ShipStationTag >();
			ActionPolicies.Get.Do( () =>
			{
				tags = this._webRequestServices.GetResponse< List< ShipStationTag > >( ShipStationCommand.GetTags, string.Empty );
			} );

			return tags;
		}

		public async Task < IEnumerable< ShipStationTag > > GetTagsAsync()
		{
			var tags = new List< ShipStationTag >();
			await ActionPolicies.GetAsync.Do( async () =>
			{
				tags = await this._webRequestServices.GetResponseAsync< List< ShipStationTag > >( ShipStationCommand.GetTags, string.Empty );
			} );

			return tags;
		}

		#region Get Orders
		public IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, ShipStationOrder > processOrder = null )
		{
			var orders = new List< ShipStationOrder >();
			var processedOrderIds = new HashSet< long >();
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

			Action< string > downloadOrders = endPoint =>
			{
				var pagesCount = int.MaxValue;
				var currentPage = 1;
				var ordersCount = 0;
				var ordersExpected = -1;

				do
				{
					var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
					var ordersEndPoint = endPoint.ConcatParams( nextPageParams );

					ActionPolicies.Get.Do( () =>
					{
						var ordersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint );
						if( pagesCount == int.MaxValue )
						{
							pagesCount = ordersWithinPage.TotalPages;
							ordersExpected = ordersWithinPage.TotalOrders;
						}
						currentPage++;
						ordersCount += ordersWithinPage.Orders.Count;

						processOrders( ordersWithinPage );
					} );
				} while( currentPage <= pagesCount );

				ShipStationLogger.Log.Trace( "Orders dowloaded API '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}",
					_webRequestServices.GetApiKey(), ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			downloadOrders( newOrdersEndpoint );

			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			downloadOrders( modifiedOrdersEndpoint );

			this.FindMarketplaceIds( orders );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null )
		{
			var orders = new List< ShipStationOrder >();
			var processedOrderIds = new HashSet< long >();

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

			Func< string, Task > downloadOrders = async endPoint =>
			{
				var pagesCount = int.MaxValue;
				var currentPage = 1;
				var ordersCount = 0;
				var ordersExpected = -1;

				do
				{
					var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
					var ordersEndPoint = endPoint.ConcatParams( nextPageParams );

					await ActionPolicies.GetAsync.Do( async () =>
					{
						var ordersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint );
						if( pagesCount == int.MaxValue )
						{
							pagesCount = ordersWithinPage.TotalPages;
							ordersExpected = ordersWithinPage.TotalOrders;
						}
						currentPage++;
						ordersCount += ordersWithinPage.Orders.Count;

						await processOrders( ordersWithinPage );
					} );
				} while( currentPage <= pagesCount );

				ShipStationLogger.Log.Trace( "Orders dowloaded API '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}",
					_webRequestServices.GetApiKey(), ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			await downloadOrders( newOrdersEndpoint );

			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			await downloadOrders( modifiedOrdersEndpoint );

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
						ShipStationLogger.Log.Trace( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
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
						ShipStationLogger.Log.Trace( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
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