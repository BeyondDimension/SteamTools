using CefNet.Avalonia;
using CefNet.JSInterop;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class WebViewExtensions
    {
        public static async Task<string> GetUserAgentAsync(this WebView webView)
        {
            var frame = webView.GetMainFrame();
            dynamic scriptable = await frame
                .GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(true);
            return scriptable.window.navigator.userAgent;
        }
    }
}