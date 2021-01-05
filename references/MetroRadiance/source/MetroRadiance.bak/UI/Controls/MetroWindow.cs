using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MetroRadiance.Interop;
using MetroRadiance.Interop.Win32;
using ShellChrome = System.Windows.Shell.WindowChrome;
using MetroChrome = MetroRadiance.Chrome.WindowChrome;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// 表示Metro风格的窗口。
	/// </summary>
	[TemplatePart(Name = PART_ResizeGrip, Type = typeof(FrameworkElement))]
	public class MetroWindow : Window
	{
		private const string PART_ResizeGrip = "PART_ResizeGrip";

		static MetroWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroWindow), new FrameworkPropertyMetadata(typeof(MetroWindow)));
		}

		/// <summary>
		/// WPF已知的系统的DPI（主监视器的DPI）。
		/// </summary>
		private Dpi systemDpi;

		/// <summary>
		/// 显示此窗口的监视器的当前DPI。
		/// </summary>
		internal Dpi CurrentDpi { get; set; }

		private HwndSource source;
		private FrameworkElement resizeGrip;
		private FrameworkElement captionBar;

		#region ShellChrome 依存関係プロパティ

		public static readonly DependencyProperty ShellChromeProperty = DependencyProperty.Register(
			nameof(ShellChrome), typeof(ShellChrome), typeof(MetroWindow), new PropertyMetadata(null, HandleShellChromeChanged));

		public ShellChrome ShellChrome
		{
			get { return (ShellChrome)this.GetValue(ShellChromeProperty); }
			set { this.SetValue(ShellChromeProperty, value); }
		}

		private static void HandleShellChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var chrome = (ShellChrome)args.NewValue;
			var window = (Window)d;

			ShellChrome.SetWindowChrome(window, chrome);
		}

		#endregion

		#region DpiScaleTransform 依存関係プロパティ

		/// <summary>
		/// 获取或设置实现DPI缩放的<see cref ="Transform" />。
		/// </summary>
		public Transform DpiScaleTransform
		{
			get { return (Transform)this.GetValue(DpiScaleTransformProperty); }
			set { this.SetValue(DpiScaleTransformProperty, value); }
		}

		public static readonly DependencyProperty DpiScaleTransformProperty =
			DependencyProperty.Register("DpiScaleTransform", typeof(Transform), typeof(MetroWindow), new UIPropertyMetadata(Transform.Identity));

		#endregion

		#region IsRestoringWindowPlacement 依存関係プロパティ

		/// <summary>
		/// 获取或设置一个值，该值指示是否可以还原窗口的位置和大小。
		/// </summary>
		public bool IsRestoringWindowPlacement
		{
			get { return (bool)this.GetValue(IsRestoringWindowPlacementProperty); }
			set { this.SetValue(IsRestoringWindowPlacementProperty, value); }
		}
		public static readonly DependencyProperty IsRestoringWindowPlacementProperty =
			DependencyProperty.Register("IsRestoringWindowPlacement", typeof(bool), typeof(MetroWindow), new UIPropertyMetadata(false));

		#endregion

		#region WindowSettings 依存関係プロパティ

		/// <summary>
		/// 获取或设置一个对象，该对象指定如何保​​存或还原窗口的位置和大小。
		/// </summary>
		public IWindowSettings WindowSettings
		{
			get { return (IWindowSettings)this.GetValue(WindowSettingsProperty); }
			set { this.SetValue(WindowSettingsProperty, value); }
		}
		public static readonly DependencyProperty WindowSettingsProperty =
			DependencyProperty.Register("WindowSettings", typeof(IWindowSettings), typeof(MetroWindow), new UIPropertyMetadata(null));

		#endregion

		#region IsCaptionBar 添付プロパティ

		public static readonly DependencyProperty IsCaptionBarProperty =
			DependencyProperty.RegisterAttached("IsCaptionBar", typeof(bool), typeof(MetroWindow), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, IsCaptionBarChangedCallback));

		public static void SetIsCaptionBar(FrameworkElement element, Boolean value)
		{
			element.SetValue(IsCaptionBarProperty, value);
		}
		public static bool GetIsCaptionBar(FrameworkElement element)
		{
			return (bool)element.GetValue(IsCaptionBarProperty);
		}

		private static void IsCaptionBarChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = d as FrameworkElement;
			if (instance == null) return;

			var window = GetWindow(instance) as MetroWindow;
			if (window == null) return;

			window.captionBar = (bool)e.NewValue ? instance : null;

			instance.Loaded += (sender, args) =>
			{
				window.UpdateIsCaptionBarHeight();
			};
		}

		private void UpdateIsCaptionBarHeight()
		{
			var chrome = ShellChrome.GetWindowChrome(this);
			if (chrome == null) return;

			var captionBar = this.captionBar;
			if (captionBar != null)
			{
				if (this.systemDpi.Y > 0)
				{
					chrome.CaptionHeight = captionBar.ActualHeight * this.CurrentDpi.Y / this.systemDpi.Y;
				}
				else
				{
					chrome.CaptionHeight = captionBar.ActualHeight;
				}
			}
			else
			{
				chrome.CaptionHeight = 0;
			}
		}

		#endregion

		public MetroWindow()
		{
			var metroChrome = new MetroChrome();
			MetroChrome.SetInstance(this, metroChrome);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			this.source = PresentationSource.FromVisual(this) as HwndSource;
			if (this.source == null) return;
			this.source.AddHook(this.WndProc);

			this.systemDpi = this.GetSystemDpi() ?? Dpi.Default;
			if (PerMonitorDpi.IsSupported)
			{
				this.CurrentDpi = this.source.GetDpi();
				this.ChangeDpi(this.CurrentDpi);
			}
			else
			{
				this.CurrentDpi = this.systemDpi;
			}

			if (this.WindowSettings == null)
			{
				this.WindowSettings = new WindowSettings(this);
			}
			if (this.IsRestoringWindowPlacement)
			{
				this.WindowSettings.Reload();

				if (this.WindowSettings.Placement.HasValue)
				{
					var placement = this.WindowSettings.Placement.Value;
					placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
					placement.flags = 0;
					placement.showCmd = placement.showCmd == ShowWindowFlags.SW_SHOWMINIMIZED ? ShowWindowFlags.SW_SHOWNORMAL : placement.showCmd;

					User32.SetWindowPlacement(this.source.Handle, ref placement);
				}
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.resizeGrip = this.GetTemplateChild(PART_ResizeGrip) as FrameworkElement;
			if (this.resizeGrip != null)
			{
				this.resizeGrip.Visibility = this.ResizeMode == ResizeMode.CanResizeWithGrip
					? Visibility.Visible
					: Visibility.Collapsed;

				ShellChrome.SetIsHitTestVisibleInChrome(this.resizeGrip, true);
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			if (this.captionBar != null) this.captionBar.Opacity = 1.0;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			if (this.captionBar != null) this.captionBar.Opacity = 0.5;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel && this.WindowSettings != null)
			{
				var hwnd = new WindowInteropHelper(this).Handle;
				var placement = User32.GetWindowPlacement(hwnd);

				this.WindowSettings.Placement = this.IsRestoringWindowPlacement ? (WINDOWPLACEMENT?)placement : null;
				this.WindowSettings.Save();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			this.source?.RemoveHook(this.WndProc);
		}


		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_NCHITTEST)
			{
				if (this.ResizeMode == ResizeMode.CanResizeWithGrip
					&& this.WindowState == WindowState.Normal
					&& this.resizeGrip != null)
				{
					var ptScreen = lParam.ToPoint();
					var ptClient = this.resizeGrip.PointFromScreen(ptScreen);

					var rectTarget = new Rect(0, 0, this.resizeGrip.ActualWidth, this.resizeGrip.ActualHeight);
					if (rectTarget.Contains(ptClient))
					{
						handled = true;
						return (IntPtr)HitTestValues.HTBOTTOMRIGHT;
					}
				}
			}
			else if (msg == (int)WindowsMessages.WM_NCCALCSIZE)
			{
				if (wParam != IntPtr.Zero)
				{
					var result = this.CalcNonClientSize(hwnd, lParam, ref handled);
					if (handled) return result;
				}
			}
			else if (msg == (int)WindowsMessages.WM_DPICHANGED)
			{
				var dpiX = wParam.ToLoWord();
				var dpiY = wParam.ToHiWord();
				var rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
				this.ChangeDpi(new Dpi(dpiX, dpiY), rect);
				handled = true;
			}

			return IntPtr.Zero;
		}

		private IntPtr CalcNonClientSize(IntPtr hWnd, IntPtr lParam, ref bool handled)
		{
			if (!User32.IsZoomed(hWnd)) return IntPtr.Zero;

			var rcsize = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);
			if (rcsize.lppos.flags.HasFlag(SetWindowPosFlags.SWP_NOSIZE)) return IntPtr.Zero;

			var hMonitor = User32.MonitorFromWindow(hWnd, MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
			if (hMonitor == IntPtr.Zero) return IntPtr.Zero;

			var monitorInfo = new MONITORINFO()
			{
				cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO))
			};
			if (!User32.GetMonitorInfo(hMonitor, ref monitorInfo)) return IntPtr.Zero;

			var workArea = monitorInfo.rcWork;
			AppBar.ApplyAppbarSpace(monitorInfo.rcMonitor, ref workArea);

			rcsize.rgrc[0] = workArea;
			rcsize.rgrc[1] = workArea;
			Marshal.StructureToPtr(rcsize, lParam, true);
			handled = true;
			return (IntPtr)(WindowValidRects.WVR_ALIGNTOP | WindowValidRects.WVR_ALIGNLEFT | WindowValidRects.WVR_VALIDRECTS);
		}

		private void ChangeDpi(Dpi dpi, RECT rect)
		{
			if (!PerMonitorDpi.IsSupported) return;

			this.ChangeDpi(dpi);

			User32.SetWindowPos(
				this.source.Handle,
				IntPtr.Zero,
				rect.Left,
				rect.Top,
				rect.Right - rect.Left,
				rect.Bottom - rect.Top,
				SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER);

			this.CurrentDpi = dpi;

			this.UpdateIsCaptionBarHeight();
		}

		private void ChangeDpi(Dpi dpi)
		{
			this.DpiScaleTransform = dpi == this.systemDpi
				? Transform.Identity
				: new ScaleTransform((double)dpi.X / this.systemDpi.X, (double)dpi.Y / this.systemDpi.Y);
		}
	}
}
