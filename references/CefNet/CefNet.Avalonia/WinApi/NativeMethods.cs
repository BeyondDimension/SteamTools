using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CefNet.WinApi
{
	static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, int flags);

		[DllImport("user32.dll", EntryPoint = "VkKeyScanW", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern ushort VkKeyScan(char ch);

		public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
		}

		[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr pPid);

		[DllImport("Dwmapi.dll", CharSet = CharSet.Auto)]
		public static unsafe extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, void* value, int size);

		[DllImport("Dwmapi.dll", CharSet = CharSet.Auto, PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();
	}
}
