using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	public enum MONITORINFOF : uint
	{
		MONITORINFOF_PRIMARY = 1
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MONITORINFO
	{
		public uint cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public MONITORINFOF dwFlags;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct MONITORINFOEX
	{
		public uint cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public MONITORINFOF dwFlags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string szDevice;
	}
}
