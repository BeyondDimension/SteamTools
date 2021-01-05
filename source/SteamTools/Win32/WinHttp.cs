using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Win32
{
	internal static class WinHttp
	{
		[DllImport("winhttp.dll", SetLastError = true)]
		internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref WinHttpCurrentUserIEProxyConfig pProxyConfig);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WinHttpCurrentUserIEProxyConfig
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool AutoDetect;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string AutoConfigUrl;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Proxy;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string ProxyBypass;

	}
}
