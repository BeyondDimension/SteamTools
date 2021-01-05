using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Platform
{
	public abstract class RawWindow
	{
		public string Name { get; set; }

		public HwndSource Source { get; private set; }

		public IntPtr Handle => this.Source?.Handle ?? IntPtr.Zero;

		public virtual void Show()
		{
			this.Show(new HwndSourceParameters(this.Name));
		}

		protected void Show(HwndSourceParameters parameters)
		{
			this.Source = new HwndSource(parameters);
			this.Source.AddHook(this.WndProc);
		}

		public virtual void Close()
		{
			this.Source?.RemoveHook(this.WndProc);
			this.Source?.Dispose();
			this.Source = null;

			User32.CloseWindow(this.Handle);
		}

		protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			return IntPtr.Zero;
		}
	}
}
