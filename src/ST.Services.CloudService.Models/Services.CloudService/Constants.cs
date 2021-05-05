using System.Application.Properties;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Essentials;

namespace System.Application.Services.CloudService
{
    public static class Constants
    {
        public const string Basic = "Bearer";

        public const string DefaultUserAgent = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/14.14263";

        public static string NetworkConnectionInterruption => SR.NetworkConnectionInterruption;

        public static class Headers
        {
            public static class Request
            {
                public const string AppVersion = "App-Version";
                public const string SecurityKey = "App-SKey";
            }

            public static class Response
            {
                public const string AppObsolete = "App-Obsolete";
            }
        }

        /// <summary>
        /// 短信间隔，60秒
        /// </summary>
        public const int SMSInterval = 60;

        /// <summary>
        /// 实际短信间隔
        /// </summary>
        public const double SMSIntervalActually = 79.5;

        public const string Prefix_HTTPS = "https://";

        public const string Prefix_HTTP = "http://";

        public static string IsNotOfficialChannelPackageWarning => SR.IsNotOfficialChannelPackageWarning;

        /// <summary>
        /// 通用分隔符
        /// </summary>
        public const char GeneralSeparator = ';';

        public static string[] GetSplitValues(string values)
        {
            if (string.IsNullOrWhiteSpace(values)) return Array.Empty<string>();
            return values.Split(GeneralSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] GetSplitValues(object @lock, string values, ref string? cacheValues, ref string[]? cacheValuesArray)
        {
            lock (@lock)
            {
                if (cacheValuesArray == null || cacheValues == null || cacheValues != values)
                {
                    cacheValues = values;
                    cacheValuesArray = GetSplitValues(values);
                }
            }
            return cacheValuesArray;
        }

        /// <summary>
        /// 判断字符串是否为 Http Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpsOnly">是否仅Https</param>
        /// <returns></returns>
        public static bool IsHttpUrl(string? url, bool httpsOnly = false) => url != null &&
            (url.StartsWith(Prefix_HTTPS, StringComparison.OrdinalIgnoreCase) ||
                  (!httpsOnly && url.StartsWith(Prefix_HTTP, StringComparison.OrdinalIgnoreCase)));

        /// <summary>
        /// 兼容 Linux/Mac/.NetCore/Android/iOS 的打开链接方法
        /// </summary>
        /// <param name="url"></param>
        public static async void BrowserOpen(string? url)
        {
            if (IsHttpUrl(url))
            {
                if (DI.DeviceIdiom == DeviceIdiom.Desktop && DI.Platform != Platform.UWP)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true,
                        });
                    }
                    catch (Win32Exception e)
                    {
#if MVVM_VM
                        // [Win32Exception: 找不到应用程序] 39次报告
                        // 疑似缺失没有默认浏览器设置会导致此异常，可能与杀毒软件有关
                        Toast.Show(e.GetAllMessage());
#endif
                    }
                }
                else
                {
                    await Browser.OpenAsync(url);
                }
            }
        }
    }
}