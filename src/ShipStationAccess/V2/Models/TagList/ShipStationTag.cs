using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShipStationAccess.V2.Models.TagList
{

	[ DataContract ]
	public sealed class ShipStationTag
	{
		[ DataMember( Name = "tagId") ]
		public long TagId{ get; set; }

		[ DataMember( Name = "name" ) ]
		public string Name { get; set; }

		[ DataMember( Name = "color" ) ]
		public string Color { get; set; }
	}
}
