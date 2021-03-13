using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace System.Application
{
    public static class VisualStudioAppCenterSDK
    {
        public static void Init(string appSecret)
        {
            AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
        }
    }
}