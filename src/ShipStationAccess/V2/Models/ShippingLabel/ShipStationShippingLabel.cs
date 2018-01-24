﻿using System;
using System.Runtime.Serialization;

namespace ShipStationAccess.V2.Models.ShippingLabel
{
	public sealed class WeightModel
	{
		[ DataMember( Name = "value" ) ]
		public string Value{ get; set; }
		[ DataMember( Name = "units" ) ]
		public string Units{ get; set; }

		public WeightModel( string value, string units )
		{
			this.Value = value;
			this.Units = units;
		}
	}

	[ DataContract ]
	public sealed class ShipStationShippingLabel
	{
		[ DataMember( Name = "shipmentId" ) ]
		public long ShipmentId{ get; set; }

		[ DataMember( Name = "shipmentCost" ) ]
		public decimal ShipmentCost{ get; set; }

		[ DataMember( Name = "insuranceCost" ) ]
		public decimal InsuranceCost{ get; set; }

		[ DataMember( Name = "trackingNumber" ) ]
		public string TrackingNumber{ get; set; }

		[ DataMember( Name = "labelData" ) ]
		public string LabelData{ get; set; }

		[ DataMember( Name = "formData" ) ]
		public string FormData{ get; set; }

		public static ShipStationShippingLabel GetMockShippingLabel()
		{
			return new ShipStationShippingLabel()
			{
				ShipmentId = 72513480,
				ShipmentCost = 7.3m,
				InsuranceCost = 0,
				TrackingNumber = "248201115029520",
				FormData = null,
				LabelData = "JVBERi0xLjQKJeLjz9MKMiAwIG9iago8PC9MZW5ndGggNjIvRmlsdGVyL0ZsYXRlRGVjb2RlPj5zdHJlYW0KeJwr5HIK4TI2UzC2NFMISeFyDeEK5CpUMFQwAEJDBV0jCz0LBV1jY0M9I4XkXAX9iDRDBZd8hUAuAEdGC7cKZW5kc3RyZWFtCmVuZG9iago0IDAgb2JqCjw8L1R5cGUvUGFnZS9NZWRpYUJveFswIDAgMjg4IDQzMl0vUmVzb3VyY2VzPDwvUHJvY1NldCBbL1BERiAvVGV4dCAvSW1hZ2VCIC9JbWFnZUMgL0ltYWdlSV0vWE9iamVjdDw8L1hmMSAxIDAgUj4+Pj4vQ29udGVudHMgMiAwIFIvUGFyZW50IDMgMCBSPj4KZW5kb2JqCjEgMCBvYmoKPDwvTGVuZ3RoIDE5ODgvRmlsdGVyWy9BU0NJSTg1RGVjb2RlL0ZsYXRlRGVjb2RlXS9SZXNvdXJjZXM8PC9Qcm9jU2V0IDUgMCBSL0ZvbnQ8PC9GMSA2IDAgUi9GMiA3IDAgUi9GMyA4IDAgUi9GNCA5IDAgUi9GNSAxMCAwIFIvRjYgMTEgMCBSL0Y3IDEyIDAgUi9GOCAxMyAwIFIvRjkgMTQgMCBSL0YxMCAxNSAwIFIvRjExIDE2IDAgUi9GMTIgMTcgMCBSPj4vWE9iamVjdDw8L0ZlZEV4R3JvdW5kIDE4IDAgUi9Hcm91bmRHIDE5IDAgUi9iYXJjb2RlMCAyMCAwIFI+Pj4+L1R5cGUvWE9iamVjdC9TdWJ0eXBlL0Zvcm0vQkJveFswIDAgNjEyIDc5Ml0vTWF0cml4WzEgMCAwIDEgMCAwXS9Gb3JtVHlwZSAxPj5zdHJlYW0KR2F0PS0+dSlEMCdSZEhUcyJLTGpQXSNYP1I2KU0nUklDZmdhTyZqMy5lLGlYUD5ERlFKcy9cJm9mXVdrMD8xV1EwUW4uQC9dWyV1ck91ZCMKRFJBdWdpJFg8LEJZOS8mWzVxMl5sJClFMTJWSzE+b1tYVTsiX1FmQEAoWG44V3JsNmM5Zz1aTlc5TSZBNzd1U14uRWRdRCs0SHNsbzIxXU0KPzdcOSNdYyFnNCNDI2QvU0pUYmk3ZjFZPCcnaVVFXkwjZENEcTE/UWVZaCshUTxcYVRGbyhxMWxmbURMRUJWKG1ObG1fKCdhR0FaczVXR0MKcl04SCtoKFJaaFtjOm9fTmprQikySWI+MEcjVz1zPkgzVkUwdWYrZiQ5cyRWczdnWSFldWw5N2NZRk9pNCNaRHJIVGc2PzszW1psJkY/SWgKZStaQiE3QmIvcUlRcXBMQmwkPlFOZUFmTUEhSUYtTVlxcSclZDJeYz5PcVBBKipYJmNtTjtYRldnIUQnalJlTUJCQi4rXT1IMVojZWA7Kk4KaWVKXjs0cl83OmBzMmk+WVAwK0RfJTA/W0JtXmsiN1I3NGNgW1NyOjdKMVFsRUNYaztkVWdHR2pEOCQ1bEFSYmFJTDY9KmYtSitHJC8sWFEKKmA7OkteK008OGE6OU1uOERCU1RMZlYuTzQ9XmBuYFUsKV86P1UsZSxmOEQ/WDJLLGlCKVplbjZcbTAyaWxVPDk3PFxVcklXOWY1O0dIJSEKMyhmXC1iNFFxQjtQUjA7UV0nT1dkVW1tdE5wTiZAVF8kVixfckNTLSFpP287RGcvW2xNViw+SmwwIkhxXFhWRF4/ZDNKXDtaLy8yJWk5V2MKY0FNQz5bZj5aOjQ8Ml9BajRVUytjR3VpJE5bQlAxajJqcyVwRFUyI2U9SnFKWElHTyo5LUAia24yZmU7XSV0PmAtLWI8bEwmQiU3NnVlQ1IKWzI2X2VRXk5wYU4wZTcqUSNDR3AxX1NNaHAjLSomUjlmNFszNHRgMV09aCpobz9oSzUoVCFVSGdHYlhLPmcoJj9AJ2xeUk4lTU5TTydYNSwKYkVQUi4xV3VkNEwtamlkYz1EZTpObWZtOWo3SE0jKiYybnRCV3FxajduVVRXTmwhOldpYUxrLVxOWmcnKEhpQypWaC9mcVFBPC1VVkVnbV8KRVU9THA2aGtoRT1vKTwrN1U8SmxEKCQ/OFNWbmkxTlFKQ2gwQ00qS1lAXWgiYF8/YCIoUy0mSU4/Z0Y1azNEU3ArYWRmZnA2bTRDS1ooLW8KUmoiOmZNcWMkcUdyQF4zMlBTNV9NK1BhXUEkZiQ/NGFYUVVQMGEzNVcrNmpiWFtVI2MxLltBIidLMkxcJkYvNTMzck9VMCpJdW1AQzJaZ2MKJnEqJ0RPL3FrV1RWWnRBKCxlY1ZWaVJuTT42JFhpa3M9PD01XSUzV0hGUSJfaWM2NFQjQi0ua1ZQKURZZVxxYlZaO2k3a11KaDc6UCJsTmgKQitJN2U4Yi5QPzZZLiEqQkUyLzhqO01vcylUZFtVLTU7X2teblQ3Yj0hM0pkQyJqZ0ckalw0Yzw5YU1NWkIhI2ZhKSZoJCVePD01ZS1CJS4KYDEuNCxgUzYzMj9WOWVnQCcqPT8sUidXaDo7Ky5nWyMoajUxIUBUV1lsSGN0OSEpTSU9SUZIUzJfZDw3XikoXytRQmNkMzh1OidTMVE7LWoKSUFcQD9fcHJsN19BOldKWS1BaSlbVEgrallmYz1QbWJCdXBrQF1ILSgzVEpoOGdTOWEwcSJIOEs1a3JePGJyaltSPDotV109RCdGbDJgRSMKWWpBYG9VL2RwQjlAOCNKPlwyMythYy4xXVBIaUNMRjA6OyFOIkpOOVtIQHNcT1xmOXNsMnJQIihLVC1gbShTTCYuKDVkNFlnUl8tXCVEOTcKTjhQKmFNcVFHXmVnZTUjXEBCVkNhImdYL2c7TVArVWBII3QsbHEnbEtCbVBEQkUyLyQmIiZkXDRzQVBkRm5FNlpWI2F0MDR0cHRfZzJLRGYKJCdAaEg0Xj4rTGNnQi1dQ3FWR1JPVFdCayxyWEYmOiwiZWFJQW80OCEsSWovWSdQbDNOLCFJMiRtLElNQWZlczZTJTYmZEBwIlJAQW1RUV0KMnVQRj5FYmtAby8vWmxKViZsa2gtRy1xOkAyWmteJCxtMzEhM0QtQ05LJ19YbypOYT05QU9eaj1EPHVPXVhSXSU9RjdUZzhJYl9ONCtwZzUKMlBiNXRoQTFma1w0VltMYy04NyZZJnNxY1ZDOXByPF50MlBHIkZxXyJjL0NySEZRUypdWyguMjRKUztaPD9AbmJEOERUV0Boc1pTTGA3KGcKa2xXSS0rTjFvKDVoUThTRU5yakleZTJAMVA5Mzo+LCQpaWRlLUIkIzBEXHJDZDcycFJiJitQQS8qXFwjXTVxYXBoNS49QklTJkdTKThbQCEKa21VKjdDTipOMGc+LSxpYUhGUS5LIWlXKUZIPTBrTl5KOkA9OlEnbmFDbF8wS2NRaWBMQSxyUUNOVVApTHEwZS0iUmUlMCYxSTFJLSxcXFAKRGBcPFQ7NFFGZF4wWSxRNGI4Pmc0YXE2RElsUFQ6IkQza1U1R2g5PCZIfj4KZW5kc3RyZWFtCmVuZG9iago1IDAgb2JqClsvUERGL1RleHQvSW1hZ2VCL0ltYWdlQy9JbWFnZUldCmVuZG9iago2IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L0hlbHZldGljYS9FbmNvZGluZy9NYWNSb21hbkVuY29kaW5nPj4KZW5kb2JqCjcgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvSGVsdmV0aWNhLUJvbGQvRW5jb2RpbmcvTWFjUm9tYW5FbmNvZGluZz4+CmVuZG9iago4IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L0hlbHZldGljYS1PYmxpcXVlL0VuY29kaW5nL01hY1JvbWFuRW5jb2Rpbmc+PgplbmRvYmoKOSAwIG9iago8PC9UeXBlL0ZvbnQvU3VidHlwZS9UeXBlMS9CYXNlRm9udC9IZWx2ZXRpY2EtQm9sZE9ibGlxdWUvRW5jb2RpbmcvTWFjUm9tYW5FbmNvZGluZz4+CmVuZG9iagoxMCAwIG9iago8PC9UeXBlL0ZvbnQvU3VidHlwZS9UeXBlMS9CYXNlRm9udC9Db3VyaWVyL0VuY29kaW5nL01hY1JvbWFuRW5jb2Rpbmc+PgplbmRvYmoKMTEgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvQ291cmllci1Cb2xkL0VuY29kaW5nL01hY1JvbWFuRW5jb2Rpbmc+PgplbmRvYmoKMTIgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvQ291cmllci1PYmxpcXVlL0VuY29kaW5nL01hY1JvbWFuRW5jb2Rpbmc+PgplbmRvYmoKMTMgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvQ291cmllci1Cb2xkT2JsaXF1ZS9FbmNvZGluZy9NYWNSb21hbkVuY29kaW5nPj4KZW5kb2JqCjE0IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L1RpbWVzLVJvbWFuL0VuY29kaW5nL01hY1JvbWFuRW5jb2Rpbmc+PgplbmRvYmoKMTUgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvVGltZXMtQm9sZC9FbmNvZGluZy9NYWNSb21hbkVuY29kaW5nPj4KZW5kb2JqCjE2IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L1RpbWVzLUl0YWxpYy9FbmNvZGluZy9NYWNSb21hbkVuY29kaW5nPj4KZW5kb2JqCjE3IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L1RpbWVzLUJvbGRJdGFsaWMvRW5jb2RpbmcvTWFjUm9tYW5FbmNvZGluZz4+CmVuZG9iagoxOCAwIG9iago8PC9UeXBlL1hPYmplY3QvU3VidHlwZS9JbWFnZS9XaWR0aCAxMTgvSGVpZ2h0IDQ3L0NvbG9yU3BhY2UvRGV2aWNlUkdCL0JpdHNQZXJDb21wb25lbnQgOC9MZW5ndGggNjkyL0ZpbHRlclsvQVNDSUk4NURlY29kZS9GbGF0ZURlY29kZV0+PnN0cmVhbQpHYiIvJjk5K11bJS4hakZicSk9TjlNOFY8OigoT2plWT1EW0xUOWlJTXI8NkFPTGpVZGxQPGVabXJAJGxLdWA7b2ssX0JNO0hMYSQ6Xk88cAotK2suJzkpKFFUKS0pYFIpLFVeRDdOZ1s3JEUnaFU9IUsjIkU8cDdVRTVEVjNwK1NRVmFSWHInSEwnI248bEJvSWA0UigvVDkkSWNFW0cqPQppR2JQNUR0PiVTVU4tbkw7cydxTVxlQVMxW0dSIWJmcG5vckFJNVUsQWg/MD0tQ3AoJWFeYlEnQGRMXFpXTGc4T1NEXjVSQ2JncGBsaGg8Vwo+KSw7c2VLMUtsbE03Kj84bENxYD86OVtgITZcP1dbMShaXz1DPkNQTzxQM3JfQSNUQzFMSVVCLCIzT28yJTpeUUFpRSNiLzBbTSgqZWN0cwpHQFAtZG1Acl5lRj8hXC9kKUYpZ0QyJ1dFb3BiMVknLjNBV1NBbSsxUTUsQF1YO0ZpSW5MdEZeUCdSaDFRKkAqN1g5I2srPEFGJnBCIlpQaApdNUYvQ2Y/KVl1LDwsXC5TV21hbltPcy49ZGxINmRsXTNzWTo1bitNLz9zQSkqX3NfRF5zcTpjQ0pOKipSIy88Li8naipsQyoyJS5PbGJnJwpVTTUpTThlX2JnbC9GLWkmNVk/QGgjOUswWjwsLi5mJ1tzQjNgPWEtIzphKmExVk4laSRBQ2c/WVJZZiVZPHMsbSw5X1IzYV1gJURxZGpTVQpna00udEhcbVtyXnQtTi9sZUNJUlxubzk2MGwtUTY6PiEsXDdzRlxlPlY0aSNrQmJwZ1whPmJWaFNaTF5uKTtqT2xAZSdhRFZeJT5HVyMhQgo+K0g5VCE4ZSJZUypwIjs8WEdDMyIzdSVpJyEqa3FPLyxeOjVPKz0tRD9+PgplbmRzdHJlYW0KZW5kb2JqCjE5IDAgb2JqCjw8L1R5cGUvWE9iamVjdC9TdWJ0eXBlL0ltYWdlL1dpZHRoIDU0L0hlaWdodCA1NC9Db2xvclNwYWNlL0RldmljZVJHQi9CaXRzUGVyQ29tcG9uZW50IDgvTGVuZ3RoIDI1MC9GaWx0ZXJbL0FTQ0lJODVEZWNvZGUvRmxhdGVEZWNvZGVdPj5zdHJlYW0KR2IiMEpfI10wQiRxIW5jP2ItWm1QL3JlKytJMkJqSVctOTBRPk83WzokV0ooQzNDYUEiTnM3WEA3MCQsVHUlYkBZUXVoblgiMWQuYVNVNlIKJ0c9M0FZN3ItZyxHJyliJktZOE9LZlRiMkdfSy4oVD90UlNZMlEiVztNYjRaJlsoMWAobmsjZSslIjdBKHVcLEE7cmdPb3I+U0UpcnRObyYKJWNLOEE9PFdIOSciWDFSODtAYUVLJypna0lLYjQxKFI+RSkpMjApJS9qOWdiT1wwbU1lPCwnIy5cclc8Kz1JQlFRQUAwI0NhV1lXQVhScm8KPUlqal1+PgplbmRzdHJlYW0KZW5kb2JqCjIwIDAgb2JqCjw8L1R5cGUvWE9iamVjdC9TdWJ0eXBlL0ltYWdlL1dpZHRoIDI3Ny9IZWlnaHQgNzAvQ29sb3JTcGFjZS9EZXZpY2VSR0IvQml0c1BlckNvbXBvbmVudCA4L0xlbmd0aCA0OTM3L0ZpbHRlclsvQVNDSUk4NURlY29kZS9GbGF0ZURlY29kZV0+PnN0cmVhbQpHYiIvYWQ7RUwqJUtrJldzKWQuUiJVOiFlODFQXGhHJlBgViNpKkg1N0AqTGpJRXEjJ2NLQlQja0J0UzxJKlZjRkgsQkM/QyZBdSdmQFRXUwpUO1Y6JF49JC1gNkE3WDJrXDwsMzYrVThAWyc7WmhTOmBaQS1HbkRrLk9TYD41JSxWMmpSQFEucmQvVFZXVDtNQydUTWR1WUhYJV81VVFdRQpHdGs4Ll8qMl0zVz5TQS07WTAwMy9hSS5vOUs8P0NJSiFrVSxIQDhtcCVqPnFQYGlyVWwyJ2tiLkFgMic9JjphKW89aSgmT29oMGNpOHVubgoqWDJHRmwpLyxFb2gzUTFnVSsmX2VNNi8qPDsraWVOL2FjVjE9a2IpKC9kYTdjX3AqRCNIKmVZNSReSiU9TzZqT2xVVkEpNkBjJHBuYFAsYAo5YFJcQS9nPSI1R3VmQnBMPzhtJmlVW0JSJ0JCVVI7LSFzdWwiTzJAcXFxa2FDNkNhcVFlIUVZLj51SiRkPzhzJldzVGk7QlghbERkV2IpLApZUT5PakNaOlNwSl0+RS42aD8kJG5HKjRkSUVTMXMtbj5HS29KQSlbSTtmQWsyK2FFdDk1PjA9UFhUNU1CTSN0Tjw8XENYMWchRl1sKz9YUQo9RmdnRjIwJGRzNVwmXVJVJXEkInBqPC9DXj8mKnRQU0Y3NkhDQyxpNHVlMUZSXFdgdVYpOFFZYitjNDdbJXVVOFdXR1s8KUQnSGkmIWVzbgpPIWRbbywkM0w5UXRAY1Q5IV0vSmllTGEyOk1SMyRccThxTStCOjBDQ191USsuJV40SF45NCtCSF9bRiEvQjU3JlpSXS0wOjtqWFNjKylHXApFWW9cSVdydGxtck1OZVRASV81ZWNtTF4oRl43OlE+RUM+JiciM15COSwoMEw/PS1kXE03cTs3OC8+XUBKXzpkPigrM1NITGg+SC82OTEhYApscj0rXkpIIiQhYDNCJHEoP2dzTm0oMD9tXDFMNzo5RjlJbV0tVz1eNS1NLj9rZXU9ay4zc2hWWS8nQjJRSyhhMz0kS0FOKVRqRWJWQUItMAprcik7Yig5WDZDXSs/citMKDJ0WCRZN1FaJ01wYGUsLyM7UXIrJDAhbC1IMzIlW2VRazhITiFePCVzO0FedUIhMydWSFhFTk4jP3JDOl1DZwpjL1I1L0QmMzdkYkhSXFBHXysoQVQqIjlePE1bKyJSXCo3czRvTW9KZWZmKmYzXzk9MExeTCMhJHM8RmRNWlFrUldBMWdFOkpKPDxaPDxdRApWWDZfLkNgLC1RKEdDazFsVmcoKVRTaUItPDpjOyZVby1HRGBdQFhqOmFaUVcmUS1CLTo5Q2ciNz5gZ0tzLT09VWQmTXNLQHRnXlJkQ0VpOApwZzBWOTlkUmZPYiUoWVFhY29NXTFrYy5SNF83S0VvKlstKTM4a1BpbEBGTm4kUVRdZm1mRE1BZDwwUnUmVldmPUE/Om1CLjY9cEgpVyhxMApZLFRBNztqaVNwNUZKZWBQLD9VQVw+TG1YRS40SywidCxwLTZUbjciQEExVHRpQjteXTUuW21sLiYmQ0VQKUwtPTNecjR0NjAhPENlaHJeMApJayRXb24wXy1tNi4tR2MpIyc3YCghXEo+YDhIO0lCVykuQ0Z0dWBQLiQ0aFJHIlxxJ0NKcm8/YWVwMltJLTRGSmBSUEFiTVFSMWBBYilYdApVYkgrSFRQKWRDJCJOXitpWzxHSlRmJWxzKldTaT5NUHNwbDNec2U0bjUqZEQ5PD9vZlVPYzpYVl1NamNrPlFCPmVJNGonWHA3cDtudGA9XgowT0A+REJAUUFWQStyZ1otZV50XjIzMSRFISkkWFVXKm0ocTdNRjtAOmhWK0lYUEBjaUpQajRcKmlhVk47ZlRlLCRxLHVVIz81OipVb1U6Rgo/OmdmJmVGKl1ScEpCaXNnLWhWbE8kT2ZlOTQhS2wiQHEmdFg8MUBuck9FZi5HVUVJaGNMdTkjbFJQXyI9MEVMSWNkdSohOHRwPFRoTSJYWQpoPDIoLWZtRCZ0OmpDcV1GcF1XZGctbW9PKV1JYjNJXEcoSCEtM2JobXJ1NCxxY1ZnSU4zNkpDQT1wJydbJDN1PXJrJlcsJV9MJXVJQnJVPQpaKF5bKEgwYzVBTVI4OlgiPy8wVzgsWWNIRFpKaShwIyM0UzBERGZlM3VNPjtCI20vQzpNRDg3YHIxMCExKihQMWJQR2M8NF8kazxMJUVxRwpURzIpVjs4RylsLWdGVyZZRytbU2BbYCdFKDR0KXIwcjxDa1NrLDBwOVgraSE7YzU4VTgnSCdgUkA8VEowOl5OaWtUciNQS010TSdmaW5IIgonRiFobT5UcjwwbEsqSERRV0g1dC8/YzZJPyNgSzlrPDRxWVVSPlIlOHVKSWteNWVURl9ENCIrP1hHZGxWOWhrSUwnWF1BYGo8LFNsbU40bgpOT1Y9JVZwcm9lVyVZPnJXKipHNk8xTz44O1U5XDY9W2szZCpnQkRrXW1MaFRkdE1ASl47JUMkO0czXk1iUj4oWS4/LnRZIVNDNmBZUzoubwpgSWo/NG9aZykpWVRSJ3VDNitLTlRYPVxUVk5xUTVlaVo0QnBhK1lbbFFOX28sKSdLY1AnTHUyZURcSERQSDonbVcldDNsZCU1W1NQR2pcIworZjJWTU86Nj8qYVxrdV9OKzwhZG9qcUpaWUA4UlM5RGZGaXAlPUROKU5kQGs3YGk8UDknWW5VZFJkS18zYE1NRkFTQkUocElXbFZfaVJkTgovNGJOLWNYYSNSNFtxMlhkZV44UVBaUCxOSjBmWiRwQTAqVi9XRTtwPUpLRWA+OiJGZTEkS3NhQUtAZEVXQTpobisrdEBxMTk/Yy07SWFFXwpDJDNnJktmLko2UVMhZWJSUk91ZWwlSS91Zi49bWEkRlMzVlk6M0g+ckNNcGUrXCZvKG4wVUoiMypRYUgnITsmWWFKbDgiMCgmU2laKjxAbwowYDMyc1JST3VlbCVJL3VmLj1tYSRGUzNWWTozSD5yQ01wZStcJm8objBVSiIzKlFhSCchOyZZYUpsOCIwKCZTaVoqPEBvMGAzMnNSUk91ZQpsJUkvdWYuPW1hJEZTM1ZZOjNIPnJDTXBlK1wmbyhuMFVKIjMqUzptX3E1Z0IpU1EmUS9JSSVwTDczX0IpRXMtNjZbdCVpWTRUPWUrJChfQwpxLURiUG9XLXJYV0teU2dDOnVzSkVkbUBPRnBLU0o6OWs2U09wUkldcmlIcmleYFohPTYicGUjYGc8QWA3T006Nj8oRExzJylfWzFtPSJpXQpFOCI4YFhdRGk5K15bNW9Gc29FVFEiaF0iIlZdIjI5LyQ8R01hQ2c1VHIrS2lMLzZqUWNsXSRILyxjV3JMSkEvKUliNDlXckciPC8uXSQxSAo+LUYka1MwJ2hXKkotbmtQPidaUUNJMjQvbHNoQlo5WmpvZzU8MlxRTUxtRGpSLVIzLi43NGshT05yZ2gsdWk7ZDo2SVJLUHFNR0VGKlVRNQpCaGRrXEhoVWE7ODJqSVJiOHJObWw/KHNLOHJHWmU4LFZkT05WMHQxSGcuOXVQVF5ESzhFXCFFJmg3VEItVjloYThzZjczXFwnPCtabUtwaQpeJidCWFVtaWc6QVdJYHFGWk9KNlYrJG9uLENYajhgcWojKV1sWyt1YiljYzZVaVUkMyQnZD8yUDlgbWtWK1owVT5pKFpQPipEInBoWWUzPApFWVcsWm9ZYS8vJU1tRW5AUyhNcVlRISNjTTdnRk03ImFSaVM5dVozTm1qOi5KXSwua11iQipyPF1XMCJzJk1jNzgmTENZKl5PS1NPZktVIQo/Rixdc1hBOlpKSGMlclROOGghOjtRcUhVaklJbXBTOXVaM05tajouSl0sLmtdYkIqcjxdVzAicyZNYzc4JkxDWSpeT0tTT2ZLVSE/RixdcwpYQTpaSkhjJXJUTjhoITo7UXFIVWpJSW1wUzl1WjNObWo6LkpdLC5rXWJCKnI8XVcwInMmTWM3OCZMQ1kqXk9LUyZKW3VsMzB1NT43L24sZwovWUE9Zj41bDErbUdpWlxDYDU8L0xKZ01EZSJDVVJeV2NXKW5uVkcuXVVuTk5JZDFfZjNEUD8tNmo7JjREWVV1anI2dDRQPjVsMSttR2laXApDYDU8L0xKZ01Ea3MrdF1MLGxWS3ErRTpBQCxldStxUjZtcCdRcUJLLld1OGpWSipbMm1bIk1hRksyIi9bP29sWStfPCRuIkdcaXRjOVVvJAo+OCRQZTQ5Nj49OFowMD1JUTMkbSo1bD07bGdCMz4waCV1STNhLWtaaEEmcTNuayFlJmUpTGlOSiRnUlcwP3Q1bTs0TWE+WDVTaDQ9TjNXWApCImRYI1FQIS88V2g/LHBRZjs5b0FhYl5OVSo1ZSpNQylcJU5zOmJzTU5rWy1DNy8yJEJccWY8VEBjWz8zXSY5alBDPkhLJmY6c2pGa2skJQovJEBqbzwjRSczQVhxaFo1Vj02MVInO2NfVS5TSzxpK28nXDtdYXQraWE6MSlrO0RDQEhWTUgkOzY4VyE6Wz9aMydwSTYjOG1qJ0dlU3VbMApZLjA0WTtkbjxqUT5yKFsha0YsY0ApPlpuTSgyZjlKP09mYTozaGVKTTE4Nz9UQF1JSm01NUorOCRSQyM0ZnI0aDxfUWkrLiJTKGI8bF1WXgpcYCtOVjo1KixHPWlPJF4kS05EKkpuNXE8UUJxJnA1OC07VmNuJCotM2tcRD5yTUdtZF06az4jR00rJzUzRkMpPENgMFJCNjdTWGYxbzJeYQpiZ2ZeRTtkJ21cOEJPUVw4N0FaSikwIS9TVllhKD5BVHBLYklJRkkrUzh1PzgmX0pqTi0mIztgKWVob0crSVQ6MiFpTkNSUzw5JiRccmpqNwpwRzFaS0xKN0gzPjA/OGY2TWZQTUlsUVA7NFg3MUJiWHI1QUoyWWk/WSE9YSdGUWVlWiVKKlBLLT1uKFhHMzdsKm1tT1hROTlWc3MnZGssOAo5aWsvXEBRbk1NQFU6OTBUKXJBLi5aY0o1NzhWW2VQLDhhPV9GdDpdX0lgc3VUKjVfJidTWy0hZG81bV5VPipuZzhZcVovJzlLTXMvI1ovcwpULzouN1lFPilHSlpnYUI4PUdcY1csLTM/ZHRcV0ctQTwlP1pcI206IWYnXF1fWyE3LC49KyknRDxCOUI7RytuNzg2T0NWZT5BVS8pUkdnXwphTkNFcj0tcz0/OWg+cHIyUlRARjZdKlJdX01GOlNuOjdEZENRITVbKCtPKmRSTiw5dTVcbk50VTdAKnJhbE44NDErQitWLHJvZSZIZTdOJQoyVDJiMVc1Y0xiT3NTQ3VWNF1HcFxNYWUtTVtJbVU7Ol8lQTA/TDYiRyxcJzkrTWdYXzA6XGkuTU1LPTdxLl9aNlZfVFBZQ2xgcXBsLEdYIgpZJUZXUF1iSFZVYz9RMVkvcXBobyQ+NTgoZFdTTD1RPTJHZ1dqNmZJOFpqMlA4XEhkSVVuITx0YWxOODQxK0IrVixyb2UmSGU3TiUyVDJiMQpXNWNMYk9zU0N1VjRdR3BcTWFlLU1bSl9sIWA5SE9YJjozQSY8UmZAKTZjcWNjVTVnO0o8US1rVGl1M0dOWygmITdsY2ViOEwzRGwyWmJPQgpkXiZMZjNKRm5BW2QuYCYlZGlyXlMzQDFuTltQZG9RN0dQTCg2Vj1dWCt1aWEhQEsxYTZGRzxsKkNpKCJOY0hPTlAiPF9hRFpqMWRVaC9mVgpFWEhaYUN0STsqKlNUaUYzSU1CZipEblNoL1FcKyIvTDZaRDx0P1RNIV91P0tLa2RVYzNmXC8jKjlDIyYsYSI9TGg/Xj9ROF4jUTZqVjZFTApmcmhPMzxIK3FqKFFqPDJyMDxqIy9ZS0pBKmBDWV4uQEQmVkxrcFdjUSxvalNyNFpVOWhAYEQrODNZS3UnJVpqME4yaU8mNGtEUCFlVl81cwo7VylASjtacWZJZHV0SnEtOSlzUWFbNkReNHNgITAkUD82dFRHJSdiTCM0aGplNj0rZyYnOEpcOFx1REdSU2FnLFY8PkYlVGl0O1E+Ny1tWwpBcjw1bUZGJywwY05JbXUyTC5pJWNfaztjQTBFLSgmbF4nSVpMSWFHUSRRKEtrb29qcjU9bnU7P1NlXjZbZSpoVi82IiwsQjFpaWRpOS06KgpWInQiMjZWTzhPXFZnInRVZy8hRT1kOV90UktFOWdjPlJNYjZYPylnSTlkOzExPT0rQUw9MDQ9PW5LaGZuWyY8cydbRSFnODoiPlUuOWNQaAouO0FEa1JjYCVOKVc1REJrUW5JRC5uRE4kbmRDdUZmUmVqaDEpXXIiKVoiIT1ORSRGYVpkUS9DUEhYIlwiMCpjPi9PYmNuWHUnJWBucm0jIwprVjRwPVYkQWxuZjYjQ1BBNXVabEMncyM8MzU7XiUlPFo1O29RPCVgYVtAKUspRzRZPUFeTWw0YTN0N0JrQnRTPF1SQmJhZF0mZVJ+PgplbmRzdHJlYW0KZW5kb2JqCjMgMCBvYmoKPDwvVHlwZS9QYWdlcy9Db3VudCAxL0tpZHNbNCAwIFJdL0lUWFQoNS4xLjEpPj4KZW5kb2JqCjIxIDAgb2JqCjw8L1R5cGUvQ2F0YWxvZy9QYWdlcyAzIDAgUj4+CmVuZG9iagoyMiAwIG9iago8PC9Qcm9kdWNlcihpVGV4dFNoYXJwIDUuMS4xIFwoY1wpIDFUM1hUIEJWQkEpL0NyZWF0aW9uRGF0ZShEOjIwMTQwNDAzMTA0MzEwLTA1JzAwJykvTW9kRGF0ZShEOjIwMTQwNDAzMTA0MzEwLTA1JzAwJyk+PgplbmRvYmoKeHJlZgowIDIzCjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDMwNCAwMDAwMCBuIAowMDAwMDAwMDE1IDAwMDAwIG4gCjAwMDAwMTAyNDAgMDAwMDAgbiAKMDAwMDAwMDE0MyAwMDAwMCBuIAowMDAwMDAyNjY5IDAwMDAwIG4gCjAwMDAwMDI3MTcgMDAwMDAgbiAKMDAwMDAwMjgwNiAwMDAwMCBuIAowMDAwMDAyOTAwIDAwMDAwIG4gCjAwMDAwMDI5OTcgMDAwMDAgbiAKMDAwMDAwMzA5OCAwMDAwMCBuIAowMDAwMDAzMTg2IDAwMDAwIG4gCjAwMDAwMDMyNzkgMDAwMDAgbiAKMDAwMDAwMzM3NSAwMDAwMCBuIAowMDAwMDAzNDc1IDAwMDAwIG4gCjAwMDAwMDM1NjcgMDAwMDAgbiAKMDAwMDAwMzY1OCAwMDAwMCBuIAowMDAwMDAzNzUxIDAwMDAwIG4gCjAwMDAwMDM4NDggMDAwMDAgbiAKMDAwMDAwNDcxMSAwMDAwMCBuIAowMDAwMDA1MTMxIDAwMDAwIG4gCjAwMDAwMTAzMDMgMDAwMDAgbiAKMDAwMDAxMDM0OSAwMDAwMCBuIAp0cmFpbGVyCjw8L1NpemUgMjMvUm9vdCAyMSAwIFIvSW5mbyAyMiAwIFIvSUQgWzw4MjFmOTc4NjEwNjZhMmUxMzFhM2UxYjQ3NjY4NTVhYT48MzU4Njk4ZTIxMWRmNjVjNzViNjFkNDAzZGI5OWJjMTc+XT4+CnN0YXJ0eHJlZgoxMDQ4NQolJUVPRgo="
			};
		}
	}

