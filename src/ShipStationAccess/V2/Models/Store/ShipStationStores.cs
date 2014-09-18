using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Store
{
	[ DataContract ]
	public class ShipStationStores
	{
		public IList< ShipStationStore > Stores{ get; set; }
	}
}