using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MetroRadiance.Interop.Win32
{
	// ReSharper disable once InconsistentNaming
	public static class SHCore
	{
		[DllImport("SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern void GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);
	}
}
