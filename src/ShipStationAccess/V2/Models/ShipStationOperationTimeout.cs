using CuttingEdge.Conditions;
using System.Collections.Generic;

namespace ShipStationAccess.V2.Models
{
	public enum ShipStationOperationEnum
	{
		ListOrders,
		GetOrder,
		GetOrderShipments,
		GetOrderFulfillments,
		CreateOrder,
		UpdateWarehouseLocation,
		GetStores,
		GetTags,
		GetShippingLabel,
		Register
	}

	public class ShipStationOperationTimeout
	{
		public int TimeoutInMs { get; private set; }

		public ShipStationOperationTimeout( int timeoutInMs )
		{
			Condition.Requires( timeoutInMs, "timeoutInMs" ).IsGreaterThan( 0 );
			this.TimeoutInMs = timeoutInMs;
		}
	}

	public class ShipStationTimeouts
	{
		public const int DefaultTimeoutInMs = 5 * 60 * 1000;
		private Dictionary< ShipStationOperationEnum, ShipStationOperationTimeout > _timeouts;

		public ShipStationOperationTimeout DefaultOperationTimeout { get; set; }

		public int this[ ShipStationOperationEnum operation ]
		{
			get
			{
				if ( _timeouts.TryGetValue( operation, out ShipStationOperationTimeout timeout ) )
					return timeout.TimeoutInMs;

				return DefaultOperationTimeout.TimeoutInMs;
			}
		}

		public void Set( ShipStationOperationEnum operation, ShipStationOperationTimeout timeout )
		{
			if ( _timeouts.ContainsKey( operation ) )
			{
				_timeouts.Remove( operation );
			}

			_timeouts.Add( operation, timeout );
		}

		public ShipStationTimeouts()
		{
			_timeouts = new Dictionary< ShipStationOperationEnum, ShipStationOperationTimeout >();
			this.DefaultOperationTimeout = new ShipStationOperationTimeout( DefaultTimeoutInMs );
		}
	}
}