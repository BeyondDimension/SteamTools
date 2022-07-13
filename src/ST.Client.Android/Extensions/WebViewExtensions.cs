using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.WebKit;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

public static class WebViewExtensions
{
    /// <summary>
    /// 使用此函数代替 <see cref="WebView.Destroy"/>
    /// </summary>
    /// <param name="webView"></param>
    public static void DestroyAndRemove(this WebView? webView)
    {
        if (webView != null)
        {
            webView.StopLoading(); // 停止加载
            webView.Settings.JavaScriptEnabled = false; // 禁用JS
            webView.ClearHistory(); //清除历史记录
            webView.RemoveAllViews();
            if (webView.Parent is ViewGroup viewGroup) viewGroup.RemoveView(webView); // 移除WebView
            webView.Destroy(); // 销毁VebView
        }
    }

    /// <summary>
    /// 在 <see cref="WebView"/> 初始化中优化
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="isHtmlOnly">是否仅加载HTML内容</param>
    public static void InitOptimize(this WebView webView, bool isHtmlOnly = true)
    {
        // https://developer.android.google.cn/guide/webapps/managing-webview?hl=zh-cn
        webView.SetWebViewClient(new WebViewClientCompat());
        webView.Settings.SetRenderPriority(WebSettings.RenderPriority.High);
        webView.Settings.BuiltInZoomControls = false;
        webView.Settings.SetSupportZoom(false);
        webView.Settings.DisplayZoomControls = false;
        webView.Settings.BlockNetworkImage = isHtmlOnly;
        webView.Settings.JavaScriptEnabled = !isHtmlOnly;
        if (Build.VERSION.SdkInt < BuildVersionCodes.M)
        {
#pragma warning disable CS0618 // 类型或成员已过时
            webView.SetHorizontalScrollbarOverlay(false);
            webView.SetVerticalScrollbarOverlay(false);
#pragma warning restore CS0618 // 类型或成员已过时
        }
    }

    /// <summary>
    /// 加载 HTML 字符串
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="htmlString"></param>
    /// <param name="encoding"></param>
    public static void LoadHtmlString(this WebView webView, string htmlString, Encoding? encoding)
    {
        webView.LoadDataWithBaseURL(baseUrl: null,
            data: htmlString,
            mimeType: MediaTypeNames.HTML,
            encoding: (encoding ?? Encoding.UTF8).WebName,
            historyUrl: null);
    }
}
