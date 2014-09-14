using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.Order
{
	[ DataContract ]
	public sealed class ShipStationOrderInsuranceOptions
	{
		[ DataMember( Name = "provider" ) ]
		public string Provider{ get; set; }

		[ DataMember( Name = "insureShipment" ) ]
		public bool InsureShipment{ get; set; }

		[ DataMember( Name = "insuredValue" ) ]
		public double InsuredValue
		{
			get { return this._insuredValue < 0.01 ? 0.01 : this._insuredValue; }

			set { this._insuredValue = value; }
		}
		private double _insuredValue;
	}
}