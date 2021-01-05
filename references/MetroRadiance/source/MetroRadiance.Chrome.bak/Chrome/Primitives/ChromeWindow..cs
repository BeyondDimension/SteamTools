using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using MetroRadiance.Interop;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Platform;
using MetroRadiance.Properties;

namespace MetroRadiance.Chrome.Primitives
{
	[TemplatePart(Name = PART_GlowingEdge, Type = typeof(GlowingEdge))]
	internal abstract class ChromeWindow : Window
	{
		static ChromeWindow()
		{
			AllowsTransparencyProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(true));
			ResizeModeProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(ResizeMode.NoResize));
			ShowActivatedProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(false));
			ShowInTaskbarProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(false));
			VisibilityProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(Visibility.Collapsed));
			WindowStyleProperty.OverrideMetadata(
				typeof(ChromeWindow),
				new FrameworkPropertyMetadata(WindowStyle.None));
		}

#pragma warning disable IDE1006
		private const string PART_GlowingEdge = nameof(PART_GlowingEdge);
#pragma warning restore IDE1006

		public static double Thickness { get; set; } = 8.0;

		private readonly WindowInteropHelper _windowInteropHelper;
		private HwndSource _source;
		private IntPtr _handle;
		private bool _sourceInitialized;
		private bool _closed;
		private bool _firstActivated;
		private WindowState _ownerPreviewState;

		protected Dpi SystemDpi { get; private set; }
		protected Dpi CurrentDpi { get; private set; }

		public new IChromeOwner Owner { get; private set; }

		public SizingMode SizingMode { get; set; }

		public GlowingEdge Edge { get; private set; }

		#region Thickness dependency property

		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
			nameof(Offset), typeof(Thickness), typeof(ChromeWindow), new PropertyMetadata(new Thickness(Thickness), OffsetChangedCallback));

		public Thickness Offset
		{
			get { return (Thickness)this.GetValue(OffsetProperty); }
			set { this.SetValue(OffsetProperty, value); }
		}


		private static void OffsetChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (ChromeWindow)d;
			instance.UpdateDpiResources();
		}

		#endregion

		#region DpiScaleTransform dependency property

		public static readonly DependencyProperty DpiScaleTransformProperty = DependencyProperty.Register(
			nameof(DpiScaleTransform), typeof(Transform), typeof(ChromeWindow), new PropertyMetadata(Transform.Identity));

		public Transform DpiScaleTransform
		{
			get { return (Transform)this.GetValue(DpiScaleTransformProperty); }
			set { this.SetValue(DpiScaleTransformProperty, value); }
		}

		#endregion

		protected ChromeWindow()
		{
			this.Width = .0;
			this.Height = .0;
			this._windowInteropHelper = new WindowInteropHelper(this);
		}


		public void Attach(Window window)
		{
			var binding = new Binding(nameof(this.BorderBrush)) { Source = window, };
			this.SetBinding(BorderBrushProperty, binding);

			var wrapper = WindowWrapper.Create(window);
			var initialShow = window.IsLoaded;

			this.Attach(wrapper, initialShow);
		}

		public void Attach(IChromeOwner window)
		{
			Action<Color> applyAccent = color =>
				this.BorderBrush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));

			var disposable = WindowsTheme.Accent.RegisterListener(applyAccent);
			this.Closed += (sender, e) => disposable.Dispose();
			applyAccent(WindowsTheme.Accent.Current);

			this.Attach(window, true);
		}

		private void Attach(IChromeOwner window, bool initialShow)
		{
			this.Detach();

			this._windowInteropHelper.Owner = window.Handle;
			this.Owner = window;
			this.Owner.StateChanged += this.OwnerStateChangedCallback;
			this.Owner.LocationChanged += this.OwnerLocationChangedCallback;
			this.Owner.SizeChanged += this.OwnerSizeChangedCallback;
			this.Owner.Activated += this.OwnerActivatedCallback;
			this.Owner.Deactivated += this.OwnerDeactivatedCallback;
			this.Owner.Closed += this.OwnerClosedCallback;

			if (initialShow)
			{
				this._ownerPreviewState = this.Owner.WindowState;
				this.Show();
				this.UpdateState(forceImmediate: true);
				this.UpdateLocationAndSize();
			}
			else
			{
				this.Owner.ContentRendered += this.OwnerContentRenderedCallback;
			}
		}

		public void Detach()
		{
			var owner = this.Owner;
			if (owner != null)
			{
				this.Owner = null;
				base.Owner = null;

				owner.StateChanged -= this.OwnerStateChangedCallback;
				owner.LocationChanged -= this.OwnerLocationChangedCallback;
				owner.SizeChanged -= this.OwnerSizeChangedCallback;
				owner.Activated -= this.OwnerActivatedCallback;
				owner.Deactivated -= this.OwnerDeactivatedCallback;
				owner.Closed -= this.OwnerClosedCallback;
				owner.ContentRendered -= this.OwnerContentRenderedCallback;
				this._windowInteropHelper.Owner = IntPtr.Zero;
			}
			this.Visibility = Visibility.Collapsed;
			this._firstActivated = false;
			this._closed = false;
		}

		private bool GetIsUpdateAvailable()
		{
			return this.Owner != null && this._sourceInitialized && !this._closed;
		}

		private void UpdateState(bool forceImmediate = false)
		{
			if (!this.GetIsUpdateAvailable()) return;

			if (this.Owner.Visibility == Visibility.Hidden)
			{
				this.Visibility = Visibility.Hidden;
			}
			else if (this.Owner.WindowState == WindowState.Normal)
			{
				if (this._ownerPreviewState == WindowState.Minimized
					&& SystemParameters.MinimizeAnimation
					&& !forceImmediate)
				{
					Action<Task> action = t =>
					{
						if (t.IsCompleted)
						{
							this.Visibility = Visibility.Visible;
							this.UpdateLocationAndSizeCore();
						}
						else if (t.IsFaulted)
						{
							t.Exception.Dump();
						}
					};

					// 最小化から復帰 && 最小化アニメーションが有効の場合
					// アニメーションが完了しウィンドウが表示されるまで遅延させる (それがだいたい 250 ミリ秒くらい)
					Task.Delay(Settings.Default.DelayForMinimizeToNormal)
						.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext())
						.ContinueWith(t => t.Exception.Dump(), TaskContinuationOptions.OnlyOnFaulted);
				}
				else
				{
					this.Visibility = Visibility.Visible;
					this.UpdateLocationAndSizeCore();
				}
			}
			else
			{
				this.Visibility = Visibility.Collapsed;
			}
		}

		protected void UpdateLocation()
		{
			if (!this.GetIsUpdateAvailable() || this._ownerPreviewState != WindowState.Normal) return;

			this.CheckDpiChange();

			var ownerRect = GetWindowRect(this.Owner.Handle);
			var left = this.GetLeft(ownerRect);
			var top = this.GetTop(ownerRect);
			var flags = SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSENDCHANGING;

			User32.SetWindowPos(this._handle, IntPtr.Zero, left, top, 0, 0, flags);
		}

		protected void UpdateSize()
		{
			if (!this.GetIsUpdateAvailable() || this._ownerPreviewState != WindowState.Normal) return;

			if (this.CheckDpiChange())
			{
				this.UpdateLocationAndSizeCore();
				return;
			}

			var ownerRect = GetWindowRect(this.Owner.Handle);
			var width = this.GetWidth(ownerRect);
			var height = this.GetHeight(ownerRect);
			var flags = SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSENDCHANGING;

			User32.SetWindowPos(this._handle, IntPtr.Zero, 0, 0, width, height, flags);
		}

		private void UpdateLocationAndSize()
		{
			if (!this.GetIsUpdateAvailable() || this._ownerPreviewState != WindowState.Normal) return;

			this.CheckDpiChange();
			this.UpdateLocationAndSizeCore();
		}

		private void UpdateLocationAndSizeCore()
		{
			var ownerRect = GetWindowRect(this.Owner.Handle);
			var left = this.GetLeft(ownerRect);
			var top = this.GetTop(ownerRect);
			var width = this.GetWidth(ownerRect);
			var height = this.GetHeight(ownerRect);
			var flags = SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSENDCHANGING;

			User32.SetWindowPos(this._handle, IntPtr.Zero, left, top, width, height, flags);
		}

		private bool CheckDpiChange()
		{
			if (PerMonitorDpi.IsSupported)
			{
				var currentDpi = PerMonitorDpi.GetDpi(this.Owner.Handle);
				if (currentDpi != this.CurrentDpi)
				{
					this.DpiScaleTransform = currentDpi == this.SystemDpi
						? Transform.Identity
						: new ScaleTransform((double)currentDpi.X / this.SystemDpi.X, (double)currentDpi.Y / this.SystemDpi.Y);
					this.CurrentDpi = currentDpi;
					this.UpdateDpiResources();
					this.UpdateLayout();
					return true;
				}
			}
			return false;
		}

		private static RECT GetWindowRect(IntPtr hWnd)
		{
			try
			{
				return Dwmapi.DwmGetExtendedFrameBounds(hWnd);
			}
			catch (COMException)
			{
				return User32.GetWindowRect(hWnd);
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.Edge = (GlowingEdge)this.GetTemplateChild(PART_GlowingEdge);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			if (!(PresentationSource.FromVisual(this) is HwndSource source))
			{
				throw new InvalidOperationException("HwndSource is missing.");
			}

			this._source = source;
			this._source.AddHook(this.WndProc);
			this._handle = source.Handle;

			this.SystemDpi = this.GetSystemDpi() ?? Dpi.Default;
			this.CurrentDpi = this.SystemDpi;
			this.UpdateDpiResources();

			var wndStyle = User32.GetWindowLong(this._handle);
			var wexStyle = User32.GetWindowLongEx(this._handle);

			User32.SetWindowLong(this._handle, wndStyle & ~WindowStyles.WS_SYSMENU);
			User32.SetWindowLongEx(this._handle, wexStyle | WindowExStyles.WS_EX_TOOLWINDOW);

			if (this.Owner is WindowWrapper wrapper)
			{
				base.Owner = wrapper.Window;
			}

			this._sourceInitialized = true;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (!this._sourceInitialized) return;

			this._source.RemoveHook(this.WndProc);
			this._closed = true;
		}

		protected abstract void UpdateDpiResources();
		protected abstract int GetLeft(RECT owner);
		protected abstract int GetTop(RECT owner);
		protected abstract int GetWidth(RECT owner);
		protected abstract int GetHeight(RECT owner);

		protected T GetContentValueOrDefault<T>(Func<FrameworkElement, T> valueSelector, T @default)
		{
			var element = this.Content as FrameworkElement;
			if (element == null) return @default;

			var contentControl = this.Content as ContentControl;
			if (contentControl?.Content is FrameworkElement)
			{
				return valueSelector((FrameworkElement)contentControl.Content);
			}

			return valueSelector(element);
		}

		protected double GetContentSizeOrDefault(Func<FrameworkElement, double> sizeSelector)
		{
			return this.GetContentValueOrDefault(sizeSelector, Thickness).SpecifiedOrDefault(Thickness);
		}

		private void OwnerContentRenderedCallback(object sender, EventArgs eventArgs)
		{
			var owner = (Window)sender;
			owner.ContentRendered -= this.OwnerContentRenderedCallback;

			this._ownerPreviewState = owner.WindowState;
			this.Show();
			this.UpdateState(forceImmediate: true);
			this.UpdateLocationAndSize();
		}

		private void OwnerStateChangedCallback(object sender, EventArgs eventArgs)
		{
			if (this._closed) return;
			this.UpdateState();
			this._ownerPreviewState = this.Owner.WindowState;
		}

		private void OwnerLocationChangedCallback(object sender, EventArgs eventArgs)
		{
			this.UpdateLocation();
		}

		protected virtual void OwnerSizeChangedCallback(object sender, EventArgs eventArgs)
		{
			this.UpdateLocationAndSize();
		}

		private void OwnerActivatedCallback(object sender, EventArgs eventArgs)
		{
			if (!this._firstActivated) return;
			this._firstActivated = true;
			this.UpdateState();
		}

		private void OwnerDeactivatedCallback(object sender, EventArgs eventArgs)
		{
		}

		private void OwnerClosedCallback(object sender, EventArgs eventArgs)
		{
			this.Close();
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == (int)WindowsMessages.WM_MOUSEACTIVATE)
			{
				if (!this.Owner.IsActive)
				{
					this.Owner.Activate();
				}

				handled = true;
				return new IntPtr(3);
			}

			else if (msg == (int)WindowsMessages.WM_SETTINGCHANGE)
			{
				this.ChangeSettings();
				handled = true;
				return IntPtr.Zero;
			}

			// Note: Double scaling is avoided on .NET Framework 4.6.2 or later.
			else if (msg == (int)WindowsMessages.WM_DPICHANGED)
			{
			//	System.Diagnostics.Debug.WriteLine("WM_DPICHANGED: " + this.GetType().Name);

			//	var dpiX = wParam.ToLoWord();
			//	var dpiY = wParam.ToHiWord();
			//	this.ChangeDpi(new Dpi(dpiX, dpiY));
				handled = true;
				return IntPtr.Zero;
			}

			return IntPtr.Zero;
		}

		protected virtual void ChangeSettings() { }

		internal void Resize(SizingMode mode)
		{
			this.Owner.Resize(mode);
		}

		internal void DoubleClick(SizingMode mode)
		{
			this.Owner.DoubleClick(mode);
		}
	}
}
