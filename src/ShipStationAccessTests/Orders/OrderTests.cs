using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LINQtoCSV;
using Netco.Extensions;
using Netco.Logging;
using Netco.Logging.SerilogIntegration;
using NUnit.Framework;
using Serilog;
using ShipStationAccess;
using ShipStationAccess.V2.Models;
using ShipStationAccess.V2.Models.Order;
using ShipStationAccess.V2.Services;

namespace ShipStationAccessTests.Orders
{
	public class OrderTests
	{
		private readonly IShipStationFactory ShipStationFactory = new ShipStationFactory();
		private ShipStationCredentials _credentials;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ShipStationCredentials.csv";
			Log.Logger = new LoggerConfiguration()
				.Destructure.ToMaximumDepth( 100 )
				.MinimumLevel.Verbose()
				.WriteTo.Console().CreateLogger();
			NetcoLogger.LoggerFactory = new SerilogLoggerFactory( Log.Logger );

			var cc = new CsvContext();
			var testConfig = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( testConfig != null )
				this._credentials = new ShipStationCredentials( testConfig.ApiKey, testConfig.ApiSecret );
		}

		[ Test ]
		public void DeserializationTest()
		{
			var json = "{\"orders\":[],\"total\":2,\"page\":1,\"pages\":3}";
			var orders = json.DeserializeJson< ShipStationOrders >();
			orders.TotalPages.Should().Be( 3 );
			orders.CurrentPageNumber.Should().Be( 1 );
			orders.TotalOrders.Should().Be( 2 );
		}

		[Test]
		public void DeserializeOrderWithNullablePaymentDateTest()
		{
			var json = "{\"orders\":["
			           +
			           "{\"orderId\":156695469,\"orderNumber\":\"100267\",\"orderKey\":\"manual-916d07f350db46f4aeb8608ffec2b265\",\"orderDate\":\"2015-07-28T10:58:54.5730000\",\"createDate\":\"2015-07-28T10:58:54.5730000\",\"modifyDate\":\"2015-08-26T07:23:39.6100000\",\"paymentDate\":null,\"orderStatus\":\"shipped\",\"customerUsername\":\"1@1\",\"customerEmail\":\"1@1\",\"billTo\":{\"name\":\"meme\",\"company\":null,\"street1\":null,\"street2\":null,\"street3\":null,\"city\":null,\"state\":null,\"postalCode\":null,\"country\":null,\"phone\":null,\"residential\":null,\"addressVerified\":null},\"shipTo\":{\"name\":\"meme\",\"company\":\"\",\"street1\":\"11751 S DIXIE HWY\",\"street2\":\"\",\"street3\":null,\"city\":\"SONORA\",\"state\":\"KY\",\"postalCode\":\"42776-9739\",\"country\":\"US\",\"phone\":\"12707662520\",\"residential\":true,\"addressVerified\":\"Address validated successfully\"},\"items\":[{\"orderItemId\":212535867,\"lineItemKey\":null,\"sku\":\"testSku1\",\"name\":\"test\",\"imageUrl\":null,\"weight\":null,\"quantity\":1,\"unitPrice\":2.00,\"taxAmount\":null,\"shippingAmount\":null,\"warehouseLocation\":\"A1 (100), A1 (12)\",\"options\":[],\"productId\":14841724,\"fulfillmentSku\":null,\"adjustment\":false,\"upc\":null,\"createDate\":\"2015-07-28T10:58:54.573\",\"modifyDate\":\"2015-07-28T10:58:54.573\"}],\"orderTotal\":2.00,\"amountPaid\":0.00,\"taxAmount\":0.00,\"shippingAmount\":0.00,\"customerNotes\":null,\"internalNotes\":null,\"gift\":false,\"giftMessage\":null,\"paymentMethod\":null,\"requestedShippingService\":null,\"carrierCode\":null,\"serviceCode\":null,\"packageCode\":null,\"confirmation\":\"none\",\"shipDate\":\"2015-08-26\",\"holdUntilDate\":null,\"weight\":{\"value\":0.00,\"units\":\"ounces\"},\"dimensions\":null,\"insuranceOptions\":{\"provider\":null,\"insureShipment\":false,\"insuredValue\":0.0},\"internationalOptions\":{\"contents\":null,\"customsItems\":null,\"nonDelivery\":null},\"advancedOptions\":{\"warehouseId\":18911,\"nonMachinable\":false,\"saturdayDelivery\":false,\"containsAlcohol\":false,\"mergedOrSplit\":false,\"parentId\":null,\"storeId\":28400,\"customField1\":null,\"customField2\":null,\"customField3\":null,\"source\":null,\"billToParty\":null,\"billToAccount\":null,\"billToPostalCode\":null,\"billToCountryCode\":null},\"tagIds\":null,\"userId\":null},"
			           + "],\"total\":146,\"page\":1,\"pages\":2}";
			var orders = json.DeserializeJson<ShipStationOrders>();
			orders.Orders.Count.Should().Be(1);
			orders.Orders[0].PaymentDate.Should().Be(null);
		}

