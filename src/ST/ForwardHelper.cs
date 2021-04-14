using System.Linq;

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
    }
}