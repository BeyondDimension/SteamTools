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
[BD.Mobius(
"""
Mobius.Helpers.VisualStudioAppCenterSDK
""")]
static partial class VisualStudioAppCenterSDK
{
    [BD.Mobius(
"""
Mobius.Helpers.VisualStudioAppCenterSDK.Init
""")]
    internal static void Init()
    {
        if (DateTime.UtcNow >= new DateTime(2025, 3, 31, default, default, default, DateTimeKind.Utc))
        {
            // Visual Studio App Center is scheduled for retirement on March 31, 2025.
            // https://learn.microsoft.com/en-us/appcenter/retirement
            return;
        }

        var appSecret = AppSecret;
        if (string.IsNullOrWhiteSpace(appSecret))
            return;
#if WINDOWS || LINUX || MACCATALYST || MACOS || APP_REVERSE_PROXY
        var utils = UtilsImpl.Instance;
        AppCenter.SetDeviceInformationHelper(utils);
        AppCenter.SetPlatformHelper(utils);
#pragma warning disable CS0612 // 类型或成员已过时
        AppCenter.SetApplicationSettingsFactory(utils);
#pragma warning restore CS0612 // 类型或成员已过时
#endif
        AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
    }

#if WINDOWS || LINUX || MACCATALYST || MACOS || APP_REVERSE_PROXY
    internal sealed class UtilsImpl :
        Microsoft.AppCenter.Utils.IAbstractDeviceInformationHelper,
        Microsoft.AppCenter.Utils.IPlatformHelper,
        Microsoft.AppCenter.Utils.IApplicationSettingsFactory,
        Microsoft.AppCenter.Utils.IApplicationSettings
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
            if (OperatingSystem.IsLinux())
            {
#if LINUX || APP_REVERSE_PROXY
                var d = IPlatformService.LinuxDistribution;
                if (d.IsDefined())
                    return d.ToString().ToUpperInvariant();
                var osName = IPlatformService.GetLinuxReleaseValue(IPlatformService.LinuxConstants.ReleaseKey_NAME);
                if (!string.IsNullOrWhiteSpace(osName))
                    return osName.ToUpperInvariant();
                osName = IPlatformService.GetLinuxReleaseValue(IPlatformService.LinuxConstants.ReleaseKey_ID);
                if (!string.IsNullOrWhiteSpace(osName))
                    return osName.ToUpperInvariant();
#endif
                return "LINUX";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "MACOS";
            }
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
#if !APP_REVERSE_PROXY
            var result = IApplication.Instance.GetScreenSize();
            return result;
#else
            return default;
#endif
        }

        #endregion

        #region IApplicationSettingsFactory

        public Microsoft.AppCenter.Utils.IApplicationSettings CreateApplicationSettings() => this;

        #endregion

        #region IApplicationSettings

        static readonly object configLock = new();
        readonly ISecureStorage storage = Ioc.Get<ISecureStorage>();

        public bool ContainsKey(string key)
        {
            lock (configLock)
            {
                Func<Task<bool>> func = () => storage.ContainsKeyAsync(key);
                var r = func.RunSync();
                return r;
            }
        }

#pragma warning disable CS8633 // 类型参数的约束中的为 Null 性与隐式实现接口方法中的类型参数的约束不匹配。
#pragma warning disable CS8766 // 返回类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。
        public T? GetValue<T>(string key, T? defaultValue = default) where T : notnull
#pragma warning restore CS8766 // 返回类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。
#pragma warning restore CS8633 // 类型参数的约束中的为 Null 性与隐式实现接口方法中的类型参数的约束不匹配。
        {
            lock (configLock)
            {
                Func<Task<string?>> func = () => storage.GetAsync(key);
                var r = func.RunSync();
                if (r != null)
                {
                    try
                    {
                        var r2 = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(r)!;
                        return r2;
                    }
                    catch
                    {
                    }
                }
            }
            return defaultValue;
        }

        public void Remove(string key)
        {
            lock (configLock)
            {
                Func<Task> func = () => storage.RemoveAsync(key);
                func.RunSync();
            }
        }

        public void SetValue(string key, object value)
        {
            var invariant = value != null ? TypeDescriptor.GetConverter(value.GetType()).ConvertToInvariantString(value) : null;
            lock (configLock)
            {
                Func<Task> func = () => storage.SetAsync(key, invariant);
                func.RunSync();
            }
        }

        #endregion
    }
#endif
}