		[Test]
		public void DeserializeOrderWithManyCustomsItemsTest()
		{
			var json = "{\"orders\":["
			           +
			           "{\"orderId\":156799376,\"orderNumber\":\"104778\",\"orderKey\":\"104778\",\"orderDate\":\"2015-07-28T23:12:50.2500000\",\"createDate\":\"2015-07-28T23:13:55.6130000\",\"modifyDate\":\"2015-07-28T23:50:23.7370000\",\"paymentDate\":\"2015-08-26T07:23:39.6100000\",\"orderStatus\":\"cancelled\",\"customerUsername\":null,\"customerEmail\":null,\"billTo\":{\"name\":\"A B\",\"company\":\"\",\"street1\":\"line1\",\"street2\":\"\",\"street3\":null,\"city\":\"Silent Hill\",\"state\":\"\",\"postalCode\":\"444455511\",\"country\":\"NL\",\"phone\":\"\",\"residential\":null,\"addressVerified\":null},\"shipTo\":{\"name\":\"A B\",\"company\":\"\",\"street1\":\"line1\",\"street2\":\"\",\"street3\":null,\"city\":\"Silent Hill\",\"state\":\"\",\"postalCode\":\"444455511\",\"country\":\"NL\",\"phone\":\"\",\"residential\":false,\"addressVerified\":\"Address not yet validated\"},\"items\":[{\"orderItemId\":212701021,\"lineItemKey\":\"32077214\",\"sku\":\"testSku1\",\"name\":\"Test Product Sync\",\"imageUrl\":null,\"weight\":{\"value\":384.00,\"units\":\"ounces\"},\"quantity\":1,\"unitPrice\":1.00,\"taxAmount\":null,\"shippingAmount\":null,\"warehouseLocation\":\"A1 (100), A1 (12)\",\"options\":[],\"productId\":14841724,\"fulfillmentSku\":null,\"adjustment\":false,\"upc\":null,\"createDate\":\"2015-07-28T23:13:55.613\",\"modifyDate\":\"2015-07-28T23:13:55.613\"}],\"orderTotal\":1.00,\"amountPaid\":0.00,\"taxAmount\":0.00,\"shippingAmount\":0.00,\"customerNotes\":null,\"internalNotes\":null,\"gift\":false,\"giftMessage\":null,\"paymentMethod\":null,\"requestedShippingService\":null,\"carrierCode\":null,\"serviceCode\":null,\"packageCode\":null,\"confirmation\":\"none\",\"shipDate\":\"2015-07-28\",\"holdUntilDate\":null,\"weight\":{\"value\":384.00,\"units\":\"ounces\"},\"dimensions\":null,\"insuranceOptions\":{\"provider\":null,\"insureShipment\":false,\"insuredValue\":0.0},\"internationalOptions\":{\"contents\":\"merchandise\",\"customsItems\":[{\"customsItemId\":15187335,\"description\":\"Test Product Sync\",\"quantity\":1,\"value\":1.00,\"harmonizedTariffCode\":null,\"countryOfOrigin\":\"US\"}],\"nonDelivery\":\"return_to_sender\"},\"advancedOptions\":{\"warehouseId\":18911,\"nonMachinable\":false,\"saturdayDelivery\":false,\"containsAlcohol\":false,\"mergedOrSplit\":false,\"parentId\":null,\"storeId\":28400,\"customField1\":null,\"customField2\":null,\"customField3\":null,\"source\":null,\"billToParty\":null,\"billToAccount\":null,\"billToPostalCode\":null,\"billToCountryCode\":null},\"tagIds\":null,\"userId\":null},"
			           + "],\"total\":146,\"page\":1,\"pages\":2}";
			var orders = json.DeserializeJson<ShipStationOrders>();
			orders.Orders.Count.Should().Be(1);
			orders.Orders[0].InternationalOptions.CustomsItems.Count.Should().BeGreaterThan(0);
		}

