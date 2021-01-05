using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MetroRadiance.Interop.Win32
{

	public static class Dwmapi
	{
		[DllImport("Dwmapi.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void DwmGetColorizationColor([Out] out uint pcrColorization, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

		[Obsolete("MetroRadiance.Interop.Win32.DwmGetWindowAttributeAsRectangle を使用してください")]
		[DllImport("Dwmapi.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, [Out] out RECT pvAttribute, uint cbAttribute);

		[DllImport("Dwmapi.dll", EntryPoint = "DwmGetWindowAttribute", ExactSpelling = true, PreserveSig = false)]
		public static extern void DwmGetWindowAttributeAsBoolean(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, [Out] out bool pvAttribute, uint cbAttribute);

		[DllImport("Dwmapi.dll", EntryPoint = "DwmGetWindowAttribute", ExactSpelling = true, PreserveSig = false)]
		public static extern void DwmGetWindowAttributeAsRectangle(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, [Out] out RECT pvAttribute, uint cbAttribute);

		public static bool DwmGetCloaked(IntPtr hWnd)
		{
			DwmGetWindowAttributeAsBoolean(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out var cloaked, (uint)Marshal.SizeOf(typeof(bool)));
			return cloaked;
		}

		public static RECT DwmGetCaptionButtonBounds(IntPtr hWnd)
		{
			DwmGetWindowAttributeAsRectangle(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out var rect, (uint)Marshal.SizeOf(typeof(RECT)));
			return rect;
		}

		public static RECT DwmGetExtendedFrameBounds(IntPtr hWnd)
		{
			DwmGetWindowAttributeAsRectangle(hWnd, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out var rect, (uint)Marshal.SizeOf(typeof(RECT)));
			return rect;
		}
	}
}
