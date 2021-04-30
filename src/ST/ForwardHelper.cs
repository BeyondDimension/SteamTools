using System.Linq;
using System.Net.Http;
using System.Text;

namespace System.Application
{
    /// <summary>
    /// 转发助手
    /// </summary>
    public static class ForwardHelper
    {
        /// <summary>
        /// 定义一组需要转发的URL
        /// </summary>
        public static readonly string[] allowUrls = new[] {
            "https://steamcommunity.com/",
        };

        public static bool IsAllowUrl(string requestUri)
        {
            return allowUrls.Any(x => requestUri.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetForwardRelativeUrl(string url) => $"api/forward?url={url.Base64UrlEncode()}";

        public static string GetForwardRelativeUrl(Uri url) => GetForwardRelativeUrl(url.ToString());

        public static string GetForwardRelativeUrl(HttpRequestMessage request) => GetForwardRelativeUrl(request.RequestUri.ThrowIsNull(nameof(request.RequestUri)));

        public static string CombineAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (baseUrl.EndsWith('/')) return baseUrl + relativeUrl;
            return $"{baseUrl}/{relativeUrl}";
        }
    }
}