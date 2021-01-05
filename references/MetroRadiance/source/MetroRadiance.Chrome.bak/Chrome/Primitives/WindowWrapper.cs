using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Platform;

namespace MetroRadiance.Chrome.Primitives
{
	internal class WindowWrapper : IChromeOwner
	{
		#region WindowWrapper 添付プロパティ

		public static readonly DependencyProperty WindowWrapperProperty =
			DependencyProperty.RegisterAttached("WindowWrapper", typeof(WindowWrapper), typeof(Window), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

		public static void SetWindowWrapper(FrameworkElement element, WindowWrapper value)
		{
			element.SetValue(WindowWrapperProperty, value);
		}
		public static WindowWrapper GetWindowWrapper(FrameworkElement element)
		{
			return (WindowWrapper)element.GetValue(WindowWrapperProperty);
		}

		#endregion
		

		public static WindowWrapper Create(Window window)
		{
			var wrapper = GetWindowWrapper(window);
			if (wrapper == null)
			{
				wrapper = new WindowWrapper(window);
				SetWindowWrapper(window, wrapper);
			}

			return wrapper;
		}
		

		public Window Window { get; }
		public IntPtr Handle { get; private set; }
		public bool IsActive => this.Window.IsActive;
		public WindowState WindowState => this.Window.WindowState;
		public ResizeMode ResizeMode => this.Window.ResizeMode;
		public Visibility Visibility => this.Window.Visibility;

		public event EventHandler ContentRendered
		{
			add { this.Window.ContentRendered += value; }
			remove { this.Window.ContentRendered -= value; }
		}
		public event EventHandler LocationChanged
		{
			add { this.Window.LocationChanged += value; }
			remove { this.Window.LocationChanged -= value; }
		}
		public event EventHandler StateChanged
		{
			add { this.Window.StateChanged += value; }
			remove { this.Window.StateChanged -= value; }
		}
		public event EventHandler Activated
		{
			add { this.Window.Activated += value; }
			remove { this.Window.Activated -= value; }
		}
		public event EventHandler Deactivated
		{
			add { this.Window.Deactivated += value; }
			remove { this.Window.Deactivated -= value; }
		}
		public event EventHandler Closed
		{
			add { this.Window.Closed += value; }
			remove { this.Window.Closed -= value; }
		}
		public event EventHandler SizeChanged;

		private WindowWrapper(Window window)
		{
			this.Window = window;
			this.Window.SourceInitialized += (sender, args) =>
			{
				var source = PresentationSource.FromVisual(this.Window) as HwndSource;
				if (source != null) this.Handle = source.Handle;
			};
			this.Window.SizeChanged += (sender, args) => this.SizeChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool Activate() => this.Window.Activate();

		public void Resize(SizingMode sizingMode)
		{
			User32.PostMessage(this.Handle, (uint)WindowsMessages.WM_NCLBUTTONDOWN, (IntPtr)sizingMode, IntPtr.Zero);
		}

		public void DoubleClick(SizingMode sizingMode)
		{
			User32.PostMessage(this.Handle, (uint)WindowsMessages.WM_NCLBUTTONDBLCLK, (IntPtr)sizingMode, IntPtr.Zero);
		}
	}
}
