using System;
using System.Threading;
using NUnit.Framework;
using ShipStationAccess.V2.Models.Register;

namespace ShipStationAccessTests.Register
{
	[ Explicit ]
	public class RegisterTests: BaseTest
	{
		[ Test ]
		public void Register()
		{
			Assert.That( () =>
					this._shipStationService.Register( new ShipStationRegister( "skuvault", this._credentials.ApiSecret, this._credentials.ApiKey ), CancellationToken.None ),
				Throws.Nothing
			);
		}
	}
}