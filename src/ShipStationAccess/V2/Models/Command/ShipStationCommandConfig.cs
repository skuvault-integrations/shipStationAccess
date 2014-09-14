using CuttingEdge.Conditions;

namespace ShipStationAccess.V2.Models.Command
{
	internal sealed class ShipStationCommandConfig
	{
		public int Page{ get; private set; }
		public int PageSize{ get; private set; }

		public ShipStationCommandConfig( int page, int pageSize )

		{
			Condition.Requires( page, "page" ).IsGreaterThan( 0 );
			Condition.Requires( pageSize, "pageSize" ).IsGreaterThan( 0 );

			this.Page = page;
			this.PageSize = pageSize;
		}

		public ShipStationCommandConfig( int pageSize )
		{
			Condition.Requires( pageSize, "pageSize" ).IsGreaterThan( 0 );

			this.PageSize = pageSize;
		}
	}
}