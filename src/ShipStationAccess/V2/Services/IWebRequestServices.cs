using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ShipStationAccess.V2.Models.Command;

namespace ShipStationAccess.V2.Services
{
	internal interface IWebRequestServices
	{
		T GetResponse< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null );
		Task< T > GetResponseAsync< T >( ShipStationCommand command, string commandParams, CancellationToken token, int? operationTimeout = null );
		void PostData( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null );
		Task PostDataAsync( ShipStationCommand command, string jsonContent, CancellationToken token, int? operationTimeout = null );
		T PostDataAndGetResponse< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		Task< T > PostDataAndGetResponseAsync< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		T PostDataAndGetResponseWithShipstationHeader< T >( ShipStationCommand command, string jsonContent, CancellationToken token, bool shouldGetExceptionMessage = false, int? operationTimeout = null );
		bool CanSkipException( WebException e );

		DateTime? LastNetworkActivityTime { get; }
	}
}