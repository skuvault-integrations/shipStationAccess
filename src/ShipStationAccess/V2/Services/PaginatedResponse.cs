using System.Collections.Generic;

namespace ShipStationAccess.V2.Services
{
	public sealed class PaginatedResponse< T > where T : class, new()
	{
		public int? TotalPagesExpected { get; set; }
		public int? TotalEntitiesExpected { get; set; }
		public int TotalPagesReceived { get; set; }

		public List< T > Data { get; private set; }
		public List< ReadError > ReadErrors { get; private set; }

		public PaginatedResponse()
		{
			this.Data = new List< T >();
			this.ReadErrors = new List< ReadError >();
		}
	}

	public sealed class ReadError
	{
		public string Url { get; set; }
		public int PageSize { get; set; }
		public int Page { get; set; }
	}
}