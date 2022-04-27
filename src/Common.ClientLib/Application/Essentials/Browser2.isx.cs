using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    partial class Browser2
    {
        public const string Prefix_HTTPS = "https://";

        public const string Prefix_HTTP = "http://";

        public const string Prefix_MSStore = "ms-windows-store://";

        public const string Prefix_Email = "mailto:";

        /// <summary>
        /// 判断字符串是否为 Http Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpsOnly">是否仅Https</param>
        /// <returns></returns>
        public static bool IsHttpUrl([NotNullWhen(true)] string? url, bool httpsOnly = false) => url != null &&
            (url.StartsWith(Prefix_HTTPS, StringComparison.OrdinalIgnoreCase) ||
                  (!httpsOnly && url.StartsWith(Prefix_HTTP, StringComparison.OrdinalIgnoreCase)));

        /// <summary>
        /// 判断字符串是否为 Store Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsStoreUrl([NotNullWhen(true)] string? url) => url != null && url.StartsWith(Prefix_MSStore, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 判断字符串是否为 Email Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsEmailUrl([NotNullWhen(true)] string? url) => url != null && url.StartsWith(Prefix_Email, StringComparison.OrdinalIgnoreCase);
    }
}