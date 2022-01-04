//using System;
//using System.Application.Services;
//using System.Application.Services.CloudService;
//using System.Net;
////using static System.Application.ForwardHelper;

//namespace WinAuth
//{
//    partial class WinAuthSteamClient
//    {
//        [Obsolete("Http Error 403", true)]
//        static bool TryGetForwardUrl(ref string url)
//        {
//            if (IsAllowUrl(url))
//            {
//                url = GetForwardRelativeUrl(url);
//                url = CombineAbsoluteUrl(ICloudServiceClient.Instance.ApiBaseUrl, url);
//                return true;
//            }
//            return false;
//        }

//        [Obsolete("Http Error 403", true)]
//        static void AppendForwardHeaders(WebHeaderCollection headers)
//        {
//            var sc = DI.Get<CloudServiceClientBase>();
//            headers.Add(Constants.Headers.Request.AppVersion, sc.Settings.AppVersionStr);
//        }
//    }
//}