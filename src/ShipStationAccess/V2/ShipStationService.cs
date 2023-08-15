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
using System.Threading;
using SkuVault.Integrations.Core.Common;

namespace ShipStationAccess.V2
{
	public sealed class ShipStationService: IShipStationService
	{
		private readonly IWebRequestServices _webRequestServices;
		private readonly SyncRunContext _syncRunContext;
		private readonly ShipStationTimeouts _timeouts;
		private const int OrdersPageSize = 500;
		// Max orders that can be processed without needing complex shipping and fulfillment handling.
		private const int OrderProcessingThreshold = 10;

		internal ShipStationService( IWebRequestServices webRequestServices, SyncRunContext syncRunContext, ShipStationTimeouts timeouts )
		{
			this._webRequestServices = webRequestServices;
			this._syncRunContext = syncRunContext;
			this._timeouts = timeouts;
		}

		/// <summary>
		///	Last service's network activity time. Can be used to monitor service's state.
		/// </summary>
		public DateTime LastActivityTime => this._webRequestServices.LastNetworkActivityTime ?? DateTime.UtcNow;

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
					if( ex is ShipStationThrottleException )
						throw;
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
					
					// Orders with status 'rejected_fulfillment' should be ignored. VT-5738
					if( curOrder.OrderStatus == ShipStationOrderStatusEnum.rejected_fulfillment )
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
					var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
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

				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Orders downloaded - {Orders}/{ExpectedOrders} orders in {Pages}/{ExpectedPages} from {Endpoint}", 
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(ShipStationService),
					nameof(GetOrders),
					ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
			};

			var newOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			downloadOrders( newOrdersEndpoint );

			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			downloadOrders( modifiedOrdersEndpoint );

			this.FindMarketplaceIds( orders, token );

