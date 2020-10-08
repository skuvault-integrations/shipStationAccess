using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Netco.Extensions;
using Newtonsoft.Json;
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
using System.Threading;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService: IShipStationService
	{
		private readonly WebRequestServices _webRequestServices;
		private readonly ShipStationOperationsTimeouts _timeouts;

		// lowered max limit for less order loss on Shipsation API's internal errors
		private const int RequestMaxLimit = 20;

		private DateTime _lastActivityTime;

		public ShipStationService( ShipStationCredentials credentials, ShipStationOperationsTimeouts timeouts )
		{
			this._webRequestServices = new WebRequestServices( credentials );
			this._timeouts = timeouts;
			_lastActivityTime = DateTime.UtcNow;
		}

		public ShipStationService( ShipStationCredentials credentials ) : this( credentials, new ShipStationOperationsTimeouts() ) { }

		public DateTime LastActivityTime
		{
			get
			{
				return this._webRequestServices.LastNetworkActivityTime ?? DateTime.UtcNow;
			}
		}

		public IEnumerable< ShipStationTag > GetTags( CancellationToken token )
		{
			var tags = new List< ShipStationTag >();
			ActionPolicies.Get.Do( () =>
			{
				tags = this._webRequestServices.GetResponse< List< ShipStationTag > >( ShipStationCommand.GetTags, string.Empty, token, _timeouts[ ShipStationOperationEnum.GetTags ] );
			} );

			return tags;
		}

		public async Task< IEnumerable< ShipStationTag > > GetTagsAsync( CancellationToken token )
		{
			var tags = new List< ShipStationTag >();
			await ActionPolicies.GetAsync.Do( async () =>
			{
				tags = await this._webRequestServices.GetResponseAsync< List< ShipStationTag > >( ShipStationCommand.GetTags, string.Empty, token, _timeouts[ ShipStationOperationEnum.GetTags ] );
			} );

			return tags;
		}

		public async Task < ShipStationShippingLabel > CreateAndGetShippingLabelAsync( string shipStationOrderId, string carrierCode, string serviceCode, string packageCode, string confirmation, DateTime shipDate, string weight, string weightUnit, CancellationToken token, bool isTestLabel = false )
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
					return await this._webRequestServices.PostDataAndGetResponseAsync< ShipStationShippingLabel >( ShipStationCommand.GetShippingLabel, endpoint, token, true, _timeouts[ ShipStationOperationEnum.GetShippingLabel ] );

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
		public IEnumerable< ShipStationOrder > GetOrders( DateTime dateFrom, DateTime dateTo, CancellationToken token, Func< ShipStationOrder, ShipStationOrder > processOrder = null )
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
						var ordersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint, token, _timeouts[ ShipStationOperationEnum.ListOrders ] );
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

			this.FindMarketplaceIds( orders, token );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token, bool getShipmentsAndFulfillments = true, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null )
		{
			var orders = new List< ShipStationOrder >();
			var processedOrderIds = new HashSet< long >();

			Func< ShipStationOrders, Task > processOrders = async sorders =>
			{
				var processedOrders = await sorders.Orders.ProcessInBatchAsync( 5, async o =>
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
					if ( getShipmentsAndFulfillments )
					{
						order.Shipments = await GetOrderShipmentsByIdAsync( order.OrderId.ToString(), token ).ConfigureAwait( false );
						order.Fulfillments = await GetOrderFulfillmentsByIdAsync( order.OrderId.ToString(), token ).ConfigureAwait( false );
					}

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

					ShipStationOrders ordersWithinPage = null;
					try
					{
						await ActionPolicies.GetAsync.Do( async () =>
						{
							ordersWithinPage = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint, token, _timeouts[ ShipStationOperationEnum.ListOrders ] );
						} );
					}
					catch( WebException e )
					{
						if( WebRequestServices.CanSkipException( e ) )
						{
							ShipStationLogger.Log.Warn( e, "Skipped get orders request page {pageNumber} of request {request} due to internal error on ShipStation's side", currentPage, ordersEndPoint );
						}
						else
							throw;
					}

					currentPage++;

					if( ordersWithinPage != null )
					{
						if( pagesCount == int.MaxValue )
						{
							pagesCount = ordersWithinPage.TotalPages;
							ordersExpected = ordersWithinPage.TotalOrders;
						}

						ordersCount += ordersWithinPage.Orders.Count;

						await processOrders( ordersWithinPage );
					}
				} while( currentPage <= pagesCount );

				ShipStationLogger.Log.Info( "Orders dowloaded API '{apiKey}' - {orders}/{expectedOrders} orders in {pages}/{expectedPages} from {endpoint}", _webRequestServices.GetApiKey(), ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			await downloadOrders( newOrdersEndpoint );

			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			await downloadOrders( modifiedOrdersEndpoint );

			await this.FindMarketplaceIdsAsync( orders, token );

			return orders;
		}
		
		public IEnumerable< ShipStationOrder > GetOrders( string storeId, string orderNumber, CancellationToken token )
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
						var ordersWithinPage = this._webRequestServices.GetResponse< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint, token, _timeouts[ ShipStationOperationEnum.ListOrders ] );
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

		public ShipStationOrder GetOrderById( string orderId, CancellationToken token )
		{
			ShipStationOrder order = null;
			ActionPolicies.Get.Do( () =>
			{
				order = this._webRequestServices.GetResponse< ShipStationOrder >( ShipStationCommand.GetOrder, "/" + orderId, token, _timeouts[ ShipStationOperationEnum.GetOrder ]);
			} );

			return order;
		}

		public async Task< ShipStationOrder > GetOrderByIdAsync( string orderId, CancellationToken token )
		{
			ShipStationOrder order = null;
			await ActionPolicies.GetAsync.Do( async () =>
			{
				order = await this._webRequestServices.GetResponseAsync< ShipStationOrder >( ShipStationCommand.GetOrder, "/" + orderId, token, _timeouts[ ShipStationOperationEnum.GetOrder ] );
			} );

			return order;
		}

		public async Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByIdAsync( string orderId, CancellationToken token )
		{
			var orderShipments = new List< ShipStationOrderShipment >();

			var currentPage = 1;
			var pagesCount = int.MaxValue;

			do
			{
				var getOrderShipmentsEndpoint = ParamsBuilder.CreateOrderShipmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
				var orderShipmentsByPageEndPoint = getOrderShipmentsEndpoint.ConcatParams( nextPageParams );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var orderShipmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderShipments >( ShipStationCommand.GetOrderShipments, orderShipmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderShipments ] );
				
					++currentPage;
					if ( pagesCount == int.MaxValue )
					{
						pagesCount = orderShipmentsPage.Pages + 1;
					}

					orderShipments.AddRange( orderShipmentsPage.Shipments );

				} );
			}
			while( currentPage <= pagesCount );

			return orderShipments;
		}

		public async Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByIdAsync( string orderId, CancellationToken token )
		{
			var orderFulfillments = new List< ShipStationOrderFulfillment >();

			var currentPage = 1;
			var pagesCount = int.MaxValue;

			do
			{
				var getOrderFulfillmentsEndpoint = ParamsBuilder.CreateOrderFulfillmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, RequestMaxLimit ) );
				var orderFulfillmentsByPageEndPoint = getOrderFulfillmentsEndpoint.ConcatParams( nextPageParams );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var orderFulfillmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderFulfillments >( ShipStationCommand.GetOrderFulfillments, orderFulfillmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderFulfillments ] );
				
					++currentPage;
					if ( pagesCount == int.MaxValue )
					{
						pagesCount = orderFulfillmentsPage.Pages + 1;
					}

					orderFulfillments.AddRange( orderFulfillmentsPage.Fulfillments );

				} );
			}
			while( currentPage <= pagesCount );

			return orderFulfillments;
		}
		#endregion

		#region Update Orders
		public void UpdateOrder( ShipStationOrder order, CancellationToken token )
		{
			if( !order.IsValid() )
				return;

			ActionPolicies.Submit.Do( () =>
			{
				try
				{
					this._webRequestServices.PostData( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson(), token, _timeouts[ ShipStationOperationEnum.CreateOrder ] );
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

		public async Task UpdateOrderAsync( ShipStationOrder order, CancellationToken token )
		{
			if( !order.IsValid() )
				return;

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				try
				{
					await this._webRequestServices.PostDataAsync( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson(), token, _timeouts[ ShipStationOperationEnum.CreateOrder ] );
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
		public void UpdateOrderItemsWarehouseLocations( ShipStationWarehouseLocations warehouseLocations, CancellationToken token )
		{
			foreach( var warehouseLocation in warehouseLocations.GetWarehouseLocationsToSend() )
			{
				this.UpdateOrderItemsWarehouseLocation( warehouseLocation, token );
			}
		}

		public async Task UpdateOrderItemsWarehouseLocationsAsync( ShipStationWarehouseLocations warehouseLocations, CancellationToken token )
		{
			foreach( var warehouseLocation in warehouseLocations.GetWarehouseLocationsToSend() )
			{
				await this.UpdateOrderItemsWarehouseLocationAsync( warehouseLocation, token );
			}
		}

		public void UpdateOrderItemsWarehouseLocation( ShipStationWarehouseLocation warehouseLocation, CancellationToken token )
		{
			var json = warehouseLocation.SerializeToJson();
			ActionPolicies.Submit.Do( () =>
			{
				this._webRequestServices.PostData( ShipStationCommand.UpdateOrderItemsWarehouseLocation, json, token, _timeouts[ ShipStationOperationEnum.UpdateWarehouseLocation ] );
			} );
		}

		public async Task UpdateOrderItemsWarehouseLocationAsync( ShipStationWarehouseLocation warehouseLocation, CancellationToken token )
		{
			var json = warehouseLocation.SerializeToJson();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				await this._webRequestServices.PostDataAsync( ShipStationCommand.UpdateOrderItemsWarehouseLocation, json, token, _timeouts[ ShipStationOperationEnum.UpdateWarehouseLocation ] );
			} );
		}
		#endregion

		#region Get Stores
		public IEnumerable< ShipStationStore > GetStores( CancellationToken token )
		{
			var stores = new List< ShipStationStore >();
			ActionPolicies.Submit.Do( () =>
			{
				stores = this._webRequestServices.GetResponse< List< ShipStationStore > >( ShipStationCommand.GetStores, ParamsBuilder.EmptyParams, token, _timeouts[ ShipStationOperationEnum.GetStores ] );
			} );
			return stores;
		}

		public async Task< IEnumerable< ShipStationStore > > GetStoresAsync( CancellationToken token )
		{
			var stores = new List< ShipStationStore >();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				stores = await this._webRequestServices.GetResponseAsync< List< ShipStationStore > >( ShipStationCommand.GetStores, ParamsBuilder.EmptyParams, token, _timeouts[ ShipStationOperationEnum.GetStores ] );
			} );
			return stores;
		}
		#endregion

		#region Register
		public ShipStationRegisterResponse Register( ShipStationRegister register, CancellationToken token )
		{
			ShipStationRegisterResponse response = null;
			ActionPolicies.Submit.Do( () =>
			{
				try
				{
					ShipStationLogger.Log.Info( "Try send register request. Command: {command}. Register: {register}", ShipStationCommand.Register.Command, register );
					response = this._webRequestServices.PostDataAndGetResponseWithShipstationHeader< ShipStationRegisterResponse >( ShipStationCommand.Register, register.SerializeToJson(), token, true, _timeouts[ ShipStationOperationEnum.Register ] );
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
		private void FindMarketplaceIds( IEnumerable< ShipStationOrder > orders, CancellationToken token )
		{
			var stores = this.GetStores( token );

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

		private async Task FindMarketplaceIdsAsync( IEnumerable< ShipStationOrder > orders, CancellationToken token )
		{
			var stores = await this.GetStoresAsync( token );

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