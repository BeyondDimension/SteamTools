using Avalonia.Interactivity;
using Avalonia.Media;
using CefNet;
using CefNet.Avalonia;
using CefNet.Internal;
using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    public sealed class WebView3 : WebView
    {
        public static RoutedEvent<FullscreenModeChangeEventArgs> FullscreenEvent = RoutedEvent.Register<WebView, FullscreenModeChangeEventArgs>("Fullscreen", RoutingStrategies.Bubble);

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
        //private const int SHOW_DEV_TOOLS = (int)CefMenuId.UserFirst + 0;
        //private const int INSPECT_ELEMENT = (int)CefMenuId.UserFirst + 1;

        public WebView3Glue(WebView3 view) : base(view)
        {
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
            model.Clear(); // 禁用右键菜单

            //model.Remove((int)CefMenuId.Print);
            //model.Remove((int)CefMenuId.ViewSource);
            //model.RemoveAt(model.Count - 1);

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

        //protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams menuParams, int commandId, CefEventFlags eventFlags)
        //{
        //    if (commandId >= (int)CefMenuId.UserFirst && commandId <= (int)CefMenuId.UserLast)
        //    {
        //        switch (commandId)
        //        {
        //            case SHOW_DEV_TOOLS:
        //                WebView.ShowDevTools();
        //                break;
        //            case INSPECT_ELEMENT:
        //                WebView.ShowDevTools(new CefPoint(menuParams.XCoord, menuParams.YCoord));
        //                break;
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen)
        {
            WebView.RaiseFullscreenModeChange(fullscreen);
        }

        protected override bool OnConsoleMessage(CefBrowser browser, CefLogSeverity level, string message, string source, int line)
        {
            Debug.Print("[{0}]: {1} ({2}, line: {3})", level, message, source, line);
            return false;
        }
    }
}