using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// ウィンドウをリサイズするためのグリップ コントロールを表します。
	/// </summary>
	public class ResizeGrip : ContentControl
	{
		static ResizeGrip()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeGrip), new FrameworkPropertyMetadata(typeof(ResizeGrip)));
		}

		private bool canResize;
		private bool isInitialized;

		public ResizeGrip()
		{
			this.Loaded += this.Initialize;
		}

		private void Initialize(object sender, RoutedEventArgs args)
		{
			if (this.isInitialized) return;

			var window = Window.GetWindow(this);
			if (window == null) return;

			var source = (HwndSource)PresentationSource.FromVisual(window);
			if (source != null) source.AddHook(this.WndProc);

			window.StateChanged += (_, __) => this.canResize = window.WindowState == WindowState.Normal;
			window.ContentRendered += (_, __) => this.canResize = window.WindowState == WindowState.Normal;

			this.isInitialized = true;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_NCHITTEST && this.canResize)
			{
				var ptScreen = lParam.ToPoint();
				var ptClient = this.PointFromScreen(ptScreen);

				var rectTarget = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

				if (rectTarget.Contains(ptClient))
				{
					handled = true;
					return (IntPtr)HitTestValues.HTBOTTOMRIGHT;
				}
			}

			return IntPtr.Zero;
		}
	}
}
