using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SteamTools.Win32
{
	internal static class NativeMethods
	{
		[DllImport("Avrt.dll")]
		public static extern IntPtr AvSetMmThreadCharacteristics(string taskName, ref uint taskIndex);
	}
}
