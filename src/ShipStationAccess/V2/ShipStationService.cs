using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Services;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService : IShipStationService
	{
		private readonly WebRequestServices _webRequestServices;
		private const int RequestMaxLimit = 1;
		private readonly TimeSpan DefaultApiDelay = TimeSpan.FromMilliseconds( 150 );

		public ShipStationService( ShipStationCredentials credentials )
		{
			this._webRequestServices = new WebRequestServices( credentials );
		}

		#region Get
		public IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			var hasOrders = true;

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount + 1, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );

				ActionPolicies.Get.Do( () =>
				{
					var newOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					var modifiedOrdersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );

					orders.AddRange( newOrdersWithinPage.Orders.Union( modifiedOrdersWithinPage.Orders ).ToList() );
					hasOrders = newOrdersWithinPage.Orders.Any() || modifiedOrdersWithinPage.Orders.Any();
				} );

				//API requirement
				this.CreateApiDelay().Wait();
				pagesCount++;
			} while( hasOrders );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var pagesCount = 1;
			var orders = new List< ShipStationOrder >();
			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			var hasOrders = true;

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( pagesCount + 1, RequestMaxLimit ) );
				var compositeNewOrdersEndpoint = newOrdersEndpoint.ConcatParams( nextPageParams );
				var compositeModifiedOrdersEndpoint = modifiedOrdersEndpoint.ConcatParams( nextPageParams );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var newOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeNewOrdersEndpoint );
					var modifiedOrdersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, compositeModifiedOrdersEndpoint );

					orders.AddRange( newOrdersWithinPage.Orders.Union( modifiedOrdersWithinPage.Orders ).ToList() );
					hasOrders = newOrdersWithinPage.Orders.Any() || modifiedOrdersWithinPage.Orders.Any();
				} );

				//API requirement
				this.CreateApiDelay().Wait();
				pagesCount++;
			} while( hasOrders );

			return orders;
		}
		#endregion

		#region Update
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

		#region Misc
		private Task CreateApiDelay()
		{
			return Task.Delay( this.DefaultApiDelay );
		}
		#endregion
	}
}