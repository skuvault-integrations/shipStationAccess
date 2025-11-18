using System;

namespace ShipStationAccess.V2.Models.Command
{
	internal sealed class ShipStationCommandConfig
	{
		public int Page{ get; private set; }
		public int PageSize{ get; private set; }

		public ShipStationCommandConfig( int page, int pageSize )

		{
			if( page <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( page ), "Value must be greater than 0." );
			}

			if( pageSize <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( pageSize ), "Value must be greater than 0." );
			}

			this.Page = page;
			this.PageSize = pageSize;
		}

		public ShipStationCommandConfig( int pageSize )
		{
			if( pageSize <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( pageSize ), "Value must be greater than 0." );
			}

			this.PageSize = pageSize;
		}
	}
}