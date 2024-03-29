﻿using System.Diagnostics;
using System.Reflection;

namespace ShipStationAccess
{
	/// <summary>
	/// Project-wise constants.
	/// </summary>
	internal static class Constants
	{
		public static readonly string VersionInfo = FileVersionInfo.GetVersionInfo( Assembly.GetExecutingAssembly().Location ).FileVersion;
		public const string ChannelName = "shipstation";
		public const string LoggingCommonPrefix = "[{Channel}] [{Version}] [{TenantId}] [{ChannelAccountId}] [{CorrelationId}] [{CallerType}] [{CallerMethodName}] ";
	}
}