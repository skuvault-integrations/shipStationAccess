using System.Collections.Generic;

namespace ShipStationAccess.V2.Services
{
	public sealed class PageResponse< T > where T : class, new()
	{
		public int? TotalPages { get; set; }
		public int? TotalEntities { get; set; }
		
		public bool HasInternalError { get; set; }

		public List< T > Data { get; private set; }
		
		public PageResponse()
		{
			this.Data = new List< T >();
			this.HasInternalError = false;
		}
	}

	public sealed class SummaryResponse< T > where T : class, new()
	{
		public int? TotalEntitiesExpected { get; set; }
		public int TotalEntitiesHandled { get; set; }

		public List< ReadError > ReadErrors { get; private set; }
		public List< T > Data { get; private set; }

		public SummaryResponse()
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