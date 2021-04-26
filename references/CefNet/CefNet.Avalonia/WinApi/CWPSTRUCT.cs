using System;
using System.Runtime.InteropServices;

namespace CefNet.WinApi
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct CWPSTRUCT
	{
		public IntPtr lParam;
		public IntPtr wParam;
		public int message;
		public IntPtr hwnd;
	}
}
