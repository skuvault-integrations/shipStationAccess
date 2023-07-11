using FluentAssertions;
using NUnit.Framework;
using ShipStationAccess.V2.Exceptions;
using ShipStationAccess.V2.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using ShipStationAccess.V2;

namespace ShipStationAccessTests
{
	[ TestFixture ]
	public class ServerErrorsTests : BaseTest
	{
		private IShipStationService _shipStationServiceWithInvalidCredentials;

		[ SetUp ]
		public new void Init()
		{
			var invalidCredentials = new ShipStationCredentials( this._credentials.ApiKey + Guid.NewGuid(), this._credentials.ApiSecret, this._credentials.PartnerKey );
			this._shipStationServiceWithInvalidCredentials = this._shipStationFactory.CreateServiceV2(invalidCredentials, this.SyncRunContext);
		}

		[ Test ]
		public void GivenInvalidCredentials_WhenGetOrdersAsyncIsCalled_ThenAuthorizationExceptionIsExpected()
		{
			var ex = Assert.Throws< AggregateException >( () =>
			{
				this._shipStationServiceWithInvalidCredentials.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None ).Wait();
			} );

			ex.Should().NotBeNull();
			ex.InnerException.Should().NotBeNull();
			ex.InnerException.GetType().Should().Be( typeof( ShipStationUnauthorizedException ) );
		}

		[ Test ]
		public async Task GivenInvalidCredentials_WhenUpdateOrdersAsyncIsCalled_ThenAuthorizationExceptionIsExpected()
		{
			var existingOrderId = "592317819";
			var order = await this._shipStationService.GetOrderByIdAsync( existingOrderId, CancellationToken.None );
			order.InternalNotes = "Some random note placed here #" + Guid.NewGuid().ToString();

			var ex = Assert.Throws< AggregateException >( () =>
			{
				this._shipStationServiceWithInvalidCredentials.UpdateOrderAsync( order, CancellationToken.None ).Wait();
			} );

			ex.Should().NotBeNull();
			ex.InnerException.Should().NotBeNull();
			ex.InnerException.GetType().Should().Be( typeof( ShipStationUnauthorizedException ) );
		}

		[ Test ]
		public void GivenTooSmallTimeout_WhenGetOrdersIsCalled_ThenExceptionIsReturned()
		{
			var timeouts = new ShipStationTimeouts();
			var tinyTimeout = new ShipStationOperationTimeout( 10 );
			timeouts.Set( ShipStationOperationEnum.ListOrders, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderShipments, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderFulfillments, tinyTimeout );

			var tinyTimeoutsService = this._shipStationFactory.CreateServiceV2( this._credentials, this.SyncRunContext, timeouts );
			var ex = Assert.Throws< TaskCanceledException >( () =>
			{
				tinyTimeoutsService.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
			} );
			
			ex.Should().NotBeNull();
		}

		[ Test ]
		public void GivenTooSmallTimeout_WhenGetOrdersAsyncIsCalled_ThenExceptionIsReturned()
		{
			var timeouts = new ShipStationTimeouts();
			var tinyTimeout = new ShipStationOperationTimeout( 10 );
			timeouts.Set( ShipStationOperationEnum.ListOrders, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderShipments, tinyTimeout );
			timeouts.Set( ShipStationOperationEnum.GetOrderFulfillments, tinyTimeout );
			var tinyTimeoutsService = this._shipStationFactory.CreateServiceV2( this._credentials, this.SyncRunContext, timeouts );

			var ex = Assert.ThrowsAsync< TaskCanceledException >( async () =>
			{
				await tinyTimeoutsService.GetOrdersAsync( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
			} );

			ex.Should().NotBeNull();
		}
	}
}