using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

/// <summary>
/// Visual Studio App Center
/// <list type="bullet">
/// <item>将移动开发人员常用的多种服务整合到一个集成的产品中。</item>
/// <item>您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。</item>
/// <item>https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin</item>
/// <item>https://visualstudio.microsoft.com/zh-hans/app-center</item>
/// </list>
/// </summary>
static partial class VisualStudioAppCenterSDK
{
    internal static void Init()
    {
        var appSecret = AppSecret;
        if (string.IsNullOrWhiteSpace(appSecret))
            return;
#if WINDOWS || LINUX || APP_REVERSE_PROXY
        var utils = UtilsImpl.Instance;
        AppCenter.SetDeviceInformationHelper(utils);
        AppCenter.SetPlatformHelper(utils);
#endif
        AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
    }

#if WINDOWS || LINUX || APP_REVERSE_PROXY
    internal sealed class UtilsImpl : Microsoft.AppCenter.Utils.IAbstractDeviceInformationHelper, Microsoft.AppCenter.Utils.IPlatformHelper
    {
        private UtilsImpl() { }

        public static UtilsImpl Instance { get; } = new();

        #region IPlatformHelper

        public string? ProductVersion => AssemblyInfo.InformationalVersion;

#if !APP_REVERSE_PROXY
        public bool? IsAnyWindowNotMinimized()
        {
            var result = IApplication.Instance.IsAnyWindowNotMinimized();
            return result;
        }

        public bool IsSupportedUnhandledException => true;
#endif

        public Action<object?, Exception>? InvokeUnhandledExceptionOccurred { get; set; }

        public bool IsSupportedApplicationExit => true;

        public event EventHandler? ApplicationExit;

        internal void OnExit(object? sender, EventArgs e)
        {
            ApplicationExit?.Invoke(sender, e);
        }

        #endregion

        #region IAbstractDeviceInformationHelper

        public string? GetDeviceModel()
        {
            return null;
        }

        public string? GetDeviceOemName()
        {
            return null;
        }

        public string? GetOsName()
        {
#if !WINDOWS
            if (OperatingSystem.IsLinux())
            {
                var d = IPlatformService.LinuxDistribution;
                if (d.IsDefined())
                    return d.ToString().ToUpperInvariant();
                var osName = IPlatformService.GetLinuxReleaseValue(IPlatformService.LinuxConstants.ReleaseKey_NAME);
                if (!string.IsNullOrWhiteSpace(osName))
                    return osName.ToUpperInvariant();
                osName = IPlatformService.GetLinuxReleaseValue(IPlatformService.LinuxConstants.ReleaseKey_ID);
                if (!string.IsNullOrWhiteSpace(osName))
                    return osName.ToUpperInvariant();
                return "LINUX";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "MACOS";
            }
#endif
            return null;
        }

        public string? GetOsBuild()
        {
            if (!OperatingSystem.IsWindows())
            {
                return Environment.OSVersion.Version.ToString();
            }
            return null;
        }

        public string? GetOsVersion()
        {
            if (!OperatingSystem.IsWindows())
            {
                return Environment.OSVersion.Version.ToString();
            }
            return null;
        }

        public string? GetAppVersion()
        {
            return AssemblyInfo.InformationalVersion;
        }

        public string? GetAppBuild()
        {
            return AssemblyInfo.FileVersion;
        }

        public System.Drawing.Size? GetScreenSize()
        {
            var result = IApplication.Instance.GetScreenSize();
            return result;
        }

        #endregion
    }
#endif
}