	[ DataContract ]
	public sealed class ShipStationShippingLabelRequest
	{
		[ DataMember( Name = "orderId" ) ]
		public string OrderId{ get; set; }

		[ DataMember( Name = "carrierCode" ) ]
		public string CarrierCode{ get; set; }

		[ DataMember( Name = "serviceCode" ) ]
		public string ServiceCode{ get; set; }

		[ DataMember( Name = "packageCode" ) ]
		public string PackageCode{ get; set; }

		[ DataMember( Name = "confirmation" ) ]
		public string Confirmation{ get; set; }

		[ DataMember( Name = "shipDate" ) ]
		public string ShipDate{ get; set; }

		[ DataMember( Name = "weight" ) ]
		public WeightModel Weight{ get; set; }

		[ DataMember( Name = "testLabel" ) ]
		public bool TestLabel{ get; set; }

		public static ShipStationShippingLabelRequest From( string shipStationOrderId, string carrierCode, string serviceCode, string packageCode, string confirmation, DateTime shipDate, string weight, string weightUnit, bool isTestLabel )
		{
			var shippingLabelRequest = new ShipStationShippingLabelRequest()
			{
				OrderId = shipStationOrderId,
				CarrierCode = carrierCode,
				ServiceCode = serviceCode,
				PackageCode = packageCode,
				Confirmation = confirmation,
				ShipDate = shipDate.ToString( "yyyy-MM-dd" ),
				TestLabel = isTestLabel
			};

			if( weight != null && weightUnit != null )
				shippingLabelRequest.Weight = new WeightModel( weight, weightUnit );
			return shippingLabelRequest;
		}
	}
}
