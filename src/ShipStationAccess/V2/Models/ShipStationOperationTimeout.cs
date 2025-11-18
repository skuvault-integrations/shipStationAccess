using System;
using System.Collections.Generic;

namespace ShipStationAccess.V2.Models
{
	public enum ShipStationOperationEnum
	{
		ListOrders,
		GetOrder,
		GetOrderShipments,
		GetOrderFulfillments,
		CreateUpdateOrder,
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
			if( timeoutInMs <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( timeoutInMs ), "Value must be greater than 0." );
			}
			this.TimeoutInMs = timeoutInMs;
		}
	}

	public class ShipStationTimeouts
	{
		public const int DefaultTimeoutInMs = 5 * 60 * 1000;
		private Dictionary< ShipStationOperationEnum, ShipStationOperationTimeout > _timeouts;

		/// <summary>
		///	This timeout value will be used if specific timeout for operation is not provided. Default value can be changed through constructor.
		/// </summary>
		public ShipStationOperationTimeout DefaultOperationTimeout { get; private set; }

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

		public ShipStationTimeouts( int defaultTimeoutInMs )
		{
			_timeouts = new Dictionary< ShipStationOperationEnum, ShipStationOperationTimeout >();
			this.DefaultOperationTimeout = new ShipStationOperationTimeout( defaultTimeoutInMs );
		}

		public ShipStationTimeouts() : this( DefaultTimeoutInMs ) { }
	}
}