			return orders;
		}

		public async Task< IEnumerable< ShipStationOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token, bool getShipmentsAndFulfillments = false, Func< ShipStationOrder, Task< ShipStationOrder > > processOrder = null, Action< IEnumerable< ReadError > > handleSkippedOrders = null )
		{
			var allOrders = new List< ShipStationOrder >();
			var createdOrders = await this.GetCreatedOrdersAsync( dateFrom, dateTo, token ).ConfigureAwait( false );
			allOrders.AddRange( createdOrders.Data );

			var modifiedOrders = await this.GetModifiedOrdersAsync( dateFrom, dateTo, token ).ConfigureAwait( false );
			allOrders.AddRange( modifiedOrders.Data );

			var uniqueOrders = allOrders.GroupBy( o => o.OrderId ).Select( gr => gr.First() ).ToList();
			var processedOrders = await uniqueOrders.ProcessInBatchAsync( 5, async order =>
			{
				if( processOrder != null )
					order = await processOrder( order ).ConfigureAwait( false );

				return order;
			} );

			await this.FindMarketplaceIdsAsync( processedOrders, token ).ConfigureAwait( false );

			if ( getShipmentsAndFulfillments )
				await this.FindShipmentsAndFulfillments( processedOrders, token ).ConfigureAwait( false );

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

		public async Task< SummaryResponse< ShipStationOrder > > GetCreatedOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var createdOrdersEndpoint = ParamsBuilder.CreateNewOrdersParams( dateFrom, dateTo );
			var createdOrdersResponse = new SummaryResponse< ShipStationOrder >();
			await this.DownloadOrdersAsync( createdOrdersResponse, createdOrdersEndpoint, 1, OrdersPageSize, token ).ConfigureAwait( false );
			if ( createdOrdersResponse.Data.Any() )
			{
				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Created orders downloaded - {Orders}/{ExpectedOrders} orders from {Endpoint}. Response: '{Response}'",
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(ShipStationService),
					nameof(this.GetCreatedOrdersAsync),
					createdOrdersResponse.Data.Count, createdOrdersResponse.TotalEntitiesExpected ?? 0, createdOrdersEndpoint, createdOrdersResponse );
			}

			return createdOrdersResponse;
		}

		public async Task< SummaryResponse< ShipStationOrder > > GetModifiedOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken token )
		{
			var modifiedOrdersEndpoint = ParamsBuilder.CreateModifiedOrdersParams( dateFrom, dateTo );
			var modifiedOrdersResponse = new SummaryResponse< ShipStationOrder >();
			await this.DownloadOrdersAsync( modifiedOrdersResponse, modifiedOrdersEndpoint, 1, OrdersPageSize, token ).ConfigureAwait( false );
			if ( modifiedOrdersResponse.Data.Any() )
			{
				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Modified orders downloaded - {Orders}/{ExpectedOrders} orders from {Endpoint}. Response: '{Response}'",
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(ShipStationService),
					nameof(this.GetModifiedOrdersAsync),
					modifiedOrdersResponse.Data.Count, modifiedOrdersResponse.TotalEntitiesExpected ?? 0, modifiedOrdersEndpoint, modifiedOrdersResponse );
			}

			return modifiedOrdersResponse;
		}

		/// <summary>
		///	Download all orders from specific endpoint
		/// </summary>
		/// <param name="endPoint">API endpoint</param>
		/// <param name="currentPage">page index</param>
		/// <param name="currentPageSize">page size</param>
		/// <param name="token">cancellation token</param>
		/// <returns></returns>
		public async Task DownloadOrdersAsync( SummaryResponse< ShipStationOrder > summary, string endPoint, int currentPage, int currentPageSize, CancellationToken token )
		{ 
			while ( true )
			{
				var ordersPage = await DownloadOrdersPageAsync( endPoint, currentPage, currentPageSize, token ).ConfigureAwait( false );
				if ( ordersPage.HasInternalError )
				{
					if ( currentPageSize == 1 )
					{
						summary.ReadErrors.Add( new ReadError()
						{
							Url = endPoint,
							Page = currentPage,
							PageSize = currentPageSize
						} );

						summary.TotalEntitiesHandled += 1;
						currentPage++;

						ShipStationLogger.Log.Warn( Constants.LoggingCommonPrefix + "Skipped order on pos {OrderPos} of request {Request} due to internal error on ShipStation's side",
							Constants.ChannelName,
							Constants.VersionInfo, 
							this._syncRunContext.TenantId,
							this._syncRunContext.ChannelAccountId,
							this._syncRunContext.CorrelationId,
							nameof(ShipStationService),
							nameof(this.DownloadOrdersAsync),
							summary.TotalEntitiesHandled - 1, endPoint );
						continue;
					}

					currentPageSize = PageSizeAdjuster.GetHalfPageSize( currentPageSize );
					currentPage = PageSizeAdjuster.GetNextPageIndex( summary.TotalEntitiesHandled, currentPageSize );

					summary.TotalEntitiesHandled -= summary.TotalEntitiesHandled % currentPageSize;
					ShipStationLogger.Log.Warn( Constants.LoggingCommonPrefix + "Trying to decrease orders page size twice due to internal error on ShipStation's side. Current page size: {OrdersPageSize}, orders downloaded {OrdersDownloaded}",
						Constants.ChannelName,
						Constants.VersionInfo, 
						this._syncRunContext.TenantId,
						this._syncRunContext.ChannelAccountId,
						this._syncRunContext.CorrelationId,
						nameof(ShipStationService),
						nameof(this.DownloadOrdersAsync),
						currentPageSize, summary.TotalEntitiesHandled );

					await this.DownloadOrdersAsync( summary, endPoint, currentPage, currentPageSize, token ).ConfigureAwait( false );
					return;
				}

				if ( !ordersPage.Data.Any() )
				{
					return;
				}

				summary.TotalEntitiesExpected = ordersPage.TotalEntities;
				summary.TotalEntitiesHandled += ordersPage.Data.Count;
				summary.Data.AddRange( ordersPage.Data );
				currentPage++;

				if ( currentPageSize != OrdersPageSize )
				{
					var newPageSize = PageSizeAdjuster.DoublePageSize( currentPageSize, OrdersPageSize );
					var newCurrentPage = PageSizeAdjuster.GetNextPageIndex( summary.TotalEntitiesHandled, newPageSize );

					if ( !PageSizeAdjuster.AreEntitiesWillBeDownloadedAgainAfterChangingThePageSize( newCurrentPage, newPageSize, summary.TotalEntitiesHandled ) )
					{
						await DownloadOrdersAsync( summary, endPoint, newCurrentPage, newPageSize, token ).ConfigureAwait( false );
						return;
					}
				}
			}
		}

		/// <summary>
		///	Downloads orders page
		/// </summary>
		/// <param name="endpoint">API endpoint</param>
		/// <param name="page">page index</param>
		/// <param name="pageSize">page size</param>
		/// <param name="token">cancellation token</param>
		/// <returns></returns>
		private async Task< PageResponse< ShipStationOrder > > DownloadOrdersPageAsync( string endPoint, int page, int pageSize, CancellationToken token )
		{
			var ordersPage = new PageResponse< ShipStationOrder >();
			var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( page, pageSize ) );
			var ordersEndPoint = endPoint.ConcatParams( nextPageParams );

			ShipStationOrders response = null;
			try
			{
				await ActionPolicies.GetAsync.Do( async () =>
				{
					response = await this._webRequestServices.GetResponseAsync< ShipStationOrders >( ShipStationCommand.GetOrders, ordersEndPoint, token, _timeouts[ ShipStationOperationEnum.ListOrders ] ).ConfigureAwait( false );
				} );
			}
			catch( WebException e )
			{
				if( this._webRequestServices.CanSkipException( e ) )
				{
					ordersPage.HasInternalError = true;
					return ordersPage;
				}
			}

			if ( response == null )
				return ordersPage;

			ordersPage.TotalPages = response.TotalPages;
			ordersPage.TotalEntities = response.TotalOrders;

			if ( response.Orders != null && response.Orders.Any() )
				ordersPage.Data.AddRange( response.Orders );

			return ordersPage;
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
					var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
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

				ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "SS Labels Get Orders - {Orders}/{ExpectedOrders} orders in {Pages}/{ExpectedPages} from {Endpoint}",
					Constants.ChannelName,
					Constants.VersionInfo, 
					this._syncRunContext.TenantId,
					this._syncRunContext.ChannelAccountId,
					this._syncRunContext.CorrelationId,
					nameof(ShipStationService),
					nameof(GetOrders),
					ordersCount, ordersExpected, currentPage - 1, pagesCount, endPoint );
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

		private async Task FindShipmentsAndFulfillments( IEnumerable< ShipStationOrder > orders, CancellationToken token )
		{
			if( orders.ToList().Count > OrderProcessingThreshold )
			{
				await GetShipmentsAndFullfilmentsByCreationDate( orders, token ).ConfigureAwait( false );
			}
			else
			{
				await GetShipmentsAndFullfilmentByOrderId( orders, token ).ConfigureAwait( false );
			}
		}

		/// <summary>
		/// Uses the minimum order's 'createDate' value to fetch all shipments and fulfillments created on or after that date.
		/// </summary>
		private async Task GetShipmentsAndFullfilmentsByCreationDate( IEnumerable< ShipStationOrder > orders, CancellationToken token )
		{
			var ordersList = orders.ToList();
			var minOrderDate = ordersList.Min( o => o.CreateDate );
			var shipments = await this.GetOrderShipmentsByCreatedDateAsync( minOrderDate, token ).ConfigureAwait( false );
			var fulfillments = await this.GetOrderFulfillmentsByCreatedDateAsync( minOrderDate, token ).ConfigureAwait( false );

			foreach( var order in ordersList )
			{
				order.Shipments = shipments.ToList().Where( s => s.OrderId == order.OrderId );
				order.Fulfillments = fulfillments.ToList().Where( f => f.OrderId == order.OrderId );
			}
		}

		/// <summary>
		/// Makes two separate calls to ShipStation's API to obtain shipments and fulfillment data per each order.
		/// </summary>
		private async Task GetShipmentsAndFullfilmentByOrderId( IEnumerable< ShipStationOrder > orders, CancellationToken token )
		{
			foreach( var order in orders.ToList() )
			{
				order.Shipments = await this.GetOrderShipmentsByIdAsync( order.OrderId.ToString(), token ).ConfigureAwait( false );
				order.Fulfillments = await this.GetOrderFulfillmentsByIdAsync( order.OrderId.ToString(), token ).ConfigureAwait( false );
			}
		}

		public async Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByIdAsync( string orderId, CancellationToken token )
		{
			var orderShipments = new List< ShipStationOrderShipment >();

			var currentPage = 1;
			int? totalShipStationShipmentsPages;

			do
			{
				var getOrderShipmentsEndpoint = ParamsBuilder.CreateOrderShipmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
				var orderShipmentsByPageEndPoint = getOrderShipmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderShipments ordersShipmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					ordersShipmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderShipments >( ShipStationCommand.GetOrderShipments, orderShipmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderShipments ] ).ConfigureAwait( false );
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
		
		public async Task< IEnumerable< ShipStationOrderShipment > > GetOrderShipmentsByCreatedDateAsync( DateTime createdDate, CancellationToken token )
		{
			var orderShipments = new List< ShipStationOrderShipment >();

			var currentPage = 1;
			int? totalShipStationShipmentsPages;

			do
			{
				var getOrderShipmentsEndpoint = ParamsBuilder.CreateOrderShipmentsParams( createdDate );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
				var orderShipmentsByPageEndPoint = getOrderShipmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderShipments ordersShipmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					ordersShipmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderShipments >( ShipStationCommand.GetOrderShipments, orderShipmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderShipments ] ).ConfigureAwait( false );
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

		public async Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByIdAsync( string orderId, CancellationToken token )
		{
			var orderFulfillments = new List< ShipStationOrderFulfillment >();

			var currentPage = 1;
			int? totalShipStationFulfillmentsPages;

			do
			{
				var getOrderFulfillmentsEndpoint = ParamsBuilder.CreateOrderFulfillmentsParams( orderId );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
				var orderFulfillmentsByPageEndPoint = getOrderFulfillmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderFulfillments orderFulfillmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					orderFulfillmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderFulfillments >( ShipStationCommand.GetOrderFulfillments, orderFulfillmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderFulfillments ] ).ConfigureAwait( false );
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
		
		public async Task< IEnumerable< ShipStationOrderFulfillment > > GetOrderFulfillmentsByCreatedDateAsync( DateTime createdDate, CancellationToken token )
		{
			var orderFulfillments = new List< ShipStationOrderFulfillment >();

			var currentPage = 1;
			int? totalShipStationFulfillmentsPages;

			do
			{
				var getOrderFulfillmentsEndpoint = ParamsBuilder.CreateOrderFulfillmentsParams( createdDate );
				var nextPageParams = ParamsBuilder.CreateGetNextPageParams( new ShipStationCommandConfig( currentPage, OrdersPageSize ) );
				var orderFulfillmentsByPageEndPoint = getOrderFulfillmentsEndpoint.ConcatParams( nextPageParams );

				ShipStationOrderFulfillments orderFulfillmentsPage = null;
				await ActionPolicies.GetAsync.Do( async () =>
				{
					orderFulfillmentsPage = await this._webRequestServices.GetResponseAsync< ShipStationOrderFulfillments >( ShipStationCommand.GetOrderFulfillments, orderFulfillmentsByPageEndPoint, token, _timeouts[ ShipStationOperationEnum.GetOrderFulfillments ] ).ConfigureAwait( false );
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
		public void UpdateOrder( ShipStationOrder order, CancellationToken token )
		{
			if( !order.IsValid() )
				return;

			ActionPolicies.Submit.Do( () =>
			{
				try
				{
					this._webRequestServices.PostData( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson(), token, _timeouts[ ShipStationOperationEnum.CreateUpdateOrder ] );
				}
				catch( WebException x )
				{
					if( x.Response.GetHttpStatusCode() == HttpStatusCode.InternalServerError )
						ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "Error updating order. Encountered 500 Internal Error. Order: {Order}",
							Constants.ChannelName,
							Constants.VersionInfo, 
							this._syncRunContext.TenantId,
							this._syncRunContext.ChannelAccountId,
							this._syncRunContext.CorrelationId,
							nameof(ShipStationService),
							nameof(this.UpdateOrder),
							order );
					else if( x.Response.GetHttpStatusCode() == HttpStatusCode.NotFound )
						ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "Error updating order. Encountered 404 Not Found. Order: {Order}",
							Constants.ChannelName,
							Constants.VersionInfo, 
							this._syncRunContext.TenantId,
							this._syncRunContext.ChannelAccountId,
							this._syncRunContext.CorrelationId,
							nameof(ShipStationService),
							nameof(this.UpdateOrder),
							order );
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
					await this._webRequestServices.PostDataAsync( ShipStationCommand.CreateUpdateOrder, order.SerializeToJson(), token, _timeouts[ ShipStationOperationEnum.CreateUpdateOrder ] );
				}
				catch( WebException x )
				{
					if( x.Response.GetHttpStatusCode() == HttpStatusCode.InternalServerError )
						ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "Error updating order. Encountered 500 Internal Error. Order: {Order}",
							Constants.ChannelName,
							Constants.VersionInfo, 
							this._syncRunContext.TenantId,
							this._syncRunContext.ChannelAccountId,
							this._syncRunContext.CorrelationId,
							nameof(ShipStationService),
							nameof(this.UpdateOrderAsync),
							order );
					else if( x.Response.GetHttpStatusCode() == HttpStatusCode.NotFound )
						ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "Error updating order. Encountered 404 Not Found. Order: {Order}",
							Constants.ChannelName,
							Constants.VersionInfo, 
							this._syncRunContext.TenantId,
							this._syncRunContext.ChannelAccountId,
							this._syncRunContext.CorrelationId,
							nameof(ShipStationService),
							nameof(this.UpdateOrderAsync),
							order );
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
					ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Try send register request. Command: {Command}. Register: {Register}",
						Constants.ChannelName,
						Constants.VersionInfo, 
						this._syncRunContext.TenantId,
						this._syncRunContext.ChannelAccountId,
						this._syncRunContext.CorrelationId,
						nameof(ShipStationService),
						nameof(this.Register),
						ShipStationCommand.Register.Command, register );
					response = this._webRequestServices.PostDataAndGetResponseWithShipstationHeader< ShipStationRegisterResponse >( ShipStationCommand.Register, register.SerializeToJson(), token, true, _timeouts[ ShipStationOperationEnum.Register ] );
					ShipStationLogger.Log.Info( Constants.LoggingCommonPrefix + "Try send register request is success. Command: {Command}. Register: {Register}",
						Constants.ChannelName,
						Constants.VersionInfo, 
						this._syncRunContext.TenantId,
						this._syncRunContext.ChannelAccountId,
						this._syncRunContext.CorrelationId,
						nameof(ShipStationService),
						nameof(this.Register),
						ShipStationCommand.Register.Command, register );
				}
				catch( Exception ex )
				{
					ShipStationLogger.Log.Error( Constants.LoggingCommonPrefix + "Try send register request is fail. Command: {Command}. Register: {Register}",
						Constants.ChannelName,
						Constants.VersionInfo, 
						this._syncRunContext.TenantId,
						this._syncRunContext.ChannelAccountId,
						this._syncRunContext.CorrelationId,
						nameof(ShipStationService),
						nameof(this.Register),
						ShipStationCommand.Register.Command, register );
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
			var stores = await this.GetStoresAsync( token ).ConfigureAwait( false );

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