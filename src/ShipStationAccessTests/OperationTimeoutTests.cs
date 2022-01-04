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
			var operationsTimeouts = new ShipStationTimeouts();

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( operationsTimeouts.DefaultOperationTimeout.TimeoutInMs );
		}

		[ Test ]
		public void GivenOwnDefaultTimeoutValue_WhenGetTimeoutIsCalled_ThenOverridenDefaultTimeoutIsReturned()
		{
			var newDefaultTimeoutInMs = 10 * 60 * 1000;
			var operationsTimeouts = new ShipStationTimeouts( newDefaultTimeoutInMs );

			operationsTimeouts[ ShipStationOperationEnum.CreateUpdateOrder ].Should().Be( newDefaultTimeoutInMs );
		}

		[ Test ]
		public void GivenListOrdersTimeoutIsSet_WhenGetTimeoutIsCalled_ThenSpecificTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationTimeouts();
			var specificTimeoutInMs = 10 * 60 * 1000;
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs ) );

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( specificTimeoutInMs );
			operationsTimeouts[ ShipStationOperationEnum.CreateUpdateOrder ].Should().Be( operationsTimeouts.DefaultOperationTimeout.TimeoutInMs );
		}

		[ Test ]
		public void GivenListOrdersTimeoutIsSetTwice_WhenGetTimeoutIsCalled_ThenSpecificTimeoutIsReturned()
		{
			var operationsTimeouts = new ShipStationTimeouts();
			var specificTimeoutInMs = 10 * 60 * 1000;
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs ) );
			operationsTimeouts.Set( ShipStationOperationEnum.ListOrders, new ShipStationOperationTimeout( specificTimeoutInMs * 2 ) );

			operationsTimeouts[ ShipStationOperationEnum.ListOrders ].Should().Be( specificTimeoutInMs * 2 );
		}
	}
}