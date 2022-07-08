using Avalonia.Controls;
using Avalonia.Platform;
//using Microsoft.Web.WebView2.WinForms;
using System;

namespace System.Application.UI.Views.Controls
{
    internal class EdgeWebView : NativeControlHost
    {
        //private WebView2? webView;

        //protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        //{
        //    if (OperatingSystem.IsWindows())
        //    {
        //        webView = new WebView2
        //        {
        //            Source = new Uri("https://www.baidu.com/"),
        //        };
        //        return new PlatformHandle(webView.Handle, "HWND");
        //    }

        //    return base.CreateNativeControlCore(parent);
        //}

        //protected override void DestroyNativeControlCore(IPlatformHandle control)
        //{
        //    if (OperatingSystem.IsWindows())
        //    {
        //        webView?.Dispose();
        //        webView = null;
        //    }

        //    base.DestroyNativeControlCore(control);
        //}
    }
}
