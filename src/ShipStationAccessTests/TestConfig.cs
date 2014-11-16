using LINQtoCSV;

namespace ShipStationAccessTests
{
	internal class TestConfig
	{

		[ CsvColumn( Name = "ApiSecret", FieldIndex = 1 ) ]
		public string ApiSecret{ get; set; }

		[ CsvColumn( Name = "ApiKey", FieldIndex = 2 ) ]
		public string ApiKey{ get; set; }

		[ CsvColumn( Name = "PartnerKey", FieldIndex = 3 ) ]
		public string PartnerKey{ get; set; }
	}
}