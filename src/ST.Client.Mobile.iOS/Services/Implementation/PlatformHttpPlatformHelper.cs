using CoreGraphics;
using Foundation;
using System.Net.Http;
using System.Threading.Tasks;
using WebKit;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformHttpPlatformHelper : HttpPlatformHelper
    {
        static readonly Lazy<string?> mUserAgent = new(() =>
        {
            var userAgent = GetUserAgentByWKWebView();
            //var userAgent = UIKit.UIDevice.CurrentDevice.CheckSystemVersion(12, 0) ?
            //       GetUserAgentByWKWebView() :
            //       GetUserAgentByUIWebView();
            return userAgent;
        });

        const string GetUserAgentJavaScriptCode = "navigator.userAgent";

        //static string GetUserAgentByUIWebView()
        //{
        //    using var webView = new UIKit.UIWebView();
        //    return webView.EvaluateJavascript(GetUserAgentJavaScriptCode);
        //}

        static string GetUserAgentByWKWebView()
        {
            using var webView = new WKWebView(CGRect.Empty, new WKWebViewConfiguration());
            Func<Task<NSObject>> func = ()
                => webView.EvaluateJavaScriptAsync(new NSString(GetUserAgentJavaScriptCode));
            var result = func.RunSync();
            return result.ToString();
        }

        public override string UserAgent => mUserAgent.Value ?? DefaultUserAgent;
    }
}