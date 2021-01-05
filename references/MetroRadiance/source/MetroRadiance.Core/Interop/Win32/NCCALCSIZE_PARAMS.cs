using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPOS
	{
		public IntPtr hwnd;
		public IntPtr hwndInsertAfter;
		public int x;
		public int y;
		public int cx;
		public int cy;
		public SetWindowPosFlags flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NCCALCSIZE_PARAMS
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public RECT[] rgrc;
		public WINDOWPOS lppos;
	}
}
