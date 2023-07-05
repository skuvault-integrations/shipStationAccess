using System;
using System.Threading;
using NUnit.Framework;
using ShipStationAccess.V2.Models.Register;

namespace ShipStationAccessTests.Register
{
	public class RegisterTests: BaseTest
	{
		[ Test ]
		public void Register()
		{
			try
			{
				this._shipStationService.Register( new ShipStationRegister( "skuvault", this._credentials.ApiSecret, this._credentials.ApiKey ), CancellationToken.None );
			}
			catch( Exception ex )
			{
				Assert.Fail( ex.Message );
			}
		}
	}
}