using FluentAssertions;
using NUnit.Framework;
using ShipStationAccess.V2.Exceptions;
using ShipStationAccess.V2.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShipStationAccessTests
{
	[ TestFixture ]
	public class ServerErrorsTests : BaseTest
	{
		private ShipStationCredentials _credentials;

		[ SetUp ]
		public void Init()
		{
			this._credentials = base.ReadCredentials();
		}

		[ Test ]
		public void GivenInvalidCredentials_WhenGetOrdersAsyncIsCalled_ThenAuthorizationExceptionIsExpected()
		{
			var invalidCredentials = new ShipStationCredentials( this._credentials.ApiKey + Guid.NewGuid().ToString(), this._credentials.ApiSecret, this._credentials.PartnerKey );
			var service = this.ShipStationFactory.CreateServiceV2( invalidCredentials );

			var ex = Assert.Throws< AggregateException >( () =>
			{
				service.GetOrdersAsync( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow, CancellationToken.None ).Wait();
			} );

			ex.Should().NotBeNull();
			ex.InnerException.Should().NotBeNull();
			ex.InnerException.GetType().Should().Be( typeof( ShipStationUnauthorizedException ) );
		}

		[ Test ]
		public async Task GivenInvalidCredentials_WhenUpdateOrdersAsyncIsCalled_ThenAuthorizationExceptionIsExpected()
		{
			var existingOrderId = "592317819";
			var workingService = base.ShipStationFactory.CreateServiceV2( this._credentials );
			var order = await workingService.GetOrderByIdAsync( existingOrderId, CancellationToken.None );
			order.InternalNotes = "Some random note placed here #" + Guid.NewGuid().ToString();

			var invalidCredentials = new ShipStationCredentials( this._credentials.ApiKey + Guid.NewGuid().ToString(), this._credentials.ApiSecret, this._credentials.PartnerKey );
			var service = this.ShipStationFactory.CreateServiceV2( invalidCredentials );

			var ex = Assert.Throws< AggregateException >( () =>
			{
				service.UpdateOrderAsync( order, CancellationToken.None ).Wait();
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

			var service = this.ShipStationFactory.CreateServiceV2( this._credentials, timeouts );

			var ex = Assert.Throws< TaskCanceledException >( () => {
				var orders = service.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
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

			var service = this.ShipStationFactory.CreateServiceV2( this._credentials, timeouts );

			var ex = Assert.Throws< TaskCanceledException >( async () => {
				var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow, CancellationToken.None );
			} );
			
			ex.Should().NotBeNull();
		}
	}
}