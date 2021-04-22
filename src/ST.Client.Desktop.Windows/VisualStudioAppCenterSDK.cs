using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.IO;
using static System.Application.AppClientAttribute;

namespace System.Application
{
    public static class VisualStudioAppCenterSDK
    {
        /// <summary>
        /// Visual Studio App Center
        /// <list type="bullet">
        /// <item>将移动开发人员常用的多种服务整合到一个集成的产品中。</item>
        /// <item>您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。</item>
        /// <item>https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin</item>
        /// <item>https://visualstudio.microsoft.com/zh-hans/app-center</item>
        /// </list>
        /// </summary>
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
                var assembly = typeof(VisualStudioAppCenterSDK).Assembly;
                const string namespacePrefix = "System.Application.Resources.";
                Stream? func(string x) => assembly.GetManifestResourceStream(x);
                var r = GetResValue(func,
#if XAMARIN_MAC || MONO_MAC || MAC
                    "appcenter-secret-mac",
#elif __ANDROID__
                    "appcenter-secret-android",
#elif __IOS__
                    "appcenter-secret-ios",
#else
                    "appcenter-secret",
#endif
                    isSingle: false,
                    namespacePrefix,
                    ResValueFormat.StringGuidD);
                return r;
            }
        }
    }
}