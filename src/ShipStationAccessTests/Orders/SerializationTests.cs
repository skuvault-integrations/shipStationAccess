using System;
using FluentAssertions;
using NUnit.Framework;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Services;

namespace ShipStationAccessTests.Orders
{
	public class SerializationTests
	{
		[ Test ]
		public void Date_Deserialization()
		{
			//------------ Arrange
			var pstDate = "2015-06-06T00:29:35.0000000";

			//------------ Act
			var serializedDate = pstDate.DeserializeJson< DateTime >();

			//------------ Assert
			serializedDate.Should().Be( new DateTime( 2015, 06, 06, 07, 29, 35, 00, DateTimeKind.Utc ) ); 
		}
		
		[ Test ]
		public void Date_Serialization()
		{
			//------------ Arrange
			var pstDate = "2015-06-06T00:29:35";

			//------------ Act
			var serializedDate = pstDate.DeserializeJson< DateTime >();

			//------------ Assert
			serializedDate.SerializeToJson().Trim( '"' ).Should().Be( pstDate ); 
		}

		[ Test ]
		public void Order_Deserialization()
		{
			//------------ Arrange
			var orderString = "{\"orderId\":148363009,\"orderNumber\":\"100208\",\"orderKey\":\"manual-31dc6fc5042b4c958ca1ad9b6a89f9d9\",\"orderDate\":\"2015-06-11T09:29:35.0970000\",\"modifyDate\":\"2015-06-13T17:18:25.6000000\",\"paymentDate\":\"2015-06-06T00:29:35.0000000\",\"orderStatus\":\"shipped\",\"customerUsername\":\"1@1\",\"customerEmail\":\"1@1\",\"billTo\":{\"name\":\"meme\",\"company\":null,\"street1\":null,\"street2\":null,\"street3\":null,\"city\":null,\"state\":null,\"postalCode\":null,\"country\":null,\"phone\":null,\"residential\":null,\"addressVerified\":null},\"shipTo\":{\"name\":\"meme\",\"company\":\"Bangbang\",\"street1\":\"1123 bang bang\",\"street2\":\"\",\"street3\":null,\"city\":\"Bang\",\"state\":\"AL\",\"postalCode\":\"44444\",\"country\":\"US\",\"phone\":\"123\",\"residential\":false,\"addressVerified\":\"Address validation failed\"},\"items\":[{\"orderItemId\":200077334,\"lineItemKey\":null,\"sku\":\"testSku1\",\"name\":\"TestLineItem\",\"imageUrl\":null,\"weight\":null,\"quantity\":1,\"unitPrice\":1.00,\"warehouseLocation\":\"AA68(20)\",\"options\":[],\"productId\":14841724,\"fulfillmentSku\":null,\"adjustment\":false}],\"orderTotal\":1.00,\"amountPaid\":0.00,\"taxAmount\":0.00,\"shippingAmount\":0.00,\"customerNotes\":null,\"internalNotes\":null,\"gift\":false,\"giftMessage\":null,\"paymentMethod\":null,\"requestedShippingService\":null,\"carrierCode\":null,\"serviceCode\":null,\"packageCode\":null,\"confirmation\":\"none\",\"shipDate\":\"2015-06-13\",\"holdUntilDate\":\"2015-06-30\",\"weight\":{\"value\":0.0,\"units\":\"ounces\"},\"dimensions\":null,\"insuranceOptions\":{\"provider\":null,\"insureShipment\":false,\"insuredValue\":0.0},\"internationalOptions\":{\"contents\":null,\"customsItems\":null,\"nonDelivery\":null},\"advancedOptions\":{\"warehouseId\":18911,\"nonMachinable\":false,\"saturdayDelivery\":false,\"containsAlcohol\":false,\"mergedOrSplit\":false,\"parentId\":null,\"storeId\":28400,\"customField1\":null,\"customField2\":null,\"customField3\":null,\"source\":null,\"billToParty\":null,\"billToAccount\":null,\"billToPostalCode\":null,\"billToCountryCode\":null},\"tagIds\":null,\"userId\":null}";
			var order = orderString.DeserializeJson< ShipStationOrder >();

			//------------ Act
			var serializedOrderString = order.SerializeToJson();
			var order2 = serializedOrderString.DeserializeJson< ShipStationOrder >();
			var serializedOrderString2 = order2.SerializeToJson();

			//------------ Assert
			order.PaymentDate.Should().Be( new DateTime( 2015, 06, 06, 07, 29, 35, 00, DateTimeKind.Utc ) ); 
			Assert.AreEqual( serializedOrderString, serializedOrderString2 );
		}
	}
}