using System;
using System.Collections.Generic;
using System.Linq;
using Netco.Extensions;

namespace ShipStationAccess.V2.Models.WarehouseLocation
{
	public sealed class ShipStationWarehouseLocations
	{
		public Dictionary< string, HashSet< long > > WarehouseLocations{ get; private set; }

		public ShipStationWarehouseLocations()
		{
			this.WarehouseLocations = new Dictionary< string, HashSet< long > >( StringComparer.InvariantCultureIgnoreCase );
		}

		public ShipStationWarehouseLocations( Dictionary< string, HashSet< long > > warehouseLocations )
		{
			this.WarehouseLocations = warehouseLocations;
		}

		public void AddItem( string warehouseLocation, long orderItemId )
		{
			HashSet< long > orderItemIds;
			if( !this.WarehouseLocations.TryGetValue( warehouseLocation, out orderItemIds ) )
			{
				this.WarehouseLocations[ warehouseLocation ] = new HashSet< long > { orderItemId };
				return;
			}

			orderItemIds.Add( orderItemId );
		}

		public void AddItems( string warehouseLocation, IEnumerable< long > orderItemIds )
		{
			HashSet< long > oldOrderItemIds;
			if( !this.WarehouseLocations.TryGetValue( warehouseLocation, out oldOrderItemIds ) )
			{
				this.WarehouseLocations[ warehouseLocation ] = orderItemIds.ToHashSet();
				return;
			}

			foreach( var orderItemId in orderItemIds )
			{
				oldOrderItemIds.Add( orderItemId );
			}
		}

		public List< ShipStationWarehouseLocation > GetWarehouseLocationsToSend()
		{
			var result = new List< ShipStationWarehouseLocation >();
			foreach( var warehouseLocation in this.WarehouseLocations )
			{
				for( var i = 0; i < 999999; i++ )
				{
					var pagedItems = warehouseLocation.Value.GetPage( i, 100 ).ToList();
					result.Add( new ShipStationWarehouseLocation { WarehouseLocation = warehouseLocation.Key, OrderItemIds = pagedItems } );
					if( pagedItems.Count < 100 )
						break;
				}
			}
			return result;
		}
	}
}