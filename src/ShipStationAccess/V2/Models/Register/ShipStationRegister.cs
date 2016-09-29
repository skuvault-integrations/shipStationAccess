using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ShipStationAccess.V2.Models.OrderItem;

namespace ShipStationAccess.V2.Models.Register
{
	[ DataContract ]
	public sealed class ShipStationRegister
	{
		[ DataMember( Name = "apiCode" ) ]
		public string ApiCode{ get; set; }

		[ DataMember( Name = "authToken" ) ]
		public string AuthToken{ get; set; }

		[ DataMember( Name = "authKey" ) ]
		public string AuthKey{ get; set; }

		public ShipStationRegister( string apiCode, string authToken, string authKey )
		{
			this.ApiCode = apiCode;
			this.AuthToken = authToken;
			this.AuthKey = authKey;
		}

		public override string ToString()
		{
			return string.Format( @"ApiCode: {0}. AuthToken: {1}. AuthKey: {2}", this.ApiCode, this.AuthToken, this.AuthKey );
		}

		#region Equality members
		public bool Equals( ShipStationRegister other )
		{
			if( ReferenceEquals( null, other ) )
				return false;
			if( ReferenceEquals( this, other ) )
				return true;
			return this.ApiCode.Equals( other.ApiCode ) &&
			       this.AuthToken.Equals( other.AuthToken ) &&
			       this.AuthKey.Equals( other.AuthKey );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			if( obj.GetType() != this.GetType() )
				return false;
			return this.Equals( ( ShipStationRegister )obj );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = this.ApiCode.GetHashCode();
				result = ( result * 397 ) ^ this.AuthToken.GetHashCode();
				result = ( result * 397 ) ^ this.AuthKey.GetHashCode();

				return result;
			}
		}

		public static bool operator ==( ShipStationRegister left, ShipStationRegister right )
		{
			return Equals( left, right );
		}

		public static bool operator !=( ShipStationRegister left, ShipStationRegister right )
		{
			return !Equals( left, right );
		}
		#endregion
	}
}