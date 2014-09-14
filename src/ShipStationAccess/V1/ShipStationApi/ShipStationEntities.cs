using System;
using System.Data.Services.Common;

namespace ShipStationAccess.V1.ShipStationApi
{
	public partial class ShipStationEntities
	{
		public ShipStationEntities( Uri serviceRoot, DataServiceProtocolVersion version ) : base( serviceRoot, version )
		{
			this.ResolveName = this.ResolveNameFromType;
			this.ResolveType = this.ResolveTypeFromName;
			this.OnContextCreated();
		}
	}
}