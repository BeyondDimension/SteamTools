using CefNet.Input;
using CefNet.Internal;


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if WPF
namespace CefNet.Wpf
#elif WINFORMS
namespace CefNet.Windows.Forms
#elif AVALONIA
namespace CefNet.Avalonia
#elif MODERNFORMS
namespace CefNet.Modern.Forms
#else
namespace CefNet
#endif
{
#if WINDOWLESS
	partial class WindowlessWebView
#else
	partial class WebView
#endif
		: IChromiumWebView, IChromiumWebViewPrivate
	{
		[Flags]
		private enum State
		{
			NotInitialized = 0,
			Creating = 1 << 0,
			Created = 1 << 1,
			Closing = 1 << 2,
			Closed = 1 << 3,
		}

		private volatile State _state;
		private CefBrowserSettings _browserSettings;
		private WeakReference openerWeakRef;
		private CefMouseEvent _mouseEventProxy = new CefMouseEvent();

		/// <summary>
		/// Occurs before a new browser window is opened.
		/// </summary>
		public event EventHandler<CreateWindowEventArgs> CreateWindow;

		/// <summary>
		/// Occurs before a CefFrame navigates to a new document.
		/// </summary>
		public event EventHandler<BeforeBrowseEventArgs> BeforeBrowse;

		/// <summary>
		/// Occurs before the WebView control navigates to a new document.
		/// </summary>
		public event EventHandler<BeforeBrowseEventArgs> Navigating;

		/// <summary>
		/// Occurs when the WebView control has navigated to a new document
		/// and has begun loading it.
		/// </summary>
		public event EventHandler<NavigatedEventArgs> Navigated;

		/// <summary>
		/// Occurs when a frame&apos;s address has changed.
		/// </summary>
		public event EventHandler<AddressChangeEventArgs> AddressChange;

		/// <summary>
		/// Occurs when the loading state has changed.
		/// </summary>
		public event EventHandler<LoadingStateChangeEventArgs> LoadingStateChange;

		/// <summary>
		/// Occurs just before a browser is destroyed.
		/// </summary>
		/// <remarks>
		/// Release all references to the <see cref="BrowserObject"/> and do not attempt to execute
		/// any methods on the <see cref="CefBrowser"/> object (other than <see cref="CefBrowser.Identifier"/>
		/// or <see cref="CefBrowser.IsSame"/> after this event.<para/>
		/// This event will be the last notification that references <see cref="BrowserObject"/> on the UI thread.
		/// Any in-progress network requests associated with <see cref="BrowserObject"/> will be aborted when
		/// the browser is destroyed, and <see cref="CefResourceRequestHandler"/> callbacks related to those
		/// requests may still arrive on the IO thread after this method is called.
		/// </remarks>
		public event EventHandler Closed;

		/// <summary>
		/// Occurs when a browser has recieved a request to close.
		/// </summary>
		public event EventHandler<CancelEventArgs> Closing;

		/// <summary>
		/// Occurs when an view or a pop-up window should be painted.<para/>
		/// This event can be occurred on a thread other than the UI thread.
		/// </summary>
		/// <remarks>
		/// This event is only occurred when <see cref="CefWindowInfo.SharedTextureEnabled"/>
		/// is set to false.
		/// </remarks>
		public event EventHandler<CefPaintEventArgs> CefPaint;

		/// <summary>
		/// Occurs when a new browser is created.
		/// </summary>
		public event EventHandler BrowserCreated;

		/// <summary>
		/// Occurs when the page title changes.
		/// </summary>
		public event EventHandler<DocumentTitleChangedEventArgs> DocumentTitleChanged;

		/// <summary>
		/// Occurs when the <see cref="StatusText"/> property value changes.
		/// </summary>
		public event EventHandler<EventArgs> StatusTextChanged
		{
			add { AddHandler(in StatusTextChangedEvent, value); }
			remove { RemoveHandler(in StatusTextChangedEvent, value); }
		}

		/// <summary>
		/// Occurs when a DevTools protocol event is available.
		/// </summary>
		public event EventHandler<DevToolsProtocolEventAvailableEventArgs> DevToolsProtocolEventAvailable;

		private static CefBrowserSettings _DefaultBrowserSettings;

		//private LifeSpanGlue lifeSpanHandler;
		//private CefRequestHandler requestHandler;
		//private CefDisplayHandler _displayHandler;
		//private CefResourceRequestHandler resourceRequestHandler;
		//private CefCookieAccessFilter cookieAccessFilter;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool GetState(State state)
		{
			return _state.HasFlag(state);
		}

		private void SetState(State flag, bool value)
		{
			_state = (value ? (_state | flag) : (_state & ~flag));
		}

		/// <summary>
		/// Closes the current browser window.
		/// </summary>
		public void Close()
		{
			((IDisposable)this).Dispose();
		}

		protected WebViewGlue ViewGlue { get; private set; }

		/// <summary>
		/// The CefBrowserSettings applied for new instances of WevView.
		/// Any changes to these settings will only apply to new browsers,
		/// leaving already created browsers unaffected.
		/// </summary>
		[Browsable(false)]
		public static CefBrowserSettings DefaultBrowserSettings
		{
			get
			{
				if (_DefaultBrowserSettings == null)
				{
					_DefaultBrowserSettings = new CefBrowserSettings();
				}
				return _DefaultBrowserSettings;
			}
		}

		/// <summary>
		/// The CefBrowserSettings applied for new instances of WevView.
		/// Any changes to these settings will only apply to new browsers,
		/// leaving already created browsers unaffected.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CefBrowserSettings BrowserSettings
		{
			get
			{
				return _browserSettings ?? DefaultBrowserSettings;
				//return GetInitProperty<CefBrowserSettings>(InitialPropertyKeys.BrowserSettings);
			}
			set
			{
				SetInitProperty(InitialPropertyKeys.BrowserSettings, value);
				_browserSettings = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CefRequestContext RequestContext
		{
			get
			{
				if (GetState(State.Created))
					return BrowserObject?.Host.RequestContext;
				return GetInitProperty<CefRequestContext>(InitialPropertyKeys.RequestContext);
			}
			set
			{
				SetInitProperty(InitialPropertyKeys.RequestContext, value);
			}
		}

		/// <summary>
		/// Gets and sets an extra information specific to the created browser
		/// that will be passed to <see cref="CefNetApplication.OnBrowserCreated"/>
		/// in the render process.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CefDictionaryValue ExtraInfo
		{
			get
			{
				if (GetState(State.Created))
					return null;
				return GetInitProperty<CefDictionaryValue>(InitialPropertyKeys.ExtraInfo);
			}
			set
			{
				SetInitProperty(InitialPropertyKeys.ExtraInfo, value);
			}
		}

		/// <summary>
		/// Gets and sets a default URL.
		/// </summary>
		/// <remarks>
		/// This property cannot be used after the browser is created.
		/// </remarks>
		public string InitialUrl
		{
			get
			{
				if (GetState(State.Created))
					return "about:blank";
				return GetInitProperty<string>(InitialPropertyKeys.Url) ?? "about:blank";
			}
			set
			{
				SetInitProperty(InitialPropertyKeys.Url, value);
			}
		}

		/// <summary>
		/// Gets the associated <see cref="CefBrowser"/> object.
		/// </summary>
		[Browsable(false)]
		public CefBrowser BrowserObject
		{
			get
			{
				return ViewGlue?.BrowserObject;
			}
		}

		[Browsable(false)]
		public CefClient Client
		{
			get
			{
				SetState(State.Creating, true);
				return ViewGlue?.Client;
			}
		}

		protected CefBrowser AliveBrowserObject
		{
			get { return BrowserObject; }
		}

		protected CefBrowserHost AliveBrowserHost
		{
			get { return AliveBrowserObject.Host; }
		}

		/// <summary>
		/// Gets a WebView that opened the current WebView. If this WebView was not
		/// opened by being linked to or created by another, returns null.
		/// </summary>
		[Browsable(false)]
		public IChromiumWebView Opener
		{
			get
			{
				WeakReference weakRef = openerWeakRef;
				if (weakRef != null && weakRef.IsAlive)
				{
					return weakRef.Target as IChromiumWebView;
				}
				return null;
			}
			protected set
			{
				openerWeakRef = value != null ? new WeakReference(value) : null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a previous page in navigation history is available, which allows the GoBack() method to succeed.
		/// </summary>
		[Browsable(false)]
		public bool CanGoBack
		{
			get
			{
				return AliveBrowserObject.CanGoBack;
			}
		}

		/// <summary>
		/// Navigates the WebView control to the previous page in the navigation history, if one is available.
		/// </summary>
		/// <returns>
		/// True if the navigation succeeds; false if a previous page in the navigation history is not available.
		/// </returns>
		public bool GoBack()
		{
			CefBrowser browser = AliveBrowserObject;
			if (!browser.CanGoBack)
				return false;
			browser.GoBack();
			GC.KeepAlive(browser);
			return true;
		}

		/// <summary>
		/// Gets a value indicating whether a subsequent page in navigation history is available, which allows the GoForward() method to succeed.
		/// </summary>
		[Browsable(false)]
		public bool CanGoForward
		{
			get
			{
				return AliveBrowserObject.CanGoForward;
			}
		}

		/// <summary>
		/// Navigates the WebView control to the next page in the navigation history, if one is available.
		/// </summary>
		/// <returns>
		/// True if the navigation succeeds; false if a subsequent page in the navigation history is not available.
		/// </returns>
		public bool GoForward()
		{
			CefBrowser browser = AliveBrowserObject;
			if (!browser.CanGoForward)
				return false;
			browser.GoForward();
			GC.KeepAlive(browser);
			return true;
		}

		/// <summary>
		/// Gets a value indicating whether the WebView control is currently loading.
		/// </summary>
		[Browsable(false)]
		public bool IsBusy
		{
			get
			{
				return AliveBrowserObject.IsLoading;
			}
		}

		/// <summary>
		/// Reload the current page.
		/// </summary>
		public void Reload()
		{
			AliveBrowserObject.Reload();
		}

		/// <summary>
		/// Reload the current page ignoring any cached data.
		/// </summary>
		public void ReloadIgnoreCache()
		{
			AliveBrowserObject.ReloadIgnoreCache();
		}

		/// <summary>
		/// Stop loading the page.
		/// </summary>
		public void Stop()
		{
			AliveBrowserObject.StopLoad();
		}

		/// <summary>
		/// Get the globally unique identifier for this browser. This value is also
		/// used as the tabId for extension APIs.
		/// </summary>
		[Browsable(false)]
		public int Identifier
		{
			get { return AliveBrowserObject.Identifier; }
		}

		/// <summary>
		/// Gets a value indicating whether a document has been loaded in the browser.
		/// </summary>
		[Browsable(false)]
		public bool HasDocument
		{
			get
			{
				return AliveBrowserObject.HasDocument;
			}
		}

		/// <summary>
		/// Returns the main (top-level) frame for the browser window.
		/// </summary>
		public CefFrame GetMainFrame()
		{
			return BrowserObject?.MainFrame;
		}

		/// <summary>
		/// Returns the focused frame for the browser window.
		/// </summary>
		public CefFrame GetFocusedFrame()
		{
			return BrowserObject?.FocusedFrame;
		}

		/// <summary>
		/// Returns the frame with the specified identifier, or null if not found.
		/// </summary>
		public CefFrame GetFrame(long identifier)
		{
			return BrowserObject?.GetFrameByIdent(identifier);
		}

		/// <summary>
		/// Returns the frame with the specified name, or null if not found.
		/// </summary>
		public CefFrame GetFrame(string name)
		{
			return BrowserObject?.GetFrame(name);
		}

		/// <summary>
		/// Returns the number of frames that currently exist.
		/// </summary>
		public int GetFrameCount()
		{
			return (int)AliveBrowserObject.FrameCount;
		}

		/// <summary>
		/// Gets the identifiers of all existing frames.
		/// </summary>
		public long[] GetFrameIdentifiers()
		{
			CefBrowser browser = BrowserObject;
			if (browser == null)
				return new long[0];

			long count = browser.FrameCount << 1;
			var identifiers = new long[count];
			browser.GetFrameIdentifiers(ref count, ref identifiers);
			GC.KeepAlive(browser);
			return identifiers;
		}

		/// <summary>
		/// Gets the names of all existing frames.
		/// </summary>
		public string[] GetFrameNames()
		{
			using (var names = new CefStringList())
			{
				AliveBrowserObject.GetFrameNames(names);
				return names.ToArray();
			}
		}

		/// <summary>
		/// Get and set the current zoom level. The default zoom level is 0.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public double ZoomLevel
		{
			get
			{
				VerifyAccess();
				return AliveBrowserHost.ZoomLevel;
			}
			set
			{
				AliveBrowserHost.ZoomLevel = value;
			}
		}

		/// <summary>
		/// Download the file at |url|.
		/// </summary>
		/// <param name="url">
		/// The URL of the file to load.
		/// </param>
		public void DownloadFile(string url)
		{
			if (url == null)
				throw new ArgumentNullException(nameof(url));

			AliveBrowserHost.StartDownload(url);
		}

		/// <summary>
		/// Download |image_url| and execute |callback| on completion with the images  received from
		/// the renderer.  
		/// </summary>
		/// <param name="imageUrl">
		/// </param>
		/// <param name="isFavicon">
		/// If |is_favicon| is True then cookies are not sent and not accepted during download.
		/// </param>
		/// <param name="maxImageSize">
		/// Images with density independent pixel (DIP) sizes larger than |max_image_size| are filtered
		/// out from the image results. Versions of the image at different scale factors may be downloaded
		/// up to the maximum scale factor supported by the system. If there are no image results
		/// &lt; = |max_image_size| then the smallest image is resized to |max_image_size| and is the only
		/// result. A |max_image_size| of 0 means unlimited.
		/// </param>
		/// <param name="bypassCache">
		/// If |bypass_cache| is True then |image_url| is requested from the server even if it is present
		/// in the browser cache.
		/// </param>
		public void DownloadImage(string imageUrl, bool isFavicon, int maxImageSize, bool bypassCache, CefDownloadImageCallback callback)
		{
			if (imageUrl == null)
				throw new ArgumentNullException(nameof(imageUrl));

			AliveBrowserHost.DownloadImage(imageUrl, isFavicon, (uint)maxImageSize, bypassCache, callback);
		}

		/// <summary>
		/// Print the current browser contents.
		/// </summary>
		public void Print()
		{
			AliveBrowserHost.Print();
		}

		/// <summary>
		/// Print the current browser contents to the PDF file and execute |callback| on completion.
		/// </summary>
		/// <param name="path">The PDF file path.</param>
		/// <param name="settings">A PDF print settings.</param>
		public void PrintToPdf(string path, CefPdfPrintSettings settings)
		{
			AliveBrowserHost.PrintToPdf(path, settings, new CefPdfPrintCallbackGlue(ViewGlue));
		}

		public void Find(int identifier, string searchText, bool forward, bool matchCase, bool findNext)
		{
			if (searchText == null)
				throw new ArgumentNullException(nameof(searchText));
			if (searchText.Length == 0)
				throw new ArgumentOutOfRangeException(nameof(searchText));

			AliveBrowserHost.Find(identifier, searchText, forward, matchCase, findNext);
		}

		public void StopFinding(bool clearSelection)
		{
			AliveBrowserHost.StopFinding(clearSelection);
		}

		public void ShowDevTools()
		{
			ShowDevTools(new CefPoint());
		}

		public virtual void ShowDevTools(CefPoint inspectElementAt)
		{
			var windowInfo = new CefWindowInfo();
			windowInfo.SetAsPopup(IntPtr.Zero, "DevTools");
			AliveBrowserHost.ShowDevTools(windowInfo, null, BrowserSettings, inspectElementAt);
			windowInfo.Dispose();
		}

		public void CloseDevTools()
		{
			AliveBrowserHost.CloseDevTools();
		}

		/// <summary>
		/// If a misspelled word is currently selected in an editable node calling this
		/// function will replace it with the specified <paramref name="word"/>.
		/// </summary>
		/// <param name="word">The word to replace.</param>
		public void ReplaceMisspelling(string word)
		{
			if (word == null)
				throw new ArgumentNullException(nameof(word));
			AliveBrowserHost.ReplaceMisspelling(word);
		}

		/// <summary>
		/// Add the specified <paramref name="word"/> to the spelling dictionary.
		/// </summary>
		/// <param name="word">The word to be added to the spelling dictionary.</param>
		public void AddWordToDictionary(string word)
		{
			if (word == null)
				throw new ArgumentNullException(nameof(word));
			AliveBrowserHost.AddWordToDictionary(word);
		}

		/// <summary>
		/// Invalidate the view. The browser will call cef_render_handler_t::OnPaint
		/// asynchronously. This function is only used when window rendering is
		/// disabled.
		/// </summary>
		public void Invalidate(CefPaintElementType type)
		{
			AliveBrowserHost.Invalidate(type);
		}

		/// <summary>
		/// Gets and sets the maximum rate in frames per second (fps) that CefPaint will be occurred
		/// for a windowless browser. The actual fps may be lower if the browser cannot generate
		/// frames at the requested rate. The minimum value is 1 and the maximum value is 60
		/// (default 30).
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int WindowlessFrameRate
		{
			get
			{
				VerifyAccess();
				return AliveBrowserHost.WindowlessFrameRate;
			}
			set
			{
				if (value < 1 || value > 60)
					throw new ArgumentOutOfRangeException(nameof(value));
				WindowlessFrameRate = value;
			}
		}

		/// <summary>
		/// Gets and sets a value indicating whether the browser&apos;s audio is muted.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AudioMuted
		{
			get
			{
				VerifyAccess();
				return AliveBrowserHost.AudioMuted;
			}
			set
			{
				AliveBrowserHost.AudioMuted = value;
			}
		}

		/// <summary>
		/// Gets 
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual KeycodeConverter KeycodeConverter
		{
			get { return KeycodeConverter.Default; }
		}

		//private ScriptableObjectProvider _provider;

		//private ScriptableObjectProvider Provider
		//{
		//	get
		//	{
		//		if (_provider == null && BrowserObject != null)
		//			_provider = new ScriptableObjectProvider(BrowserObject.GetMainFrame());
		//		return _provider;
		//	}
		//}


		//public Window GetWindow()
		//{
		//	return new Window(Provider.GetGlobal(), Provider);
		//}


		/// <summary>
		/// Send a notification to the browser that the screen info has changed.<para/>
		/// This function is only used when window rendering is disabled.
		/// </summary>
		/// <remarks>
		/// The browser will then call <see cref="CefRenderHandler.GetScreenInfo"/>
		/// to update the screen information with the new values. This simulates moving
		/// the webview window from one display to another, or changing the properties
		/// of the current display.
		/// </remarks>
		public void NotifyScreenInfoChanged()
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null || !browserHost.IsWindowRenderingDisabled)
				return;

			browserHost.NotifyScreenInfoChanged();
		}

		/// <summary>
		/// Sends a notification to the browser that the root window has been moved or resized.<para/>
		/// This function is only used when window rendering is disabled.
		/// </summary>
		public void NotifyRootMovedOrResized()
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null || !browserHost.IsWindowRenderingDisabled)
				return;

			browserHost.NotifyMoveOrResizeStarted();
			UpdateOffscreenViewLocation();
			browserHost.NotifyScreenInfoChanged();
		}

		/// <summary>
		/// Loads the document at the specified location into the WebView control.
		/// </summary>
		/// <param name="url">The URL of the document to load.</param>
		public void Navigate(string url)
		{
			AliveBrowserObject.MainFrame.LoadUrl(url);
		}

		void IChromiumWebViewPrivate.RaiseCefBrowserCreated()
		{
			SetState(State.Created, true);
			RaiseCrossThreadEvent(OnBrowserCreated, EventArgs.Empty, true);
		}

		bool IChromiumWebViewPrivate.RaiseClosing()
		{
			if (GetState(State.Closing))
				return false; // don't stop, in the process of disposing

			var ea = new CancelEventArgs();
			RaiseCrossThreadEvent(OnClosing, ea, true);
			if (!ea.Cancel)
			{
				SetState(State.Closing, true);
				return false;
			}
			return true;
		}

		protected virtual void OnClosing(CancelEventArgs e)
		{
			Closing?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseClosed()
		{
			SetState(State.Closed, true);
			RaiseCrossThreadEvent(OnClosed, EventArgs.Empty, false);
		}

		protected virtual void OnClosed(EventArgs e)
		{
			Closed?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseCefCreateWindow(CreateWindowEventArgs e)
		{
			RaiseCrossThreadEvent(OnCreateWindow, e, true);
		}

		protected virtual void OnCreateWindow(CreateWindowEventArgs e)
		{
			CreateWindow?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseBeforeBrowse(BeforeBrowseEventArgs e)
		{
			RaiseCrossThreadEvent(OnBeforeBrowse, e, true);
		}

		protected virtual void OnBeforeBrowse(BeforeBrowseEventArgs e)
		{
			BeforeBrowse?.Invoke(this, e);

			CefFrame frame = e.Frame;
			if (frame != null && frame.IsMain)
			{
				Navigating?.Invoke(this, e);
			}
		}

		void IChromiumWebViewPrivate.RaiseCefPaint(CefPaintEventArgs e)
		{
			OnCefPaint(e);
		}

		void IChromiumWebViewPrivate.RaisePopupShow(PopupShowEventArgs e)
		{
			RaiseCrossThreadEvent(OnPopupShow, e, false);
		}

		void IChromiumWebViewPrivate.RaiseAddressChange(AddressChangeEventArgs e)
		{
			RaiseCrossThreadEvent(OnAddressChange, e, false);
		}

		protected virtual void OnNavigated(NavigatedEventArgs e)
		{
			Navigated?.Invoke(this, e);
		}

		protected virtual void OnAddressChange(AddressChangeEventArgs e)
		{
			if (e.IsMainFrame)
			{
				OnNavigated(e);
			}
			AddressChange?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseTitleChange(DocumentTitleChangedEventArgs e)
		{
			RaiseCrossThreadEvent(OnDocumentTitleChanged, e, false);
		}

		/// <summary>
		/// Raises the <see cref="DocumentTitleChanged"/> event.
		/// </summary>
		/// <param name="e">A <see cref="DocumentTitleChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnDocumentTitleChanged(DocumentTitleChangedEventArgs e)
		{
			DocumentTitleChanged?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseLoadingStateChange(LoadingStateChangeEventArgs e)
		{
			RaiseCrossThreadEvent(OnLoadingStateChange, e, false);
		}

		void IChromiumWebViewPrivate.RaiseTextFound(ITextFoundEventArgs e)
		{
			RaiseCrossThreadEvent(OnTextFound, e, false);
		}

		/// <summary>
		/// Occurs to report find results returned by <see cref="Find"/>.
		/// </summary>
		public event EventHandler<ITextFoundEventArgs> TextFound
		{
			add { AddHandler(in TextFoundEvent, value); }
			remove { RemoveHandler(in TextFoundEvent, value); }
		}

		/// <summary>
		/// Raises the <see cref="TextFound"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ITextFoundEventArgs"/> that contains the event data.</param>
		protected virtual void OnTextFound(ITextFoundEventArgs e)
		{
			TextFoundEvent?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaisePdfPrintFinished(IPdfPrintFinishedEventArgs e)
		{
			RaiseCrossThreadEvent(OnPdfPrintFinished, e, false);
		}

		/// <summary>
		/// Occurs when the PDF printing has completed.
		/// </summary>
		public event EventHandler<IPdfPrintFinishedEventArgs> PdfPrintFinished
		{
			add { AddHandler(in PdfPrintFinishedEvent, value); }
			remove { RemoveHandler(in PdfPrintFinishedEvent, value); }
		}

		/// <summary>
		/// Raises the <see cref="PdfPrintFinished"/> event.
		/// </summary>
		/// <param name="e">A <see cref="IPdfPrintFinishedEventArgs"/> that contains the event data.</param>
		protected virtual void OnPdfPrintFinished(IPdfPrintFinishedEventArgs e)
		{
			PdfPrintFinishedEvent?.Invoke(this, e);
		}


		/// <summary>
		/// Raises the StatusTextChanged event.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected virtual void OnStatusTextChanged(EventArgs e)
		{
			StatusTextChangedEvent?.Invoke(this, e);
		}


		void IChromiumWebViewPrivate.RaiseDevToolsEventAvailable(DevToolsProtocolEventAvailableEventArgs e)
		{
			RaiseCrossThreadEvent(OnDevToolsProtocolEventAvailable, e, false);
		}

		/// <summary>
		/// Raises the <see cref="DevToolsProtocolEventAvailable"/> event.
		/// </summary>
		/// <param name="e">A <see cref="DevToolsProtocolEventAvailableEventArgs"/> that contains the event data.</param>
		protected virtual void OnDevToolsProtocolEventAvailable(DevToolsProtocolEventAvailableEventArgs e)
		{
			DevToolsProtocolEventAvailable?.Invoke(this, e);
		}

		void IChromiumWebViewPrivate.RaiseScriptDialogOpening(IScriptDialogOpeningEventArgs e)
		{
			RaiseCrossThreadEvent(OnScriptDialogOpening, e, true);
		}

		/// <inheritdoc />
		public event EventHandler<IScriptDialogOpeningEventArgs> ScriptDialogOpening
		{
			add { AddHandler(in ScriptDialogOpeningEvent, value); }
			remove { RemoveHandler(in ScriptDialogOpeningEvent, value); }
		}

		/// <summary>
		/// Raises the <see cref="ScriptDialogOpening"/> event.
		/// </summary>
		/// <param name="e">A <see cref="IScriptDialogOpeningEventArgs"/> that contains the event data.</param>
		protected virtual void OnScriptDialogOpening(IScriptDialogOpeningEventArgs e)
		{
			ScriptDialogOpeningEvent?.Invoke(this, e);
		}

		private void InitMouseEvent(int x, int y, CefEventFlags modifiers)
		{
			CefPoint point = PointToViewport(new CefPoint(x, y));
			_mouseEventProxy.X = point.X;
			_mouseEventProxy.Y = point.Y;
			_mouseEventProxy.Modifiers = (uint)modifiers;
		}

		/// <summary>
		/// Sends a mouse move event to the browser.
		/// </summary>
		/// <param name="x">The x-coordinate of the mouse relative to the left edge of the view.</param>
		/// <param name="y">The y-coordinate of the mouse relative to the top edge of the view.</param>
		/// <param name="modifiers">A bitwise combination of the <see cref="CefEventFlags"/> values.</param>
		public void SendMouseMoveEvent(int x, int y, CefEventFlags modifiers)
		{
			InitMouseEvent(x, y, modifiers);
			this.BrowserObject?.Host.SendMouseMoveEvent(_mouseEventProxy, false);
		}

		public void SendMouseLeaveEvent()
		{
			_mouseEventProxy.Modifiers = (uint)CefEventFlags.None;
			this.BrowserObject?.Host.SendMouseMoveEvent(_mouseEventProxy, true);
		}

		/// <summary>
		/// Sends a mouse down event to the browser.
		/// </summary>
		/// <param name="x">The x-coordinate of the mouse relative to the left edge of the view.</param>
		/// <param name="y">The y-coordinate of the mouse relative to the top edge of the view.</param>
		/// <param name="button">One of the <see cref="CefMouseButtonType"/> values.</param>
		/// <param name="clicks">A click count.</param>
		/// <param name="modifiers">A bitwise combination of the <see cref="CefEventFlags"/> values.</param>
		public void SendMouseDownEvent(int x, int y, CefMouseButtonType button, int clicks, CefEventFlags modifiers)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost == null)
				return;

			InitMouseEvent(x, y, modifiers);
			browserHost.SendFocusEvent(true);
			browserHost.SendMouseClickEvent(_mouseEventProxy, button, false, clicks);
		}

		/// <summary>
		/// Sends a mouse up event to the browser.
		/// </summary>
		/// <param name="x">The x-coordinate of the mouse relative to the left edge of the view.</param>
		/// <param name="y">The y-coordinate of the mouse relative to the top edge of the view.</param>
		/// <param name="button">One of the <see cref="CefMouseButtonType"/> values.</param>
		/// <param name="clicks">A click count.</param>
		/// <param name="modifiers">A bitwise combination of the <see cref="CefEventFlags"/> values.</param>
		public void SendMouseUpEvent(int x, int y, CefMouseButtonType button, int clicks, CefEventFlags modifiers)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost == null)
				return;

			InitMouseEvent(x, y, modifiers);
			browserHost.SendFocusEvent(true);
			browserHost.SendMouseClickEvent(_mouseEventProxy, button, true, clicks);
		}

		/// <summary>
		/// Sends a mouse wheel event to the browser.
		/// </summary>
		/// <param name="x">The x-coordinate of the mouse relative to the left edge of the view.</param>
		/// <param name="y">The y-coordinate of the mouse relative to the top edge of the view.</param>
		/// <param name="deltaX">A movement delta in the X direction.</param>
		/// <param name="deltaY">A movement delta in the Y direction.</param>
		public void SendMouseWheelEvent(int x, int y, int deltaX, int deltaY)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost == null)
				return;

			InitMouseEvent(x, y, CefEventFlags.None);
			browserHost.SendMouseWheelEvent(_mouseEventProxy, deltaX, deltaY);
		}

		/// <summary>
		/// Send a touch event to the browser.
		/// </summary>
		/// <param name="eventInfo">The touch event information.</param>
		public void SendTouchEvent(CefTouchEvent eventInfo)
		{
			this.BrowserObject?.Host.SendTouchEvent(eventInfo);
		}

		/// <summary>
		/// Call this function when the user drags the mouse into the web view.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		/// <param name="dragData">
		/// The <paramref name="dragData"/> should not contain file contents as this type of data is not allowed to be
		/// dragged into the web view. File contents can be removed using <see cref="CefDragData.ResetFileContents"/>
		/// (for example, if <paramref name="dragData"/> comes from the <see cref="StartDragging"/> event).
		/// </param>
		public void SendDragEnterEvent(int x, int y, CefEventFlags modifiers, CefDragData dragData, CefDragOperationsMask allowedOps)
		{
			if (dragData is null)
				throw new ArgumentNullException(nameof(dragData));

			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			InitMouseEvent(x, y, modifiers);
			browserHost.DragTargetDragEnter(dragData, _mouseEventProxy, allowedOps);
		}

		/// <summary>
		/// Call this function each time the mouse is moved across the web view during
		/// a drag operation.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		public void SendDragOverEvent(int x, int y, CefEventFlags modifiers, CefDragOperationsMask allowedOps)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			InitMouseEvent(x, y, modifiers);
			browserHost.DragTargetDragOver(_mouseEventProxy, allowedOps);
		}

		/// <summary>
		/// Call this function when the user drags the mouse out of the web view.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		public void SendDragLeaveEvent()
		{
			this.BrowserObject?.Host?.DragTargetDragLeave();
		}

		/// <summary>
		/// Call this function when the user completes the drag operation by dropping
		/// the object onto the web view. The object being dropped is |dragData|, given
		/// as an argument to the previous <see cref="SendDragEnterEvent"/> call.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="modifiers"></param>
		public void SendDragDropEvent(int x, int y, CefEventFlags modifiers)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			InitMouseEvent(x, y, modifiers);
			browserHost.DragTargetDrop(_mouseEventProxy);
		}

		/// <summary>
		/// Inform the web view that the drag operation started by a <see cref="StartDragging"/>
		/// event has ended. If the web view is both the drag source and the drag target then all
		/// Drag* functions should be called before DragSource* methods.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		public void DragSourceSystemDragEnded()
		{
			this.BrowserObject?.Host?.DragSourceSystemDragEnded();
		}

		/// <summary>
		/// Inform the web view that the drag operation started by a CefRenderHandler::StartDragging call
		/// has ended either in a drop or by being cancelled. If the web view is both the drag source and the
		/// drag target then all Drag* functions should be called before DragSource* methods.
		/// <para/>This function is only used when window rendering is disabled.
		/// </summary>
		/// <param name="x">The x-coordinate of the mouse pointer relative to the left edge of the view.</param>
		/// <param name="y">The y-coordinate of the mouse pointer relative to the upper edge of the view.</param>
		/// <param name="effects"></param>
		public void DragSourceEndedAt(int x, int y, CefDragOperationsMask effects)
		{
			CefPoint point = PointToViewport(new CefPoint(x, y));
			this.BrowserObject?.Host?.DragSourceEndedAt(point.X, point.Y, effects);
		}

		/// <summary>
		/// Sends the KeyDown event to the browser.
		/// </summary>
		/// <param name="c">The character associated with the key.</param>
		/// <param name="ctrlKey">The Control key flag.</param>
		/// <param name="altKey">The Alt key flag.</param>
		/// <param name="shiftKey">The Shift key flag.</param>
		/// <param name="metaKey">The Meta key flag.</param>
		/// <param name="repeatCount">The repeat count.</param>
		/// <param name="extendedKey">The extended key flag.</param>
		public void SendKeyDown(char c, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false, int repeatCount = 0, bool extendedKey = false)
		{
			SendKeyChange(CefKeyEventType.RawKeyDown, c, ctrlKey, altKey, shiftKey, metaKey, repeatCount, extendedKey);
		}

		/// <summary>
		/// Sends the KeyDown event to the browser.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="ctrlKey">The Control key flag.</param>
		/// <param name="altKey">The Alt key flag.</param>
		/// <param name="shiftKey">The Shift key flag.</param>
		/// <param name="metaKey">The Meta key flag.</param>
		/// <param name="repeatCount">The repeat count.</param>
		/// <param name="extendedKey">The extended key flag.</param>
		public void SendKeyDown(VirtualKeys key, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false, int repeatCount = 0, bool extendedKey = false)
		{
			SendKeyChange(CefKeyEventType.RawKeyDown, key, ctrlKey, altKey, shiftKey, metaKey, repeatCount, extendedKey);
		}

		/// <summary>
		/// Sends the KeyUp event to the browser.
		/// </summary>
		/// <param name="c">The character associated with the key.</param>
		/// <param name="ctrlKey">The Control key flag.</param>
		/// <param name="altKey">The Alt key flag.</param>
		/// <param name="shiftKey">The Shift key flag.</param>
		/// <param name="metaKey">The Meta key flag.</param>
		/// <param name="extendedKey">The extended key flag.</param>
		public void SendKeyUp(char c, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false, bool extendedKey = false)
		{
			SendKeyChange(CefKeyEventType.KeyUp, c, ctrlKey, altKey, shiftKey, metaKey, 0, extendedKey);
		}

		/// <summary>
		/// Sends the KeyUp event to the browser.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="ctrlKey">The Control key flag.</param>
		/// <param name="altKey">The Alt key flag.</param>
		/// <param name="shiftKey">The Shift key flag.</param>
		/// <param name="metaKey">The Meta key flag.</param>
		/// <param name="extendedKey">The extended key flag.</param>
		public void SendKeyUp(VirtualKeys key, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false, bool extendedKey = false)
		{
			SendKeyChange(CefKeyEventType.KeyUp, key, ctrlKey, altKey, shiftKey, metaKey, 0, extendedKey);
		}

		private void SendKeyChange(CefKeyEventType eventType, char c, bool ctrlKey, bool altKey, bool shiftKey, bool metaKey, int repeatCount, bool extendedKey)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			CefEventFlags modifiers = CefEventFlags.None;
			if (KeycodeConverter.IsShiftRequired(c))
				shiftKey = !shiftKey;
			VirtualKeys key = KeycodeConverter.CharacterToVirtualKey(c);

			if (shiftKey)
				modifiers |= CefEventFlags.ShiftDown;
			if (altKey)
				modifiers |= CefEventFlags.AltDown;
			if (ctrlKey)
				modifiers |= CefEventFlags.ControlDown;
			if (metaKey)
				modifiers |= CefEventFlags.CommandDown;

			var k = new CefKeyEvent();
			k.Type = eventType;
			k.Modifiers = (uint)modifiers;
			k.IsSystemKey = altKey;
			k.WindowsKeyCode = (int)key;
			k.NativeKeyCode = KeycodeConverter.VirtualKeyToNativeKeyCode(key, modifiers, extendedKey);
			k.Character = c;
			k.UnmodifiedCharacter = c;
			this.BrowserObject?.Host?.SendKeyEvent(k);
		}

		private void SendKeyChange(CefKeyEventType eventType, VirtualKeys key, bool ctrlKey, bool altKey, bool shiftKey, bool metaKey, int repeatCount, bool extendedKey)
		{
			if (key < VirtualKeys.None || key > VirtualKeys.OemClear)
				throw new ArgumentOutOfRangeException(nameof(key));

			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			CefEventFlags modifiers = CefEventFlags.None;

			if (shiftKey)
				modifiers |= CefEventFlags.ShiftDown;
			if (altKey)
				modifiers |= CefEventFlags.AltDown;
			if (ctrlKey)
				modifiers |= CefEventFlags.ControlDown;
			if (metaKey)
				modifiers |= CefEventFlags.CommandDown;

			var k = new CefKeyEvent();
			k.Type = eventType;
			k.Modifiers = (uint)modifiers;
			k.IsSystemKey = altKey;
			k.WindowsKeyCode = (int)key;
			k.NativeKeyCode = KeycodeConverter.VirtualKeyToNativeKeyCode(key, modifiers, extendedKey);
			k.Character = (char)key;
			k.UnmodifiedCharacter = (char)key;
			this.BrowserObject?.Host?.SendKeyEvent(k);
		}

		/// <summary>
		/// Sends the KeyPress event to the browser.
		/// </summary>
		/// <param name="c">The character associated with the key.</param>
		/// <param name="ctrlKey">The Control key flag.</param>
		/// <param name="altKey">The Alt key flag.</param>
		/// <param name="shiftKey">The Shift key flag.</param>
		/// <param name="metaKey">The Meta key flag.</param>
		/// <param name="extendedKey">The extended key flag.</param>
		public void SendKeyPress(char c, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false, bool extendedKey = false)
		{
			CefBrowserHost browserHost = this.BrowserObject?.Host;
			if (browserHost is null)
				return;

			CefEventFlags modifiers = CefEventFlags.None;
			if (KeycodeConverter.IsShiftRequired(c))
				shiftKey = !shiftKey;
			if (shiftKey)
				modifiers |= CefEventFlags.ShiftDown;
			if (altKey)
				modifiers |= CefEventFlags.AltDown;
			if (ctrlKey)
				modifiers |= CefEventFlags.ControlDown;
			if (metaKey)
				modifiers |= CefEventFlags.CommandDown;

			VirtualKeys key = KeycodeConverter.CharacterToVirtualKey(c);

			var k = new CefKeyEvent();
			k.Type = CefKeyEventType.Char;
			k.Modifiers = (uint)modifiers;
			k.IsSystemKey = altKey;
			k.WindowsKeyCode = PlatformInfo.IsLinux ? (int)key : c;
			k.NativeKeyCode = KeycodeConverter.VirtualKeyToNativeKeyCode(key, modifiers, extendedKey);
			k.Character = c;
			k.UnmodifiedCharacter = c;
			this.BrowserObject?.Host?.SendKeyEvent(k);
		}

		/// <summary>
		/// Causes the calling thread to yield execution to the specified CEF thread.
		/// </summary>
		/// <param name="threadId">The CEF thread identifier to switch to.</param>
		/// <returns>
		/// An object that is notified when the switch to the CEF thread operation is finished.
		/// </returns>
		protected CefNetSynchronizationContextAwaiter SwitchToCefThread(CefThreadId threadId)
		{
			return CefNetSynchronizationContextAwaiter.GetForThread(threadId);
		}

	}
}
