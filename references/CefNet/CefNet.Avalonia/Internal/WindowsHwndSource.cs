using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using CefNet.WinApi;

namespace CefNet.Internal
{

	internal delegate IntPtr WindowsWindowProcDelegate(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled);

	internal sealed class WindowsHwndSource : CriticalFinalizerObject, IDisposable
	{
		private delegate IntPtr WindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);


		private bool _disposed;
		private IntPtr hWndProcHook;
		private readonly WindowProc fnWndProcHook;


		public static WindowsHwndSource FromHwnd(IntPtr hwnd)
		{
			const int GWLP_WNDPROC = -4;

			uint tid = NativeMethods.GetWindowThreadProcessId(hwnd, IntPtr.Zero);
			if (tid == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			var source = new WindowsHwndSource(hwnd);
			source.hWndProcHook = NativeMethods.SetWindowLong(hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(source.fnWndProcHook));
			if (source.hWndProcHook == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return source;
		}

		private WindowsHwndSource(IntPtr hwnd)
		{
			hWndProcHook = IntPtr.Zero;
			this.Handle = hwnd;
			fnWndProcHook = new WindowProc(WndProcHook);
		}

		~WindowsHwndSource()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (hWndProcHook != IntPtr.Zero)
			{
				const int GWLP_WNDPROC = -4;
				NativeMethods.SetWindowLong(this.Handle, GWLP_WNDPROC, hWndProcHook);
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public IntPtr Handle { get; }

		public WindowsWindowProcDelegate WndProcCallback { get; set; }

		private unsafe IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr retval = IntPtr.Zero;
			WindowsWindowProcDelegate wndproc = this.WndProcCallback;
			if (wndproc != null)
			{
				bool handled = false;
				retval = wndproc(hwnd, msg, wParam, lParam, ref handled);
				if (handled)
					return retval;
			}
			return NativeMethods.CallWindowProc(hWndProcHook, hwnd, msg, wParam, lParam);
		}



	}

}
