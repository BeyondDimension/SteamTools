using System;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Net;
using static System.Application.ForwardHelper;

namespace WinAuth
{
    partial class WinAuthSteamClient
    {
        static bool TryGetForwardUrl(ref string url)
        {
            if (IsAllowUrl(url))
            {
                url = GetForwardRelativeUrl(url);
                url = CombineAbsoluteUrl(ICloudServiceClient.Instance.ApiBaseUrl, url);
                return true;
            }
            return false;
        }

        static void AppendForwardHeaders(WebHeaderCollection headers)
        {
            var sc = DI.Get<CloudServiceClientBase>();
            headers.Add(Constants.Headers.Request.AppVersion, sc.Settings.AppVersionStr);
        }
    }
}