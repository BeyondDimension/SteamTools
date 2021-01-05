using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using ChromeHookCLR;
using MetroRadiance.Chrome;
using MetroRadiance.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Platform
{
	/// <summary>
	/// 外部プロセスのウィンドウから、位置、サイズ、およびアクティブ状態のイベントを受信できるようにします。
	/// </summary>
	public class ExternalWindow : IChromeOwner, IDisposable
	{
		private static IChromeHookService _serviceInstance;
		private readonly IChromeHook _external;

		public IntPtr Handle { get; }

		public bool IsActive { get; private set; }

		public WindowState WindowState { get; private set; }

		//public ResizeMode ResizeMode => this.Handle.GetWindowLong().HasFlag(WS.SIZEFRAME)
		//	? ResizeMode.CanResize
		//	: ResizeMode.NoResize;
		// 本当なら☝にしたいけど、別プロセスウィンドウに Resize メッセージ飛ばせないっぽいから保留
		public ResizeMode ResizeMode => ResizeMode.NoResize;

		public Visibility Visibility => User32.IsWindowVisible(this.Handle)
			? Visibility.Visible
			: Visibility.Hidden;

#pragma warning disable 0067
		public event EventHandler ContentRendered;
#pragma warning restore 0067
		public event EventHandler LocationChanged;
		public event EventHandler SizeChanged;
		public event EventHandler StateChanged;
		public event EventHandler Activated;
		public event EventHandler Deactivated;
		public event EventHandler Closed;

		/// <summary>
		/// 外部プロセスで動作している Win32 ウィンドウのハンドルを指定して、<see cref="ExternalWindow"/>
		/// クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="hWnd">ウィンドウ ハンドル。</param>
		public ExternalWindow(IntPtr hWnd)
		{
			this.Handle = hWnd;

			if (_serviceInstance == null)
			{
				_serviceInstance = ServiceFactory.CreateInstance();
			}

			this._external = _serviceInstance.Register(hWnd);
			this._external.StateChanged += this.ExternalOnStateChanged;
			this._external.SizeChanged += this.ExternalOnSizeChanged;
			this._external.WindowMoved += this.ExternalOnWindowMoved;
			this._external.Activated += this.ExternalOnActivated;
			this._external.Deactivated += this.ExternalOnDeactivated;
			this._external.Closed += this.ExternalOnClosed;
			
			this.IsActive = User32.GetForegroundWindow() == this.Handle;
			this.WindowState = WindowState.Normal;
			if (User32.IsIconic(hWnd)) this.WindowState = WindowState.Minimized;
			if (User32.IsZoomed(hWnd)) this.WindowState = WindowState.Maximized;
		}

		public bool Activate()
		{
			return User32.SetActiveWindow(this.Handle) != IntPtr.Zero;
		}

		private void ExternalOnStateChanged(IChromeHook sender, int state)
		{
			switch (state)
			{
				case 0:
					this.WindowState = WindowState.Normal;
					break;
				case 1:
					this.WindowState = WindowState.Minimized;
					break;
				case 2:
					this.WindowState = WindowState.Maximized;
					break;
			}

			this.StateChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ExternalOnSizeChanged(IChromeHook sender, Size newSize)
		{
			this.SizeChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ExternalOnWindowMoved(IChromeHook sender, Point newLocation)
		{
			this.LocationChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ExternalOnActivated(IChromeHook sender)
		{
			this.IsActive = true;
			this.Activated?.Invoke(this, EventArgs.Empty);
		}

		private void ExternalOnDeactivated(IChromeHook sender)
		{
			this.IsActive = false;
			this.Deactivated?.Invoke(this, EventArgs.Empty);
		}

		private void ExternalOnClosed(IChromeHook sender)
		{
			this._external.Dispose();
			this.Closed?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			this._external.Dispose();
		}

		public void Resize(SizingMode sizingMode)
		{
			User32.PostMessage(this.Handle, _serviceInstance.PseudoNcLButtonDownMessage, (IntPtr)sizingMode, IntPtr.Zero);
		}

		public void DoubleClick(SizingMode sizingMode)
		{
			User32.PostMessage(this.Handle, (uint)WindowsMessages.WM_NCLBUTTONDBLCLK, (IntPtr)sizingMode, IntPtr.Zero);
		}
	}
}
