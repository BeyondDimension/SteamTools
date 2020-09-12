using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SteamTools.Win32
{
	[StructLayout(LayoutKind.Sequential)]
	internal class RECT
	{
		public int left;
		public int top;
		public int width;
		public int height;
	}
}