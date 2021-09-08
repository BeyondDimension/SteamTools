using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application
{
    public static class Browser2
    {
        public static event Action<Win32Exception>? OnError;

        public static bool HttpsOnly { get; set; }

        public const string Prefix_HTTPS = "https://";

        public const string Prefix_HTTP = "http://";

        public const string Prefix_MSStore = "ms-windows-store://";

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

        static sbyte OpenCore(string? url)
        {
            if (IsStoreUrl(url))
            {
                return OpenCoreByProcess(url);
            }
            else if (IsHttpUrl(url, HttpsOnly))
            {
                if (DeviceInfo.Platform == DevicePlatform.Unknown)
                {
                    return OpenCoreByProcess(url);
                }
                else
                {
                    return 0;
                }
            }
            return -2;

            static sbyte OpenCoreByProcess(string url)
            {
                try
                {
                    Process2.Start(url, useShellExecute: true);
                    return 1;
                }
                catch (Win32Exception e)
                {
                    // [Win32Exception: 找不到应用程序] 39次报告
                    // 疑似缺失没有默认浏览器设置会导致此异常，可能与杀毒软件有关
                    if (OnError == null)
                    {
                        try
                        {
                            Toast.Show(e.GetAllMessage());
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        OnError(e);
                    }
                    return -1;
                }
            }
        }

        /// <summary>
        /// 兼容 Linux/macOS/.Net Core/Android/iOS 的打开链接方法
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task OpenAsync(string? url) => OpenCore(url) switch
        {
            0 => Browser.OpenAsync(url),
            _ => Task.CompletedTask,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static async void Open(string? url) => await OpenAsync(url);

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task OpenAsync(string? url, BrowserLaunchMode launchMode) => OpenCore(url) switch
        {
            0 => Browser.OpenAsync(url, launchMode),
            _ => Task.CompletedTask,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task OpenAsync(string? url, BrowserLaunchOptions options) => OpenCore(url) switch
        {
            0 => Browser.OpenAsync(url, options),
            _ => Task.CompletedTask,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task OpenAsync(Uri uri) => OpenCore(uri.ToString()) switch
        {
            0 => Browser.OpenAsync(uri),
            _ => Task.CompletedTask,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task OpenAsync(Uri uri, BrowserLaunchMode launchMode) => OpenCore(uri.ToString()) switch
        {
            0 => Browser.OpenAsync(uri, launchMode),
            _ => Task.CompletedTask,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => OpenCore(uri.ToString()) switch
        {
            0 => Browser.OpenAsync(uri, options),
            1 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };
    }
}