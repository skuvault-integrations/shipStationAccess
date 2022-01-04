using System;

namespace ShipStationAccess.V2.Misc
{
	static class PageSizeAdjuster
	{
		public static int GetHalfPageSize( int currentPageSize )
		{
			return (int)Math.Floor( currentPageSize / 2.0 );
		}

		public static int DoublePageSize( int currentPageSize, int maxPageSize )
		{
			return Math.Min( currentPageSize * 2, maxPageSize );
		}

		public static int GetNextPageIndex( int totalEntitiesReceived, int pageSize )
		{
			return (int)Math.Floor( totalEntitiesReceived * 1.0 / pageSize ) + 1;
		}

		public static bool AreEntitiesWillBeDownloadedAgainAfterChangingThePageSize( int newPage, int newPageSize, int receivedEntities )
		{
			var firstEntityIndexToDownload = ( newPage - 1 ) * newPageSize;
			return receivedEntities > firstEntityIndexToDownload;
		}
	}
}