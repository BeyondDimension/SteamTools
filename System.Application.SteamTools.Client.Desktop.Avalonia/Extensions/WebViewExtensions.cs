using CefNet.Avalonia;
using CefNet.JSInterop;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class WebViewExtensions
    {
        static string? mUserAgent;

        public static async ValueTask<string> GetUserAgentAsync(this WebView webView)
        {
            if (mUserAgent != null) return mUserAgent;
            try
            {
                var frame = webView.GetMainFrame();
                dynamic scriptable = await frame
                    .GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(true);
                mUserAgent = scriptable.window.navigator.userAgent;
            }
            catch
            {
                return string.Empty;
            }
            return mUserAgent;
        }
    }
}