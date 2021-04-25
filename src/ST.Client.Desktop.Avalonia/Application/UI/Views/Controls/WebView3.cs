using Avalonia.Interactivity;
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
using System.Diagnostics;
using System.Properties;
using static System.Application.Services.CloudService.Constants;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Views.Controls
{
    public sealed class WebView3 : WebView
    {
        public static readonly RoutedEvent<FullscreenModeChangeEventArgs> FullscreenEvent = RoutedEvent.Register<WebView, FullscreenModeChangeEventArgs>("Fullscreen", RoutingStrategies.Bubble);

        public event EventHandler<FullscreenModeChangeEventArgs> Fullscreen
        {
            add { AddHandler(FullscreenEvent, value); }
            remove { RemoveHandler(FullscreenEvent, value); }
        }

        public WebView3()
        {
        }

        public WebView3(WebView opener) : base(opener)
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

        public bool OpenInBrowser { get; set; } = true;
    }

    public class FullscreenModeChangeEventArgs : RoutedEventArgs
    {
        public FullscreenModeChangeEventArgs(IInteractive source, bool fullscreen) : base(WebView3.FullscreenEvent, source)
        {
            Fullscreen = fullscreen;
        }

        public bool Fullscreen { get; }
    }

    internal sealed class WebView3Glue : AvaloniaWebViewGlue
    {
        const int SHOW_DEV_TOOLS = (int)CefMenuId.UserFirst + 0;
        //const int INSPECT_ELEMENT = (int)CefMenuId.UserFirst + 1;

        readonly WebView3 webView;

        public WebView3Glue(WebView3 view) : base(view)
        {
            webView = view;
        }

        private new WebView3 WebView => (WebView3)base.WebView;

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source)
        {
            if (source == CefFocusSource.Navigation)
                return true;
            return false;
        }

        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, CefMenuModel model)
        {
            if (AppHelper.EnableDevtools)
            {
                model.Remove((int)CefMenuId.Print);
                model.Remove((int)CefMenuId.ViewSource);
                if (model.Count >= 1) model.RemoveAt(model.Count - 1);

                model.AddItem(SHOW_DEV_TOOLS, "&Show DevTools");
            }
            else
            {
                model.Clear(); // 禁用右键菜单
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
                        browser.MainFrame.LoadUrl(targetUrl);
                    }
                    return true;
            }
            return base.OnBeforePopup(browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, ref client, settings, ref extraInfo, ref noJavascriptAccess);
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
    }
}