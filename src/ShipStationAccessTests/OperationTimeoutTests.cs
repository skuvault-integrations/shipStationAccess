using FluentAssertions;
using NUnit.Framework;
using ShipStationAccess.V2.Models;

namespace ShipStationAccessTests
{
	[ TestFixture ]
	public class OperationTimeoutTests
	{
		[ Test ]
		public void GivenSpecificTimeoutsAreNotSet_WhenGetTimeoutIsCalled_ThenDefaultTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationOperationsTimeouts();

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( operationsTimeouts.DefaultOperationTimeout.TimeoutInMs );
		}

		[ Test ]
		public void GivenOwnDefaultTimeoutValue_WhenGetTimeoutIsCalled_ThenOverridenDefaultTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationOperationsTimeouts();
			var newDefaultTimeoutInMs = 10 * 60 * 1000;
			operationsTimeouts.DefaultOperationTimeout = new ShipStationOperationTimeout( newDefaultTimeoutInMs );

			operationsTimeouts[ ShipStationOperationEnum.CreateOrder ].Should().Be( newDefaultTimeoutInMs );
		}

		[ Test ]
		public void GivenListOrdersTimeoutIsSet_WhenGetTimeoutIsCalled_ThenSpecificTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationOperationsTimeouts();
			var specificTimeoutInMs = 10 * 60 * 1000;
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs ) );

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( specificTimeoutInMs );
			operationsTimeouts[ ShipStationOperationEnum.CreateOrder ].Should().Be( operationsTimeouts.DefaultOperationTimeout.TimeoutInMs );
		}

		[ Test ]
		public void GivenListOrdersTimeoutIsSetTwice_WhenGetTimeoutIsCalled_ThenSpecificTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationOperationsTimeouts();
			var specificTimeoutInMs = 10 * 60 * 1000;
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs ) );
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs * 2 ) );

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( specificTimeoutInMs * 2 );
		}
	}
}