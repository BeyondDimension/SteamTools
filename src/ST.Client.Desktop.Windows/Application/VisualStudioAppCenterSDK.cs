using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Runtime.Versioning;

namespace System.Application
{
    public static class VisualStudioAppCenterSDK
    {
#if !__MOBILE__
        [SupportedOSPlatform("Windows")]
#endif
        public static void Init(string appSecret)
        {
            AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
        }
    }
}