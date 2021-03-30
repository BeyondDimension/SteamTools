using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace System.Application
{
    public static class VisualStudioAppCenterSDK
    {
        public static void Init()
        {
            var appSecret = AppSecret;
            if (!string.IsNullOrWhiteSpace(appSecret))
            {
                AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
            }
        }

        static string? AppSecret
        {
            get
            {
                const string namespacePrefix = "System.Application.";
                var r = AppClientAttribute.GetResValue(
                    typeof(VisualStudioAppCenterSDK).Assembly,
                    name: "appcenter-secret",
                    isSingle: false,
                    namespacePrefix);
                return r;
            }
        }
    }
}