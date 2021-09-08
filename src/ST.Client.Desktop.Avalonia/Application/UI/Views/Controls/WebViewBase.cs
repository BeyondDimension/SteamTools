using Avalonia.Interactivity;
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
using System.Application.Models;
using System.Application.Services.CloudService;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Views.Controls
{
    public class WebViewBase : WebView
    {
        public static readonly RoutedEvent<FullscreenModeChangeEventArgs> FullscreenEvent = RoutedEvent.Register<WebView, FullscreenModeChangeEventArgs>("Fullscreen", RoutingStrategies.Bubble);

        public event EventHandler<FullscreenModeChangeEventArgs> Fullscreen
        {
            add { AddHandler(FullscreenEvent, value); }
            remove { RemoveHandler(FullscreenEvent, value); }
        }

        public WebViewBase()
        {
        }

        public WebViewBase(WebView opener) : base(opener)
        {
        }

        protected override WebViewGlue CreateWebViewGlue()
        {
            return new WebView3Glue(this);
        }

        internal void RaiseFullscreenModeChange(bool fullscreen)
        {
            RaiseCrossThreadEvent(OnFullScreenModeChange, new FullscreenModeChangeEventArgs(this, fullscreen), false);
        }

        private void OnFullScreenModeChange(FullscreenModeChangeEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// 在浏览器中打开新窗口，所有新页面打开的Url会在浏览器中打开
        /// </summary>
        public bool OpenInBrowser { get; set; } = true;

        /// <summary>
        /// 是否固定单页，为 <see langword="true"/> 时所有跳转都将在浏览器中打开
        /// </summary>
        public bool FixedSinglePage { get; set; } = false;

        /// <summary>
        /// 需要拦截响应流的Url
        /// </summary>
        public string[]? StreamResponseFilterUrls { get; set; }

        /// <summary>
        /// 拦截响应流的处理
        /// </summary>
        public Action<string, Stream>? OnStreamResponseFilterResourceLoadComplete { get; set; }

        public bool IsSecurity { get; set; }

        public Aes? Aes { get; internal set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Aes?.Dispose();
                StreamResponseFilterUrls = null;
                OnStreamResponseFilterResourceLoadComplete = null;
            }
            base.Dispose(disposing);
        }
    }

    public class FullscreenModeChangeEventArgs : RoutedEventArgs
    {
        public FullscreenModeChangeEventArgs(IInteractive source, bool fullscreen) : base(WebViewBase.FullscreenEvent, source)
        {
            Fullscreen = fullscreen;
        }

        public bool Fullscreen { get; }
    }

    internal sealed class WebView3Glue : AvaloniaWebViewGlue
    {
        const int SHOW_DEV_TOOLS = (int)CefMenuId.UserFirst + 0;
        //const int INSPECT_ELEMENT = (int)CefMenuId.UserFirst + 1;

        readonly WebViewBase webView;

        public WebView3Glue(WebViewBase view) : base(view)
        {
            webView = view;
        }

        private new WebViewBase WebView => (WebViewBase)base.WebView;

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source)
        {
            if (source == CefFocusSource.Navigation)
                return true;
            return false;
        }

        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model)
        {
            var count = model.Count;
            if (count > 0)
            {
                List<int> list = new();
                for (int i = 0; i < count; i++)
                {
                    var commandId = (CefMenuId)model.GetCommandIdAt(i);
                    switch (commandId)
                    {
                        case CefMenuId.Undo:
                        case CefMenuId.Redo:
                        case CefMenuId.Cut:
                        case CefMenuId.Copy:
                        case CefMenuId.Paste:
                        case CefMenuId.Delete:
                        case CefMenuId.SelectAll:
                        case CefMenuId.Find:
                            break;
                        default:
                            list.Add((int)commandId);
                            break;
                    }
                }
                list.ForEach(x => model.Remove(x));
            }
            if (AppHelper.EnableDevtools)
            {
                model.AddItem(SHOW_DEV_TOOLS, "&Show DevTools");
            }
            //model.InsertItemAt(model.Count > 0 ? 1 : 0, (int)CefMenuId.ReloadNocache, "Refresh");
            //model.AddSeparator();
            //model.AddItem(SHOW_DEV_TOOLS, "&Show DevTools");
            //model.AddItem(INSPECT_ELEMENT, "Inspect element");

            //CefMenuModel submenu = model.AddSubMenu(0, "Submenu Test");
            //submenu.AddItem((int)CefMenuId.Copy, "Copy");
            //submenu.AddItem((int)CefMenuId.Paste, "Paste");
            //submenu.SetColorAt(submenu.Count - 1, CefMenuColorType.Text, CefColor.FromArgb((int)Colors.Blue.ToUint32()));
            //submenu.AddCheckItem(0, "Checked Test");
            //submenu.SetCheckedAt(submenu.Count - 1, true);
            //submenu.AddRadioItem(0, "Radio Off", 0);
            //submenu.AddRadioItem(0, "Radio On", 1);
            //submenu.SetCheckedAt(submenu.Count - 1, true);
        }

        protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, int commandId, CefEventFlags eventFlags)
        {
            if (commandId >= (int)CefMenuId.UserFirst && commandId <= (int)CefMenuId.UserLast)
            {
                switch (commandId)
                {
                    case SHOW_DEV_TOOLS:
                        WebView.ShowDevTools();
                        break;
                        //case INSPECT_ELEMENT:
                        //    WebView.ShowDevTools(new CefPoint(menuParams.XCoord, menuParams.YCoord));
                        //    break;
                }
                return true;
            }
            return false;
        }

        protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen)
        {
            WebView.RaiseFullscreenModeChange(fullscreen);
        }

        protected override bool OnConsoleMessage(CefBrowser browser, CefLogSeverity level, string message, string source, int line)
        {
            Debug.Print("[{0}]: {1} ({2}, line: {3})", level, message, source, line);
            return false;
        }

        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref int noJavascriptAccess)
        {
            if (webView.FixedSinglePage)
            {
                BrowserOpen(targetUrl);
                return true;
            }
            switch (targetDisposition)
            {
                case CefWindowOpenDisposition.NewForegroundTab:
                case CefWindowOpenDisposition.NewBackgroundTab:
                case CefWindowOpenDisposition.NewPopup:
                case CefWindowOpenDisposition.NewWindow:
                    if (webView.OpenInBrowser)
                    {
                        BrowserOpen(targetUrl);
                    }
                    else
                    {
                        // 禁止创建新窗口，仅在当前窗口跳转
                        if (IsHttpUrl(targetUrl))
                        {
                            browser.MainFrame.LoadUrl(targetUrl);
                        }
                    }
                    return true;
            }
            return base.OnBeforePopup(browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, ref client, settings, ref extraInfo, ref noJavascriptAccess);
        }

        protected override CefReturnValue OnBeforeResourceLoad(CefBrowser browser, CefFrame frame, CefRequest request, CefRequestCallback callback)
        {
            var sc = DI.Get<CloudServiceClientBase>();
            if (request.Url.StartsWith(sc.ApiBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
                request.SetHeaderByName(Headers.Request.AppVersion, sc.Settings.AppVersionStr, true);
                if (webView.IsSecurity)
                {
                    if (webView.Aes == null)
                    {
                        webView.Aes = AESUtils.Create();
                    }
                    var skey_bytes = webView.Aes.ToParamsByteArray();
                    var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
                    request.SetHeaderByName(Headers.Request.SecurityKey, skey_str, true);
                }
                Func<Task<JWTEntity?>> getAuthTokenAsync = () => conn_helper.Auth.GetAuthTokenAsync().AsTask();
                var authToken = getAuthTokenAsync.RunSync();
                var authHeaderValue = conn_helper.GetAuthenticationHeaderValue(authToken);
                if (authHeaderValue != null)
                {
                    var authHeaderValueStr = authHeaderValue.ToString();
                    request.SetHeaderByName("Authorization", authHeaderValueStr, true);
                }
            }
            var returnValue = base.OnBeforeResourceLoad(browser, frame, request, callback);
            return returnValue;
        }

        //static readonly string[] urls = new[]
        //{
        //    "https://localhost",
        //};

        //protected override CefReturnValue OnBeforeResourceLoad(CefBrowser browser, CefFrame frame, CefRequest request, CefRequestCallback callback)
        //{
        //    //using var header = new CefStringMultimap();
        //    //request.GetHeaderMap(header);
        //    //var header_new = new CefStringMultimap();
        //    //foreach (var item in header.AllKeys)
        //    //{
        //    //    string value;
        //    //    if ("Accept-Language".Equals(item, StringComparison.OrdinalIgnoreCase))
        //    //    {
        //    //        value = R.GetAcceptLanguage();
        //    //    }
        //    //    else
        //    //    {
        //    //        value = header[item];
        //    //    }
        //    //    header_new.Add(item, value);
        //    //}
        //    //request.SetHeaderMap(header_new);
        //    //if (urls.Any(x => request.Url.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        //    //{
        //    //    request.SetHeaderByName("Accept-Language2", R.AcceptLanguage, true);
        //    //}
        //    var returnValue = base.OnBeforeResourceLoad(browser, frame, request, callback);
        //    return returnValue;
        //}

        protected override void OnResourceLoadComplete(CefBrowser browser, CefFrame frame, CefRequest request, CefResponse response, CefUrlRequestStatus status, long receivedContentLength)
        {
            base.OnResourceLoadComplete(browser, frame, request, response, status, receivedContentLength);
            if (responseDictionary.TryGetValue(request.Identifier, out var filter))
            {
                if (webView.OnStreamResponseFilterResourceLoadComplete != null)
                {
                    Task.Run(() =>
                    {
                        webView.OnStreamResponseFilterResourceLoadComplete(request.Url, filter.Data);
                    });
                }
            }
        }

        readonly Dictionary<ulong, StreamResponseFilter> responseDictionary = new();

        protected override CefResponseFilter GetResourceResponseFilter(CefBrowser browser, CefFrame frame, CefRequest request, CefResponse response)
        {
            if (webView.StreamResponseFilterUrls != null &&
                webView.StreamResponseFilterUrls.Any(x => request.Url.StartsWith(x, StringComparison.OrdinalIgnoreCase)) &&
                webView.OnStreamResponseFilterResourceLoadComplete != null)
            {
                var rspIsCiphertext = false;
                if (webView.IsSecurity)
                {
                    var contentType = response.GetHeaderByName("Content-Type");
                    if (!string.IsNullOrWhiteSpace(contentType) && MediaTypeHeaderValue.TryParse(contentType, out var contentType_))
                    {
                        var mime = contentType_.MediaType;
                        if (mime == MediaTypeNames.Security)
                        {
                            rspIsCiphertext = true;
                        }
                    }
                }
                var dataFilter = new StreamResponseFilter(webView, rspIsCiphertext);
                responseDictionary.Add(request.Identifier, dataFilter);
                return dataFilter;
            }
            return base.GetResourceResponseFilter(browser, frame, request, response);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/45816851/using-cefsharp-to-capture-resource-response-data-body
        /// </summary>
        unsafe class StreamResponseFilter : CefResponseFilter
        {
            readonly WebViewBase webView;
            readonly bool rspIsCiphertext;
            MemoryStream? memoryStream;

            public StreamResponseFilter(WebViewBase webView, bool rspIsCiphertext)
            {
                this.webView = webView;
                this.rspIsCiphertext = rspIsCiphertext;
            }

            protected override bool InitFilter()
            {
                memoryStream = new();
                return true;
            }

            protected override CefResponseFilterStatus Filter(IntPtr dataIn, long dataInSize, ref long dataInRead, IntPtr dataOut, long dataOutSize, ref long dataOutWritten)
            {
                Stream? dataInStream;
                if (dataIn == default)
                {
                    dataInStream = default;
                }
                else
                {
                    var dataInBytePtr = (byte*)dataIn.ToPointer();
                    dataInStream = new UnmanagedMemoryStream(dataInBytePtr, dataInSize, dataInSize, FileAccess.Read);
                }

                var dataOutBytePtr = (byte*)dataOut.ToPointer();
                UnmanagedMemoryStream dataOutStream = new(dataOutBytePtr, dataOutSize, dataOutSize, FileAccess.Write);

                return Filter(dataInStream, dataInSize, ref dataInRead, dataOutStream, dataOutSize, ref dataOutWritten);
            }

            protected virtual CefResponseFilterStatus Filter(Stream? dataIn, long dataInSize, ref long dataInRead, Stream dataOut, long dataOutSize, ref long dataOutWritten)
            {
                if (dataIn == null)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return CefResponseFilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                //Important we copy dataIn to dataOut
                dataIn.CopyTo(dataOut);

                //Copy data to stream
                dataIn.Position = 0;
                dataIn.CopyTo(memoryStream!);

                return CefResponseFilterStatus.Done;
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    if (disposing)
                    {
                        if (memoryStream != null)
                        {
                            memoryStream?.Dispose();
                            memoryStream = null;
                            stream?.Dispose();
                            stream = null;
                        }
                    }
                }
                base.Dispose(disposing);
            }

            Stream? stream;

            public Stream Data
            {
                get
                {
                    if (stream == null) stream = GetStream();
                    return stream;
                }
            }

            Stream GetStream()
            {
                if (rspIsCiphertext && webView.Aes != null)
                {
                    memoryStream!.Position = 0;
                    var cryptoStream = new CryptoStream(memoryStream, webView.Aes.CreateDecryptor(), CryptoStreamMode.Read);
                    return cryptoStream;
                }
                return memoryStream!;
            }
        }
    }
}