using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CefNet.Input;
using CefNet.Internal;
using CefNet.WinApi;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CefNet.Avalonia
{
	public partial class WebView : TemplatedControl, IAvaloniaWebViewPrivate, IDisposable
	{
		private CefRect _windowBounds;
		private IntPtr _keyboardLayout;
		private bool _allowResizeNotifications = true;
		private int _suppressLostFocusEvent = 0;
		private PointerPressedEventArgs _lastPointerPressedEventArgs;
		private Dictionary<InitialPropertyKeys, object> InitialPropertyBag = new Dictionary<InitialPropertyKeys, object>();

		/// <summary>
		/// Identifies the <see cref="StartDragging"/> routed event.
		/// </summary>
		public static readonly RoutedEvent<StartDraggingEventArgs> StartDraggingEvent = RoutedEvent.Register<WebView, StartDraggingEventArgs>(nameof(StartDragging), RoutingStrategies.Bubble);

		/// <summary>
		/// Occurs when the user starts dragging content in the web view.
		/// </summary>
		/// <remarks>
		/// OS APIs that run a system message loop may be used within the StartDragging event handler.
		/// Call <see cref="WebView.DragSourceEndedAt"/> and <see cref="WebView.DragSourceSystemDragEnded"/>
		/// either synchronously or asynchronously to inform the web view that the drag operation has ended.
		/// </remarks>
		public event EventHandler<StartDraggingEventArgs> StartDragging
		{
			add { AddHandler(StartDraggingEvent, value); }
			remove { RemoveHandler(StartDraggingEvent, value); }
		}

		public WebView()
			: this(null)
		{

		}

		public WebView(WebView opener)
		{
			if (IsDesignMode)
			{
				//BackColor = System.Drawing.Color.White;
				return;
			}
			if (opener != null)
			{
				this.Opener = opener;
				this.BrowserSettings = opener.BrowserSettings;
			}
			Initialize();

			this.GetPropertyChangedObservable(Control.BoundsProperty).Subscribe(OnBoundsChanged);

			AddHandler(InputElement.KeyDownEvent, HandlePreviewKeyDown, RoutingStrategies.Tunnel, true);
			AddHandler(InputElement.KeyUpEvent, HandlePreviewKeyUp, RoutingStrategies.Tunnel, true);
			AddHandler(DragDrop.DragEnterEvent, HandleDragEnter);
			AddHandler(DragDrop.DragOverEvent, HandleDragOver);
			AddHandler(DragDrop.DragLeaveEvent, HandleDragLeave);
			AddHandler(DragDrop.DropEvent, HandleDrop);
		}

		/// <summary>
		/// Identifies the <see cref="TextFound"/> routed event.
		/// </summary>
		public static readonly RoutedEvent TextFoundEvent = RoutedEvent.Register<WebView, TextFoundRoutedEventArgs>(nameof(TextFound), RoutingStrategies.Bubble);

		/// <summary>
		/// Adds a handler for the <see cref="TextFound"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="TextFound"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void AddTextFoundHandler(IInteractive element, EventHandler<TextFoundRoutedEventArgs> handler)
		{
			element?.AddHandler(TextFoundEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the <see cref="TextFound"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="TextFound"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void RemoveTextFoundHandler(IInteractive element, EventHandler<TextFoundRoutedEventArgs> handler)
		{
			element?.RemoveHandler(TextFoundEvent, handler);
		}

		/// <summary>
		/// Identifies the <see cref="PdfPrintFinished"/> routed event.
		/// </summary>
		public static readonly RoutedEvent PdfPrintFinishedEvent = RoutedEvent.Register<WebView, PdfPrintFinishedRoutedEventArgs>(nameof(PdfPrintFinished), RoutingStrategies.Bubble);

		/// <summary>
		/// Adds a handler for the <see cref="PdfPrintFinished"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="PdfPrintFinished"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void AddPdfPrintFinishedHandler(IInteractive element, EventHandler<PdfPrintFinishedRoutedEventArgs> handler)
		{
			element?.AddHandler(PdfPrintFinishedEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the <see cref="PdfPrintFinished"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="PdfPrintFinished"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void RemovePdfPrintFinishedHandler(IInteractive element, EventHandler<PdfPrintFinishedRoutedEventArgs> handler)
		{
			element?.RemoveHandler(PdfPrintFinishedEvent, handler);
		}

		/// <summary>
		/// Identifies the <see cref="StatusTextChanged"/> routed event.
		/// </summary>
		public static readonly RoutedEvent StatusTextChangedEvent = RoutedEvent.Register<WebView, RoutedEventArgs>(nameof(StatusTextChanged), RoutingStrategies.Bubble);

		/// <summary>
		/// Adds a handler for the <see cref="StatusTextChanged"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="StatusTextChanged"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void AddStatusTextChangedHandler(IInteractive element, EventHandler<EventArgs> handler)
		{
			element?.AddHandler(StatusTextChangedEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the <see cref="StatusTextChanged"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="StatusTextChanged"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void RemoveStatusTextChangedHandler(IInteractive element, EventHandler<EventArgs> handler)
		{
			element?.RemoveHandler(StatusTextChangedEvent, handler);
		}

		/// <summary>
		/// Identifies the <see cref="ScriptDialogOpening"/> routed event.
		/// </summary>
		public static readonly RoutedEvent ScriptDialogOpeningEvent = RoutedEvent.Register<WebView, ScriptDialogOpeningRoutedEventArgs>(nameof(ScriptDialogOpening), RoutingStrategies.Bubble);

		/// <summary>
		/// Adds a handler for the <see cref="ScriptDialogOpening"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="ScriptDialogOpening"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void AddScriptDialogOpeningHandler(IInteractive element, EventHandler<ScriptDialogOpeningRoutedEventArgs> handler)
		{
			element?.AddHandler(ScriptDialogOpeningEvent, handler);
		}

		/// <summary>
		/// Removes a handler for the <see cref="ScriptDialogOpening"/> attached event.
		/// </summary>
		/// <param name="element">An object that raise the <see cref="ScriptDialogOpening"/> routed event.</param>
		/// <param name="handler">The handler.</param>
		public static void RemoveScriptDialogOpeningHandler(IInteractive element, EventHandler<ScriptDialogOpeningRoutedEventArgs> handler)
		{
			element?.RemoveHandler(ScriptDialogOpeningEvent, handler);
		}

		protected bool IsDesignMode
		{
			get
			{
				return Design.IsDesignMode;
			}
		}

		protected OffscreenGraphics OffscreenGraphics { get; private set; }

		protected virtual void OnCreateBrowser()
		{
			if (this.Opener != null)
				return;

			if (GetState(State.Creating) || GetState(State.Created))
				throw new InvalidOperationException();

			SetState(State.Creating, true);

			Dictionary<InitialPropertyKeys, object> propertyBag = InitialPropertyBag;
			InitialPropertyBag = null;

			var avaloniaWindow = this.GetVisualRoot() as Window;
			if (avaloniaWindow is null)
				throw new InvalidOperationException("Window not found!");

			using (var windowInfo = new CefWindowInfo())
			{
				IPlatformHandle platformHandle = avaloniaWindow.PlatformImpl.Handle;
				if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle)
					windowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
				else
					windowInfo.SetAsWindowless(platformHandle.Handle);

				string initialUrl = null;
				CefDictionaryValue extraInfo = null;
				CefRequestContext requestContext = null;
				CefBrowserSettings browserSettings = null;
				if (propertyBag != null)
				{
					object value;
					if (propertyBag.TryGetValue(InitialPropertyKeys.Url, out value))
						initialUrl = value as string;
					if (propertyBag.TryGetValue(InitialPropertyKeys.BrowserSettings, out value))
						browserSettings = value as CefBrowserSettings;
					if (propertyBag.TryGetValue(InitialPropertyKeys.RequestContext, out value))
						requestContext = value as CefRequestContext;
					if (propertyBag.TryGetValue(InitialPropertyKeys.ExtraInfo, out value))
						extraInfo = value as CefDictionaryValue;
				}

				if (initialUrl == null)
					initialUrl = "about:blank";
				if (browserSettings == null)
					browserSettings = DefaultBrowserSettings;

				if (!CefApi.CreateBrowser(windowInfo, ViewGlue.Client, initialUrl, browserSettings, extraInfo, requestContext))
					throw new InvalidOperationException("Failed to create browser instance.");
			}
		}

		private void SetInitProperty(InitialPropertyKeys key, object value)
		{
			var propertyBag = InitialPropertyBag;
			if (propertyBag != null)
			{
				propertyBag[key] = value;
			}
			else
			{
				throw new InvalidOperationException("This property must be set before the underlying CEF browser is created.");
			}
		}

		private T GetInitProperty<T>(InitialPropertyKeys key)
		{
			var propertyBag = InitialPropertyBag;
			if (propertyBag != null && propertyBag.TryGetValue(key, out object value))
			{
				return (T)value;
			}
			return default;
		}

		protected virtual void Initialize()
		{
			DragDrop.SetAllowDrop(this, true);
			ToolTip = new ToolTip { IsVisible = false };
			this.ViewGlue = CreateWebViewGlue();
		}

		protected virtual WebViewGlue CreateWebViewGlue()
		{
			return new AvaloniaWebViewGlue(this);
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();
			GlobalHooks.Initialize(this);
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.ViewGlue != null)
			{
				SetState(State.Closing, true);
				BrowserObject?.Host.CloseBrowser(true);
				this.ViewGlue = null;
			}
		}

		/// <summary>
		/// Identifies the ToolTip property.
		/// </summary>
		public static readonly StyledProperty<object> ToolTipProperty = AvaloniaProperty.Register<WebView, object>("ToolTip");

		/// <summary>
		/// Gets or sets the tool-tip object that is displayed for this element in the user interface (UI).
		/// </summary>
		public object ToolTip
		{
			get { return GetValue(ToolTipProperty); }
			set { SetValue(ToolTipProperty, value); }
		}

		public string StatusText { get; protected set; }

		public override void Render(DrawingContext drawingContext)
		{
			base.Render(drawingContext);
			if (OffscreenGraphics != null)
			{
				OffscreenGraphics.Render(drawingContext);
			}
			else
			{
				drawingContext.DrawText(Brushes.Black, new Point(10, 10), new FormattedText
				{
					Text = this.GetType().Name,
					Typeface = new Typeface(FontFamily, FontStyle, FontWeight),
					FontSize = FontSize,
				});
			}
		}

		private int _fixResizeGlitchesFlag;
		/// <summary>
		/// Fixes resize glitches when maximizing or restoring the parent window if a static page is displayed.
		/// </summary>
		private void FixResizeGlitches()
		{
			// We must force the browser to be redrawn so that the new size is applied.
			// See for CEF implementation details:
			// https://bitbucket.org/chromiumembedded/cef/issues/2733/viz-osr-might-be-causing-some-graphic#comment-56271100

			if (Interlocked.Exchange(ref _fixResizeGlitchesFlag, 0) != 1)
				return;
			BrowserObject?.Host.Invalidate(CefPaintElementType.View);
		}

		protected virtual void OnBoundsChanged(EventArgs e)
		{
			if (_allowResizeNotifications)
			{
				OnUpdateRootBounds();
				if (OffscreenGraphics != null)
				{
					Rect bounds = this.Bounds;
					if (OffscreenGraphics.SetSize((int)bounds.Width, (int)bounds.Height))
					{
						double scaling = this.VisualRoot?.RenderScaling ?? 1.0;
						OffscreenGraphics.DpiScale = new DpiScale(scaling, scaling);
						BrowserObject?.Host.WasResized();

						if (Interlocked.Exchange(ref _fixResizeGlitchesFlag, 1) != 0)
							return;
						CefNetApi.Post(CefThreadId.UI, FixResizeGlitches, 30);
					}
				}
			}
		}

		protected internal virtual unsafe void OnUpdateRootBounds()
		{
			var window = this.GetVisualRoot() as Window;
			if (window != null)
			{
				RECT windowBounds;
				DpiScale scale = OffscreenGraphics.DpiScale;

				if (PlatformInfo.IsWindows && NativeMethods.DwmIsCompositionEnabled()
					&& NativeMethods.DwmGetWindowAttribute(window.PlatformImpl.Handle.Handle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, &windowBounds, sizeof(RECT)) == 0)
				{
					windowBounds = new RECT
					{
						Left = (int)Math.Floor(windowBounds.Left / scale.DpiScaleX),
						Top = (int)Math.Floor(windowBounds.Top / scale.DpiScaleY),
						Right = (int)Math.Ceiling(windowBounds.Right / scale.DpiScaleX),
						Bottom = (int)Math.Ceiling(windowBounds.Bottom / scale.DpiScaleY)
					};
				}
				else
				{
					Rect bounds = window.Bounds;
					PixelPoint pos = window.Position;
					windowBounds = new RECT {
						Left = (int)Math.Floor(pos.X / scale.DpiScaleX),
						Top = (int)Math.Floor(pos.Y / scale.DpiScaleY),
						Right = (int)Math.Ceiling(bounds.Right / scale.DpiScaleX),
						Bottom = (int)Math.Ceiling(bounds.Bottom / scale.DpiScaleY)
					};
				}
				RootBoundsChanged(windowBounds.ToCefRect());
			}
		}

		protected void SuspendResizeNotifications()
		{
			_allowResizeNotifications = false;
		}

		protected void ResumeResizeNotifications()
		{
			_allowResizeNotifications = true;
			OnBoundsChanged(EventArgs.Empty);
		}

		protected internal virtual void OnRootResizeBegin(EventArgs e)
		{
			SuspendResizeNotifications();
		}

		protected internal virtual void OnRootResizeEnd(EventArgs e)
		{
			ResumeResizeNotifications();
		}

		protected internal void RootBoundsChanged(CefRect bounds)
		{
			_windowBounds = bounds;

			if (_allowResizeNotifications)
			{
				NotifyRootMovedOrResized();
			}
		}

		private void UpdateOffscreenViewLocation()
		{
			PixelPoint screenPoint = PointToScreen(default);
			OffscreenGraphics.SetLocation(screenPoint.X, screenPoint.Y);
		}

		protected override Size MeasureOverride(Size constraint)
		{
			if (constraint.Width == 0 || constraint.Height == 0)
				return new Size(1, 1);
			return base.MeasureOverride(constraint);
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			arrangeBounds = base.ArrangeOverride(arrangeBounds);
			if (!IsDesignMode)
			{
				if (OffscreenGraphics == null)
				{
					OffscreenGraphics = new OffscreenGraphics();
					OnCreateBrowser();
				}
			}
			return arrangeBounds;
		}

		/// <summary>
		/// Gets the rectangle that represents the bounds of the WebView control.
		/// </summary>
		/// <returns>
		/// A <see cref="CefRect"/> representing the bounds within which the WebView control is scaled.
		/// </returns>
		public CefRect GetBounds()
		{
			if (OffscreenGraphics is null)
			{
				Rect bounds = this.Bounds;
				return new CefRect((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
			}
			return OffscreenGraphics.GetBounds();
		}

		/// <summary>
		/// Sets the bounds of the control to the specified location and size.
		/// </summary>
		/// <param name="x">The new <see cref="X"/> property value of the control.</param>
		/// <param name="y">The new <see cref="Y"/> property value of the control.</param>
		/// <param name="width">The new <see cref="Width"/> property value of the control.</param>
		/// <param name="height">The new <see cref="Height"/> property value of the control.</param>
		public void SetBounds(int x, int y, int width, int height)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			Width = width;
			Height = height;
		}

		protected virtual void RaiseCrossThreadEvent<TEventArgs>(Action<TEventArgs> raiseEvent, TEventArgs e, bool synchronous)
		{
			if (synchronous)
			{
				if (Dispatcher.UIThread.CheckAccess())
					raiseEvent(e);
				else
					Dispatcher.UIThread.InvokeAsync(() => raiseEvent(e)).Wait();
			}
			else
			{
				Dispatcher.UIThread.Post(() => raiseEvent(e));
			}
		}

		/// <summary>
		/// Adds a routed event handler for a specified routed event, adding the handler
		/// to the handler collection on the current element.
		/// </summary>
		/// <param name="routedEvent">An identifier for the routed event to be handled.</param>
		/// <param name="handler">A reference to the handler implementation.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AddHandler(in RoutedEvent routedEvent, Delegate handler)
		{
			AddHandler(routedEvent, handler);
		}

		/// <summary>
		/// Removes the specified routed event handler from this element.
		/// </summary>
		/// <param name="routedEvent">The identifier of the routed event for which the handler is attached.</param>
		/// <param name="handler">The specific handler implementation to remove from the event handler collection on this element.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RemoveHandler(in RoutedEvent routedEvent, Delegate handler)
		{
			RemoveHandler(routedEvent, handler);
		}

		protected virtual void OnBrowserCreated(EventArgs e)
		{
			BrowserCreated?.Invoke(this, e);
		}

		protected virtual void OnLoadingStateChange(LoadingStateChangeEventArgs e)
		{
			LoadingStateChange?.Invoke(this, e);
		}

		protected virtual void OnCefPaint(CefPaintEventArgs e)
		{
			OffscreenGraphics.Draw(e);
			CefPaint?.Invoke(this, e);
			Dispatcher.UIThread.Post(new Action(() => { this.InvalidateVisual(); }), DispatcherPriority.Render);
		}

		protected virtual void OnPopupShow(PopupShowEventArgs e)
		{
			OffscreenGraphics.SetPopup(e);
		}

		private CefPoint PointToViewport(CefPoint point)
		{
			return point;
		}

		public PixelPoint PointToScreen(Point point)
		{
			if (((IVisual)this).IsAttachedToVisualTree)
			{
				PixelPoint pixelPoint = global::Avalonia.VisualExtensions.PointToScreen(this, point);
				DpiScale dpi = OffscreenGraphics.DpiScale;
				if (dpi.DpiScaleX == 1.0 && dpi.DpiScaleY == 1.0)
					return pixelPoint;
				return new PixelPoint((int)Math.Ceiling(pixelPoint.X / dpi.DpiScaleX), (int)Math.Ceiling(pixelPoint.Y / dpi.DpiScaleY));
			}
			CefRect viewRect = OffscreenGraphics.GetBounds();
			return new PixelPoint(viewRect.X + (int)point.X, viewRect.Y + (int)point.Y);
		}

		void IChromiumWebViewPrivate.RaisePopupBrowserCreating()
		{
			SetState(State.Creating, true);
			InitialPropertyBag = null;
		}

		bool IChromiumWebViewPrivate.GetCefScreenInfo(ref CefScreenInfo screenInfo)
		{
			return false;
		}

		unsafe bool IChromiumWebViewPrivate.CefPointToScreen(ref CefPoint point)
		{
			PixelPoint ppt = new PixelPoint(point.X, point.Y);

			if (Dispatcher.UIThread.CheckAccess())
			{
				ppt = PointToScreen(new Point(ppt.X, ppt.Y));
			}
			else
			{
				Thread.MemoryBarrier();
				Dispatcher.UIThread.InvokeAsync(new Action(() =>
				{
					Thread.MemoryBarrier();
					ppt = PointToScreen(new Point(ppt.X, ppt.Y));
					Thread.MemoryBarrier();
				}), DispatcherPriority.Render).Wait();
				Thread.MemoryBarrier();
			}

			point.X = ppt.X;
			point.Y = ppt.Y;
			return true;
		}

		float IChromiumWebViewPrivate.GetDevicePixelRatio()
		{
			return (float)OffscreenGraphics.DpiScale.DpiScaleX;
		}

		CefRect IChromiumWebViewPrivate.GetCefRootBounds()
		{
			return _windowBounds;
		}

		CefRect IChromiumWebViewPrivate.GetCefViewBounds()
		{
			if (OffscreenGraphics is null)
				return new CefRect(0, 0, 1, 1);

			return OffscreenGraphics.GetBounds();
		}

		bool IChromiumWebViewPrivate.RaiseRunContextMenu(CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model, CefRunContextMenuCallback callback)
		{
			if (model.Count == 0)
			{
				callback.Cancel();
				return true;
			}
			var runner = new AvaloniaContextMenuRunner(model, callback);
			var pt = new Point(menuParams.XCoord, menuParams.YCoord);

			Interlocked.Increment(ref _suppressLostFocusEvent);
			try
			{
				if (Dispatcher.UIThread.CheckAccess())
					return RunContextMenu(runner, pt);
				return Dispatcher.UIThread.InvokeAsync(new Func<bool>(() => RunContextMenu(runner, pt))).GetAwaiter().GetResult();
			}
			finally
			{
				if (runner.CompletionTask is null)
					Interlocked.Decrement(ref _suppressLostFocusEvent);
				else
					runner.CompletionTask.ContinueWith(_ => { Interlocked.Decrement(ref _suppressLostFocusEvent); });
			}
		}

		private bool RunContextMenu(AvaloniaContextMenuRunner runner, Point position)
		{
			if (this.ContextMenu != null)
			{
				runner.Cancel();
				return true;
			}
			runner.Build();
			runner.RunMenuAt(this, position);
			return true;
		}

		void IAvaloniaWebViewPrivate.RaiseCefCursorChange(CursorChangeEventArgs e)
		{
			RaiseCrossThreadEvent(OnCursorChange, e, true);
		}

		protected virtual void OnCursorChange(CursorChangeEventArgs e)
		{
			this.Cursor = e.Cursor;
		}

		void IAvaloniaWebViewPrivate.CefSetToolTip(string text)
		{
			Dispatcher.UIThread.Post(() => OnSetToolTip(text));
		}

		protected virtual void OnSetToolTip(string text)
		{
			if (this.ToolTip is ToolTip tooltip)
			{
				if (global::Avalonia.Controls.ToolTip.GetTip(this) != tooltip)
				{
					global::Avalonia.Controls.ToolTip.SetTip(this, tooltip);
				}

				if (string.IsNullOrWhiteSpace(text))
				{
					global::Avalonia.Controls.ToolTip.SetIsOpen(this, false);
					tooltip.IsVisible = false;
				}
				else
				{
					if (!string.Equals(text, tooltip.Content as string))
						tooltip.Content = text;
					if (!tooltip.IsVisible)
						tooltip.IsVisible = true;
					global::Avalonia.Controls.ToolTip.SetIsOpen(this, true);
				}
			}
		}

		void IAvaloniaWebViewPrivate.CefSetStatusText(string statusText)
		{
			this.StatusText = statusText;
			RaiseCrossThreadEvent(OnStatusTextChanged, new RoutedEventArgs(StatusTextChangedEvent, this), false);
		}

		void IAvaloniaWebViewPrivate.RaiseStartDragging(StartDraggingEventArgs e)
		{
			RaiseCrossThreadEvent(OnStartDragging, e, true);
		}

		/// <summary>
		/// Raises <see cref="WebView.StartDragging"/> event.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected virtual async void OnStartDragging(StartDraggingEventArgs e)
		{
			RaiseEvent(e);

			if (e.Handled)
				return;

			e.Handled = true;

			await DragDrop.DoDragDrop(_lastPointerPressedEventArgs, new CefNetDragData(this, e.Data), e.AllowedEffects.ToDragDropEffects());
			DragSourceSystemDragEnded();
		}

		protected override void OnGotFocus(GotFocusEventArgs e)
		{
			BrowserObject?.Host.SetFocus(true);
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			if (Volatile.Read(ref _suppressLostFocusEvent) == 0)
				BrowserObject?.Host.SetFocus(false);
			base.OnLostFocus(e);
		}

		protected override void OnPointerMoved(PointerEventArgs e)
		{
			if (e.Pointer.Type == PointerType.Touch)
			{
				OnTouch(CefTouchEventType.Moved, e);
			}
			else
			{
				CefEventFlags modifiers = CefEventFlags.None;
				PointerPoint pointerPoint = e.GetCurrentPoint(this);
				PointerPointProperties pp = pointerPoint.Properties;
				if (pp.IsLeftButtonPressed)
					modifiers |= CefEventFlags.LeftMouseButton;
				if (pp.IsRightButtonPressed)
					modifiers |= CefEventFlags.RightMouseButton;
				Point mousePos = pointerPoint.Position;// e.GetPosition(this);
				SendMouseMoveEvent((int)mousePos.X, (int)mousePos.Y, modifiers);
			}
			base.OnPointerMoved(e);
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			SendMouseLeaveEvent();
			base.OnPointerLeave(e);
		}

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			_lastPointerPressedEventArgs = e;

			if (!IsFocused)
			{
				Focus();
			}
			if (e.GetCurrentPoint(null).Properties.PointerUpdateKind.GetMouseButton() <= MouseButton.Right)
			{
				if (e.Pointer.Type == PointerType.Touch)
				{
					OnTouch(CefTouchEventType.Pressed, e);
				}
				else
				{
					Point mousePos = e.GetPosition(this);
					SendMouseDownEvent((int)mousePos.X, (int)mousePos.Y, GetButton(e), e.ClickCount, GetModifierKeys(e.KeyModifiers));
				}
			}
			base.OnPointerPressed(e);
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			base.OnPointerReleased(e);
			if (e.GetCurrentPoint(null).Properties.PointerUpdateKind.GetMouseButton() > MouseButton.Right)
				return;

			if (e.Pointer.Type == PointerType.Touch)
			{
				OnTouch(CefTouchEventType.Released, e);
			}
			else
			{
				Point mousePos = e.GetPosition(this);
				SendMouseUpEvent((int)mousePos.X, (int)mousePos.Y, GetButton(e), 1, GetModifierKeys(e.KeyModifiers));
			}
		}

		protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
		{
			base.OnPointerWheelChanged(e);

			const int WHEEL_DELTA = 120;
			Point mousePos = e.GetPosition(this);
			SendMouseWheelEvent((int)mousePos.X, (int)mousePos.Y, (int)e.Delta.X * WHEEL_DELTA, (int)e.Delta.Y * WHEEL_DELTA);
			e.Handled = true;
		}

		private void OnTouch(CefTouchEventType eventType, PointerEventArgs e)
		{
			Point pt = e.GetPosition(this);
			CefPoint location = PointToViewport(new CefPoint((int)pt.X, (int)pt.Y));
			var eventInfo = new CefTouchEvent();
			eventInfo.Type = eventType;
			eventInfo.X = location.X;
			eventInfo.Y = location.Y;
			eventInfo.Id = e.Pointer.Id;
			eventInfo.PointerType = CefPointerType.Touch;
			eventInfo.Modifiers = (uint)GetModifierKeys(e.KeyModifiers);
			SendTouchEvent(eventInfo);
		}

		protected virtual bool ProcessPreviewKey(CefKeyEventType eventType, KeyEventArgs e)
		{
			if (PlatformInfo.IsWindows)
				SetWindowsKeyboardLayoutForCefUIThreadIfNeeded();

			CefEventFlags modifiers = GetCefKeyboardModifiers(e);
			Key key = e.Key;
			if (eventType == CefKeyEventType.KeyUp && key == Key.None)
			{
				if (e.KeyModifiers == KeyModifiers.Shift)
				{
					key = Key.LeftShift;
					modifiers |= CefEventFlags.IsLeft;
				}
			}
			
			VirtualKeys virtualKey = key.ToVirtualKey();
			bool isSystemKey = (e.KeyModifiers.HasFlag(KeyModifiers.Alt) || key == Key.LeftAlt || key == Key.RightAlt);

			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost != null)
			{
				var k = new CefKeyEvent();
				k.Type = eventType;
				k.Modifiers = (uint)modifiers;
				k.IsSystemKey = isSystemKey;
				k.WindowsKeyCode = (int)virtualKey;
				k.NativeKeyCode = KeycodeConverter.VirtualKeyToNativeKeyCode(virtualKey, modifiers, false);
				if (PlatformInfo.IsMacOS)
				{
					k.UnmodifiedCharacter = char.ToUpperInvariant(CefNet.Input.KeycodeConverter.TranslateVirtualKey(virtualKey, CefEventFlags.None));
					k.Character = CefNet.Input.KeycodeConverter.TranslateVirtualKey(virtualKey, modifiers);
				}
				this.BrowserObject?.Host.SendKeyEvent(k);

				if (key == Key.Enter && eventType == CefKeyEventType.RawKeyDown)
				{
					k.Type = CefKeyEventType.Char;
					k.Character = '\r';
					k.UnmodifiedCharacter = '\r';
					this.BrowserObject?.Host.SendKeyEvent(k);
				}
			}

			if (isSystemKey)
				return true;

			// Prevent keyboard navigation using arrows and home and end keys
			if (key >= Key.PageUp && key <= Key.Down)
				return true;

			if (key == Key.Tab)
				return true;

			// Allow Ctrl+A to work when the WebView control is put inside listbox.
			if (key == Key.A && e.KeyModifiers.HasFlag(KeyModifiers.Control))
				return true;

			return false;
		}

		private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
		{
			OnPreviewKeyDown(e);
		}

		protected virtual void OnPreviewKeyDown(KeyEventArgs e)
		{
			e.Handled = ProcessPreviewKey(CefKeyEventType.RawKeyDown, e);
		}

		private void HandlePreviewKeyUp(object sender, KeyEventArgs e)
		{
			OnPreviewKeyUp(e);
		}

		protected virtual void OnPreviewKeyUp(KeyEventArgs e)
		{
			e.Handled = ProcessPreviewKey(CefKeyEventType.KeyUp, e);
		}

		protected override void OnTextInput(TextInputEventArgs e)
		{
			foreach (char symbol in e.Text)
			{
				CefEventFlags modifiers = CefNet.Input.KeycodeConverter.IsShiftRequired(symbol) ? CefEventFlags.ShiftDown : CefEventFlags.None;

				VirtualKeys key = KeycodeConverter.CharacterToVirtualKey(symbol);

				var k = new CefKeyEvent();
				k.Type = CefKeyEventType.Char;
				k.WindowsKeyCode = PlatformInfo.IsLinux ? (int)key : symbol;
				k.Character = symbol;
				k.UnmodifiedCharacter = symbol;
				k.Modifiers = (uint)modifiers;
				k.NativeKeyCode = KeycodeConverter.VirtualKeyToNativeKeyCode(key, modifiers, false);
				this.BrowserObject?.Host.SendKeyEvent(k);
			}
			e.Handled = true;
		}

		private void HandleDragEnter(object sender, DragEventArgs e)
		{
			OnDragEnter(e);
		}

		private void HandleDragOver(object sender, DragEventArgs e)
		{
			OnDragOver(e);
		}

		private void HandleDragLeave(object sender, RoutedEventArgs e)
		{
			OnDragLeave(e);
		}

		private void HandleDrop(object sender, DragEventArgs e)
		{
			OnDrop(e);
		}

		protected virtual void OnDragEnter(DragEventArgs e)
		{
			Point mousePos = e.GetPosition(this);
			SendDragEnterEvent((int)mousePos.X, (int)mousePos.Y, e.GetModifiers(), e.GetCefDragData(), e.DragEffects.ToCefDragOperationsMask());
			e.DragEffects = DragDropEffects.Copy & e.DragEffects;
			e.Handled = true;
		}

		protected virtual void OnDragOver(DragEventArgs e)
		{
			Point mousePos = e.GetPosition(this);
			SendDragOverEvent((int)mousePos.X, (int)mousePos.Y, e.GetModifiers(), e.DragEffects.ToCefDragOperationsMask());
			e.DragEffects = e.DragEffects & DragDropEffects.Copy;
			e.Handled = true;
		}

		protected virtual void OnDragLeave(RoutedEventArgs e)
		{
			SendDragLeaveEvent();
			e.Handled = true;
		}

		protected virtual void OnDrop(DragEventArgs e)
		{
			Point mousePos = e.GetPosition(this);
			SendDragDropEvent((int)mousePos.X, (int)mousePos.Y, e.GetModifiers());

			if (e.Data.Contains(nameof(CefNetDragData)))
			{
				CefNetDragData data = (CefNetDragData)e.Data.Get(nameof(CefNetDragData));
				if (data != null && data.Source == this)
				{
					DragSourceEndedAt((int)mousePos.X, (int)mousePos.Y, e.DragEffects.ToCefDragOperationsMask());
				}
			}
			e.Handled = true;
		}

		protected static CefMouseButtonType GetButton(PointerEventArgs e)
		{
			
			switch (e.GetCurrentPoint(null).Properties.PointerUpdateKind)
			{
				case PointerUpdateKind.RightButtonPressed:
				case PointerUpdateKind.RightButtonReleased:
					return CefMouseButtonType.Right;
				case PointerUpdateKind.MiddleButtonPressed:
				case PointerUpdateKind.MiddleButtonReleased:
					return CefMouseButtonType.Middle;
			}
			return CefMouseButtonType.Left;
		}

		protected static CefEventFlags GetModifierKeys(KeyModifiers modKeys)
		{
			CefEventFlags modifiers = CefEventFlags.None;
			if (modKeys.HasFlag(KeyModifiers.Shift))
				modifiers |= CefEventFlags.ShiftDown;
			if (modKeys.HasFlag(KeyModifiers.Control))
				modifiers |= CefEventFlags.ControlDown;
			if (modKeys.HasFlag(KeyModifiers.Alt))
				modifiers |= CefEventFlags.AltDown;
			return modifiers;
		}

		protected CefEventFlags GetCefKeyboardModifiers(KeyEventArgs e)
		{
			CefEventFlags modifiers = GetModifierKeys(e.KeyModifiers);


			// TODO:
			//if (Keyboard.IsKeyToggled(Key.NumLock))
			//	modifiers |= CefEventFlags.NumLockOn;
			//if (Keyboard.IsKeyToggled(Key.CapsLock))
			//	modifiers |= CefEventFlags.CapsLockOn;

			switch (e.Key)
			{
				case Key.Return:
					//if (e.IsExtendedKey())
					//	modifiers |= CefEventFlags.IsKeyPad;
					break;
				case Key.Insert:
				case Key.Delete:
				case Key.Home:
				case Key.End:
				case Key.Prior:
				case Key.Next:
				case Key.Up:
				case Key.Down:
				case Key.Left:
				case Key.Right:
					//if (!e.IsExtendedKey())
					//	modifiers |= CefEventFlags.IsKeyPad;
					break;
				case Key.NumLock:
				case Key.NumPad0:
				case Key.NumPad1:
				case Key.NumPad2:
				case Key.NumPad3:
				case Key.NumPad4:
				case Key.NumPad5:
				case Key.NumPad6:
				case Key.NumPad7:
				case Key.NumPad8:
				case Key.NumPad9:
				case Key.Divide:
				case Key.Multiply:
				case Key.Subtract:
				case Key.Add:
				case Key.Decimal:
				case Key.Clear:
					modifiers |= CefEventFlags.IsKeyPad;
					break;
				case Key.LeftShift:
				case Key.LeftCtrl:
				case Key.LeftAlt:
				case Key.LWin:
					modifiers |= CefEventFlags.IsLeft;
					break;
				case Key.RightShift:
				case Key.RightCtrl:
				case Key.RightAlt:
				case Key.RWin:
					modifiers |= CefEventFlags.IsRight;
					break;
			}
			return modifiers;
		}

		/// <summary>
		/// Sets the current input locale identifier for the UI thread in the browser.
		/// </summary>
		private void SetWindowsKeyboardLayoutForCefUIThreadIfNeeded()
		{
			IntPtr hkl = NativeMethods.GetKeyboardLayout(0);
			if (_keyboardLayout == hkl)
				return;

			if (CefApi.CurrentlyOn(CefThreadId.UI))
			{
				_keyboardLayout = hkl;
			}
			else
			{
				CefNetApi.Post(CefThreadId.UI, () => {
					NativeMethods.ActivateKeyboardLayout(hkl, 0);
					_keyboardLayout = hkl;
				});
			}
		}
	}
}
