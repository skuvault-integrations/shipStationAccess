using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ShipStationAccessTests
{
	public class ThrottlingTests: BaseTest
	{

		[ Test ]
		public async Task ThrottlingTest()
		{
			var endDate = DateTime.UtcNow; //new DateTime( 2015, 06, 01, 22, 45, 00, DateTimeKind.Utc );
			var startDate = endDate.AddDays( -5 );

			var orders = this._shipStationService.GetOrders( startDate, endDate, CancellationToken.None );

			var tasks = new List< Task >();

			foreach( var i in Enumerable.Range( 0, 20 ) )
			{
				tasks.Add( this._shipStationService.GetOrdersAsync( startDate, endDate, CancellationToken.None ) );
			}

			await Task.WhenAll( tasks );

			orders.Count().Should().BeGreaterThan( 0 );
		}
	}
}