		[ Test ]
		public void GetOrders()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void SerializationOrderTest()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -7 ), DateTime.UtcNow );
			var testOrder = orders.First();

			var serializedOrder = testOrder.SerializeToJson();
			var deserializedOrder = serializedOrder.DeserializeJson< ShipStationOrder >();
			var serializedOrder2 = deserializedOrder.SerializeToJson();
			Assert.AreEqual( serializedOrder, serializedOrder2 );
		}

		[ Test ]
		public async Task GetOrdersAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -3 ), DateTime.UtcNow );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[Test]
		public async Task GetTagsAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var tags = await service.GetTagsAsync();

			tags.Count().Should().BeGreaterThan( 0 );
		}

		[Test]
		public void GetTags()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var tags = service.GetTags();

			tags.Count().Should().BeGreaterThan( 0 );
		}

		public async Task TrottlingTest()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var endDate = DateTime.UtcNow; //new DateTime( 2015, 06, 01, 22, 45, 00, DateTimeKind.Utc );

			var orders = service.GetOrders( endDate.AddDays( -1 ), endDate );

			var tasks = new List< Task >();

			foreach( var i in Enumerable.Range( 0, 500 ) )
			{
				tasks.Add( service.GetOrdersAsync( endDate.AddDays( -1 ), endDate ) );
			}

			await Task.WhenAll( tasks );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void UpdateOrder()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -10 ), DateTime.UtcNow );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
			{
				Assert.Fail( "No order found to update" );
				return;
			}

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			service.UpdateOrder( orderToChange );
		}

		[ Test ]
		public async Task UpdateOrderAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -90 ), DateTime.UtcNow );
			var orderToChange = orders.Select( o => o ).FirstOrDefault( or => or.IsValid() && or.OrderStatus == ShipStationOrderStatusEnum.awaiting_shipment || or.OrderStatus == ShipStationOrderStatusEnum.awaiting_payment );

			if( orderToChange == null )
				return;

			orderToChange.Items[ 0 ].WarehouseLocation = "AA22(30)";
			await service.UpdateOrderAsync( orderToChange );
		}

		[ Test ]
		public void UpdateOrderOnGetOrders()
		{
			var rand = new Random();
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			Func< ShipStationOrder, ShipStationOrder > updateOrderLocation = o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				service.UpdateOrder( o );
				return o;
			};
			var orders = service.GetOrders( DateTime.UtcNow.AddDays( -2 ), DateTime.UtcNow, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task UpdateOrderOnGetOrdersAsync()
		{
			var rand = new Random();
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			Func< ShipStationOrder, Task< ShipStationOrder > > updateOrderLocation = async o =>
			{
				if( o.Items.Count == 0 )
					return o;

				o.Items[ 0 ].WarehouseLocation = "AA{0}({1})".FormatWith( rand.Next( 1, 99 ), rand.Next( 1, 50 ) );
				await service.UpdateOrderAsync( o );
				return o;
			};
			var orders = await service.GetOrdersAsync( DateTime.UtcNow.AddDays( -7 ), DateTime.UtcNow, updateOrderLocation );

			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetStores()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = service.GetStores();

			stores.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetStoresAsync()
		{
			var service = this.ShipStationFactory.CreateServiceV2( this._credentials );
			var stores = await service.GetStoresAsync();

			stores.Count().Should().BeGreaterThan( 0 );
		}
	}
}