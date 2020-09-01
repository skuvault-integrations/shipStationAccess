using System.Collections.Generic;

namespace ShipStationAccess.V2.Services
{
	public sealed class PaginatedResponse< T > where T : class, new()
	{
		public int? TotalPagesExpected { get; set; }
		public int? TotalEntitiesExpected { get; set; }
		public int? TotalPagesReceived { get; set; }

		public IEnumerable< T > Data { get; private set; }

		public PaginatedResponse( IEnumerable< T > data )
		{
			this.Data = data ?? new List< T >();
		}
	}
}