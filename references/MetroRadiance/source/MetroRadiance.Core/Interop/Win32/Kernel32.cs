using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace MetroRadiance.Interop.Win32
{
	public static class Kernel32
	{
		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hReservedNull, LoadLibraryExFlags dwFlags);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FreeLibrary(IntPtr hModule);
	}
}
