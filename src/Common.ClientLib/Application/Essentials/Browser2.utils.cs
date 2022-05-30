using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System.Application;

partial class Browser2
{
    enum OpenResultCode : sbyte
    {
        /// <summary>
        /// 值为 Http URL
        /// </summary>
        HttpUrl = 0,

        /// <summary>
        /// 已由 Process2 启动
        /// </summary>
        StartedByProcess2 = 1,

        /// <summary>
        /// 出现异常，已由 <see cref="OnError"/> 或 <see cref="Toast"/> 处理
        /// </summary>
        Exception = -1,

        /// <summary>
        /// 值格式不正确或未知
        /// </summary>
        Unknown = -2,
    }

    const string TAG = nameof(Browser2);

    static void HandlerException(Exception e)
    {
        if (OnError == null)
        {
            try
            {
                e.LogAndShowT(TAG);
            }
            catch
            {
            }
        }
        else
        {
            OnError(e);
        }
    }

    static OpenResultCode OpenAnalysis(string? url)
    {
        if (IsStoreUrl(url) || IsEmailUrl(url))
        {
            return OpenCoreByProcess(url);
        }
        else if (IsHttpUrl(url, HttpsOnly))
        {
            if (!Essentials.IsSupported)
            {
                return OpenCoreByProcess(url);
            }
            else
            {
                return OpenResultCode.HttpUrl;
            }
        }
        return OpenResultCode.Unknown;

        static OpenResultCode OpenCoreByProcess(string url)
        {
            var r = Process2.OpenCoreByProcess(url, s => Toast.Show(s));
            return r ? OpenResultCode.StartedByProcess2 : OpenResultCode.Exception;
        }
    }

    static async Task<bool> OpenCoreAsync(Uri uri, BrowserLaunchMode launchMode = DefaultBrowserLaunchMode)
    {
        try
        {
            await IBrowserPlatformService.Instance.OpenAsync(uri, launchMode);
            return true;
        }
        catch (Exception e)
        {
            HandlerException(e);
            return false;
        }
    }

    static async Task<bool> OpenCoreAsync(string uri, BrowserLaunchMode launchMode = DefaultBrowserLaunchMode)
    {
        try
        {
            await IBrowserPlatformService.Instance.OpenAsync(uri, launchMode);
            return true;
        }
        catch (Exception e)
        {
            HandlerException(e);
            return false;
        }
    }

    static async Task<bool> OpenCoreAsync(Uri uri, BrowserLaunchOptions options)
    {
        try
        {
            return await IBrowserPlatformService.Instance.OpenAsync(uri, options);
        }
        catch (Exception e)
        {
            HandlerException(e);
            return false;
        }
    }

    static async Task<bool> OpenCoreAsync(string uri, BrowserLaunchOptions options)
    {
        try
        {
            await IBrowserPlatformService.Instance.OpenAsync(uri, options);
            return true;
        }
        catch (Exception e)
        {
            HandlerException(e);
            return false;
        }
    }

    static bool OpenCore(string uri, BrowserLaunchMode launchMode = DefaultBrowserLaunchMode)
    {
        OpenCoreSync(uri, launchMode);
        return true;

        static async void OpenCoreSync(string uri, BrowserLaunchMode launchMode) => await OpenCoreAsync(uri, launchMode);
    }
}
