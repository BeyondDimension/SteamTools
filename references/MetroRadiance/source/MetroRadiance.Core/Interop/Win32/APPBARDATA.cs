using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	[StructLayout(LayoutKind.Sequential)]
	public struct APPBARDATA
	{
		public int cbSize;
		public IntPtr hWnd;
		public uint uCallbackMessage;
		public AppBarEdges uEdge;
		public RECT rc;
		public IntPtr lParam;
	}
}
