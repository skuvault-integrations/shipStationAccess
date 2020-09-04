using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Netco.Extensions;
using ShipStationAccess.V2.Exceptions;
using ShipStationAccess.V2.Misc;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Command;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Models.ShippingLabel;
using ShipStationAccess.V2.Models.Store;
using ShipStationAccess.V2.Models.TagList;
using ShipStationAccess.V2.Models.WarehouseLocation;
using ShipStationAccess.V2.Services;
using ShipStationAccess.V2.Models.Register;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService: IShipStationService
	{
		private readonly WebRequestServices _webRequestServices;
		// lowered max limit for less order loss on Shipsation API's internal errors
		private const int RequestMaxLimit = 20;

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

		public async Task< IEnumerable< ShipStationTag > > GetTagsAsync()
		{
			var tags = new List< ShipStationTag >();
			await ActionPolicies.GetAsync.Do( async () =>
			{
				tags = await this._webRequestServices.GetResponseAsync< List< ShipStationTag > >( ShipStationCommand.GetTags, string.Empty );
			} );

			return tags;
		}

		public async Task < ShipStationShippingLabel > CreateAndGetShippingLabelAsync( string shipStationOrderId, string carrierCode, string serviceCode, string packageCode, string confirmation, DateTime shipDate, string weight, string weightUnit, bool isTestLabel = false )
		{
			//return ShipStationShippingLabel.GetMockShippingLabel();
			return await ActionPolicies.GetAsync.Get( async () =>
			{
				try
				{
					if( string.IsNullOrWhiteSpace( carrierCode ) || string.IsNullOrWhiteSpace( serviceCode ) )
						throw new ShipStationLabelException( "Has a carrier been selected in ShipStation for this order?" );
					if( string.IsNullOrWhiteSpace( confirmation ) )
						throw new ShipStationLabelException( "Has a confirmation type been selected in ShipStation for this order?" );
					var endpoint = ShipStationShippingLabelRequest.From( shipStationOrderId, carrierCode, serviceCode, packageCode, confirmation, shipDate, weight, weightUnit, isTestLabel ).SerializeToJson();
					return await this._webRequestServices.PostDataAndGetResponseAsync< ShipStationShippingLabel >( ShipStationCommand.GetShippingLabel, endpoint, true );

				}
				catch( Exception ex )
				{
					if( ex.InnerException is WebException )
						throw new ShipStationLabelException( ex.Message );
					throw new ShipStationLabelException( "Please verify this order has the correct shipping address and carrier settings in ShipStation." );
				}
			} );
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

				ShipStationLogger.Log.Info( "Orders downloaded API '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}", _webRequestServices.GetApiKey(), ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			downloadOrders( newOrdersEndpoint );

			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			downloadOrders( modifiedOrdersEndpoint );

			this.FindMarketplaceIds( orders );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, bool getShipmentsAndFulfillments = false, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null, Action< IEnumerable< ReadError > > handleSkippedOrders = null )
		{
			var allOrders = new List< ShipStationOrder >();
			var createdOrders = await this.GetCreatedOrdersAsync( dateFrom, dateTo ).ConfigureAwait( false );
			allOrders.AddRange( createdOrders.Data );

			var modifiedOrders = await this.GetModifiedOrdersAsync( dateFrom, dateTo ).ConfigureAwait( false );
			allOrders.AddRange( modifiedOrders.Data );

			var uniqueOrders = allOrders.GroupBy( o => o.OrderId ).Select( gr => gr.First() ).ToList();
			var processedOrders = await uniqueOrders.ProcessInBatchAsync( 5, async order =>
			{
				if( processOrder != null )
					order = await processOrder( order ).ConfigureAwait( false );

				return order;
			} );

			await this.FindMarketplaceIdsAsync( processedOrders ).ConfigureAwait( false );

			if ( getShipmentsAndFulfillments )
				await this.FindShipmentsAndFulfillments( processedOrders ).ConfigureAwait( false );

			if ( handleSkippedOrders != null )
			{
				var allSkippedOrders = new List< ReadError >();
				allSkippedOrders.AddRange( createdOrders.ReadErrors );
				allSkippedOrders.AddRange( modifiedOrders.ReadErrors );

				if ( allSkippedOrders.Any() )
					handleSkippedOrders( allSkippedOrders );
			}

			return processedOrders;
		}

		public async Task< PaginatedResponse< ShipStationOrder > > GetCreatedOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var createdOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			var createdOrdersResponse = await this.DownloadOrdersAsync( createdOrdersEndpoint ).ConfigureAwait( false );
			if ( createdOrdersResponse.Data.Any() )
			{
				ShipStationLogger.Log.Info( "Created orders downloaded using tenant's API key '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}", 
					_webRequestServices.GetApiKey(), 
					createdOrdersResponse.Data.Count(), 
					createdOrdersResponse.TotalEntitiesExpected ?? 0, 
					createdOrdersResponse.TotalPagesReceived, 
					createdOrdersResponse.TotalPagesExpected ?? 0, 
					createdOrdersResponse );
			}

			return createdOrdersResponse;
		}

		public async Task< PaginatedResponse< ShipStationOrder > > GetModifiedOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			var modifiedOrdersResponse = await this.DownloadOrdersAsync( modifiedOrdersEndpoint ).ConfigureAwait( false );
			if ( modifiedOrdersResponse.Data.Any() )
			{
				ShipStationLogger.Log.Info( "Modified orders downloaded using tenant's API key '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}", 
									_webRequestServices.GetApiKey(), 
									modifiedOrdersResponse.Data.Count(), 
									modifiedOrdersResponse.TotalEntitiesExpected ?? 0, 
									modifiedOrdersResponse.TotalPagesReceived, 
									modifiedOrdersResponse.TotalPagesExpected ?? 0, 
									modifiedOrdersResponse );
			}

			return modifiedOrdersResponse;
		}

		public async Task< PaginatedResponse< ShipStationOrder > > DownloadOrdersAsync( string endPoint )
		{ 
			var response = new PaginatedResponse< ShipStationOrder >();
			var currentPage = 1;

			do
			{
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
				var ordersEndPoint = endPoint.ConcatParams( nextPageParams );

				ShipStationOrders ordersPage = null;
				try
				{
					await ActionPolicies.GetAsync.Do( async () =>
					{
						ordersPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint ).ConfigureAwait( false );
					} );
				}
				catch( WebException e )
				{
					if( WebRequestServices.CanSkipException( e ) )
					{
						response.ReadErrors.Add( new ReadError()
						{
							Url = endPoint,
							Page = currentPage,
							PageSize = RequestMaxLimit
						} );

						ShipStationLogger.Log.Warn( e, "Skipped get orders request page {pageNumber} of request {request} due to internal error on ShipStation's side", currentPage, ordersEndPoint );
						currentPage++;
						continue;
					}
					else
						throw;
				}

				currentPage++;

				if ( ordersPage?.Orders == null || !ordersPage.Orders.Any() )
					break;

				response.TotalPagesExpected = ordersPage.TotalPages;
				response.TotalEntitiesExpected = ordersPage.TotalOrders;

				response.Data.AddRange( ordersPage.Orders );

			} while( currentPage <= response.TotalPagesExpected );
			
			response.TotalPagesReceived = currentPage - 1;
			
			return response;
		}

		public IEnumerable< ShipStationOrder > GetOrders( string storeId, string orderNumber )
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

				ShipStationLogger.Log.Info( "SS Labels Get Orders '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}", this._webRequestServices.GetApiKey(), ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateStoreIdOrderNumberParams( storeId, orderNumber );
			downloadOrders( newOrdersEndpoint );
			return orders;
		}

		public ShipStationOrder GetOrderById( string orderId )
		{
			ShipStationOrder order = null;
			ActionPolicies.Get.Do( () =>
			{
				order = this._webRequestServices.GetResponse< ShipStationOrder >( ShipStationCommand.GetOrder, "/" + orderId );
			} );

			return order;
		}

		public async Task< ShipStationOrder > GetOrderByIdAsync( string orderId )
		{
			ShipStationOrder order = null;
			await ActionPolicies.GetAsync.Do( async () =>
			{
				order = await this._webRequestServices.GetResponseAsync< ShipStationOrder >( ShipStationCommand.GetOrder, "/" + orderId );
			} );

			return order;
		}

		private async Task FindShipmentsAndFulfillments( IEnumerable< ShipStationOrder > orders )
		{
			foreach( var order in orders )
			{
				order.Shipments = await this.GetOrderShipmentsByIdAsync( order.OrderId.ToString() ).ConfigureAwait( false );
				order.Fulfillments = await this.GetOrderFulfillmentsByIdAsync( order.OrderId.ToString() ).ConfigureAwait( false );
			}
		}

		public async Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByIdAsync( string orderId )
		{
			var orderShipments = new List< ShipStationOrderShipment >();

			var currentPage = 1;
			int? totalShipStationShipmentsPages;

			do
			{
				var getOrderShipmentsEndpoint = ParamsBuilder.CreateOrderShipmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
				var orderShipmentsByPageEndPoint = getOrderShipmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderShipments ordersShipmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					ordersShipmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderShipments >( ShipStationCommand.GetOrderShipments, orderShipmentsByPageEndPoint ).ConfigureAwait( false );
				} );

				if ( ordersShipmentsPage?.Shipments == null || !ordersShipmentsPage.Shipments.Any() )
					break;

				++currentPage;
				totalShipStationShipmentsPages = ordersShipmentsPage.Pages + 1;

				orderShipments.AddRange( ordersShipmentsPage.Shipments );
			}
			while( currentPage <= totalShipStationShipmentsPages );

			return orderShipments;
		}

		public async Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByIdAsync( string orderId )
		{
			var orderFulfillments = new List< ShipStationOrderFulfillment >();

			var currentPage = 1;
			int? totalShipStationFulfillmentsPages;

			do
			{
				var getOrderFulfillmentsEndpoint = ParamsBuilder.CreateOrderFulfillmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
				var orderFulfillmentsByPageEndPoint = getOrderFulfillmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderFulfillments orderFulfillmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					orderFulfillmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderFulfillments >( ShipStationCommand.GetOrderFulfillments, orderFulfillmentsByPageEndPoint ).ConfigureAwait( false );
				} );

				if ( orderFulfillmentsPage?.Fulfillments == null || !orderFulfillmentsPage.Fulfillments.Any() )
					break;

				++currentPage;
				totalShipStationFulfillmentsPages = orderFulfillmentsPage.Pages + 1;

				orderFulfillments.AddRange( orderFulfillmentsPage.Fulfillments );
			}
			while( currentPage <= totalShipStationFulfillmentsPages );

			return orderFulfillments;
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
						ShipStationLogger.Log.Error( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
					else if( x.Response.GetHttpStatusCode() == HttpStatusCode.NotFound )
						ShipStationLogger.Log.Error( "Error updating order. Encountered 404 Not Found. Order: {order}", order );
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
						ShipStationLogger.Log.Error( "Error updating order. Encountered 500 Internal Error. Order: {order}", order );
					else if( x.Response.GetHttpStatusCode() == HttpStatusCode.NotFound )
						ShipStationLogger.Log.Error( "Error updating order. Encountered 404 Not Found. Order: {order}", order );
					else
						throw;
				}
			} );
		}
		#endregion

		#region Update Order Items Warehouse Locations
		public void UpdateOrderItemsWarehouseLocations( ShipStationWarehouseLocations warehouseLocations )
		{
			foreach( var warehouseLocation in warehouseLocations.GetWarehouseLocationsToSend() )
			{
				this.UpdateOrderItemsWarehouseLocation( warehouseLocation );
			}
		}

		public async Task UpdateOrderItemsWarehouseLocationsAsync( ShipStationWarehouseLocations warehouseLocations )
		{
			foreach( var warehouseLocation in warehouseLocations.GetWarehouseLocationsToSend() )
			{
				await this.UpdateOrderItemsWarehouseLocationAsync( warehouseLocation );
			}
		}

		public void UpdateOrderItemsWarehouseLocation( ShipStationWarehouseLocation warehouseLocation )
		{
			var json = warehouseLocation.SerializeToJson();
			ActionPolicies.Submit.Do( () =>
			{
				this._webRequestServices.PostData( ShipStationCommand.UpdateOrderItemsWarehouseLocation, json );
			} );
		}

		public async Task UpdateOrderItemsWarehouseLocationAsync( ShipStationWarehouseLocation warehouseLocation )
		{
			var json = warehouseLocation.SerializeToJson();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				await this._webRequestServices.PostDataAsync( ShipStationCommand.UpdateOrderItemsWarehouseLocation, json );
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

		#region Register
		public ShipStationRegisterResponse Register( ShipStationRegister register )
		{
			ShipStationRegisterResponse response = null;
			ActionPolicies.Submit.Do( () =>
			{
				try
				{
					ShipStationLogger.Log.Info( "Try send register request. Command: {command}. Register: {register}", ShipStationCommand.Register.Command, register );
					response = this._webRequestServices.PostDataAndGetResponseWithShipstationHeader< ShipStationRegisterResponse >( ShipStationCommand.Register, register.SerializeToJson(), true );
					ShipStationLogger.Log.Info( "Try send register request is success. Command: {command}. Register: {register}", ShipStationCommand.Register.Command, register );
				}
				catch( Exception ex )
				{
					ShipStationLogger.Log.Error( "Try send register request is fail. Command: {command}. Register: {register}", ShipStationCommand.Register.Command, register );
					throw new ShipStationRegisterException( ex.Message );
				}
			} );

			return response;
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
				{
					order.MarketplaceId = store.MarketplaceId;
					order.MarketplaceName = store.MarketplaceName;
				}
			}
		}

		private async Task FindMarketplaceIdsAsync( IEnumerable< ShipStationOrder > orders )
		{
			var stores = await this.GetStoresAsync().ConfigureAwait( false );

			foreach( var order in orders )
			{
				var store = stores.FirstOrDefault( s => s.StoreId == order.AdvancedOptions.StoreId );
				if( store != null )
				{
					order.MarketplaceId = store.MarketplaceId;
					order.MarketplaceName = store.MarketplaceName;
				}
			}
		}
		#endregion
	}
}