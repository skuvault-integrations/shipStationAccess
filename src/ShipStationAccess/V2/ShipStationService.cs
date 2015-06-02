using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		private const int RequestMaxLimit = 500;

		public ShipStationService( ShipStationCredentials credentials )
		{
			this._webRequestServices = new WebRequestServices( credentials );
		}

		#region Get Orders
		public IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var hasOrders = true;

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );
				pagesCount++;

				ActionPolicies.Get.Do( () =>
				{
					var newOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					var modifiedOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );

					orders.AddRange( newOrdersWithinPage.Orders.Union( modifiedOrdersWithinPage.Orders ).ToList() );
					hasOrders = newOrdersWithinPage.Orders.Any() || modifiedOrdersWithinPage.Orders.Any();
				} );
			} while( hasOrders );

			this.FindMarketplaceIds( orders );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom.UtcToPst(), dateTo.UtcToPst() );
			var hasOrders = true;

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );
				pagesCount++;

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var newOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					var modifiedOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );

					orders.AddRange( newOrdersWithinPage.Orders.Union( modifiedOrdersWithinPage.Orders ).ToList() );
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
			ActionPolicies.Submit.Do( () => this._webRequestServices.PostData( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson() ) );
		}

		public async Task UpdateOrderAsync( ShipStationOrder order )
		{
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				await this._webRequestServices.PostDataAsync( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson() );
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