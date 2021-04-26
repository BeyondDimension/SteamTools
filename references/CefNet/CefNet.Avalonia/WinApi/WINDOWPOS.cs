using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CefNet.WinApi
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPOS
	{
		public IntPtr hwnd;
		public IntPtr hwndInsertAfter;
		public int x;
		public int y;
		public int cx;
		public int cy;
		public int flags;
	}

}
