#if __ANDROID__
using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Util;
using Android.Opengl;
using XEPlatform = Xamarin.Essentials.Platform;
using Process = System.Diagnostics.Process;
using AndroidApplication = Android.App.Application;
#endif
using Microsoft.Extensions.DependencyInjection;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Properties;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System.Application.UI
{
    static class AboutAppInfoPopup
    {
        static readonly IPlatformService platformService = IPlatformService.Instance;
        static int show_runtime_info_counter;
        static DateTime show_runtime_info_last_click_time;
        const int show_runtime_info_counter_max = 5;
        const double show_runtime_info_click_effective_interval = 1.5;
        const string os_ver =
#if __ANDROID__
            "[os.ver] Android ";
#else
            "[os.ver] ";
#endif

        public static string GetInfoString()
        {
            StringBuilder b = new(os_ver);
#if __ANDROID__
            var activity = XEPlatform.CurrentActivity;
            var sdkInt = Build.VERSION.SdkInt;
            b.AppendFormat("{0}(API {1})", sdkInt, (int)sdkInt);
#else
            if (OperatingSystem2.IsWindows())
            {
#pragma warning disable CA1416 // 验证平台兼容性
                var productName = platformService.WindowsProductName;
                var major = Environment.OSVersion.Version.Major;
                var minor = Environment.OSVersion.Version.Minor;
                var build = Environment.OSVersion.Version.Build;
                var revision = platformService.WindowsVersionRevision;
                b.AppendFormat("{0} {1}.{2}.{3}.{4}", productName, major, minor, build, revision);
                var servicePack = Environment.OSVersion.ServicePack;
                if (!string.IsNullOrEmpty(servicePack))
                {
                    b.Append(' ');
                    b.Append(servicePack);
                }
                var releaseId = platformService.WindowsReleaseIdOrDisplayVersion;
#pragma warning restore CA1416 // 验证平台兼容性
                if (!string.IsNullOrWhiteSpace(releaseId))
                {
                    b.Append(" (");
                    b.Append(releaseId);
                    b.Append(')');
                }
            }
            else if (OperatingSystem2.IsLinux())
            {
#pragma warning disable CA1416 // 验证平台兼容性
                var linuxIssue = platformService.LinuxIssue;
                if (!string.IsNullOrWhiteSpace(linuxIssue))
                {
                    b.Append(linuxIssue);
                }
                else
                {
                    AppendOSName();
                }
#pragma warning restore CA1416 // 验证平台兼容性
            }
            else
            {
                AppendOSName();
            }
            void AppendOSName() => b.AppendFormat("{0} {1}", DeviceInfo2.OSName(), Environment.OSVersion.Version);
#endif
            b.AppendLine();

            b.Append("[os.name] ");
            b.Append(DeviceInfo2.OSNameValue());
            b.AppendLine();

            b.Append("[app.ver] ");
#if __ANDROID__
            GetAppDisplayVersion(activity, b);
            static void GetAppDisplayVersion(Context context, StringBuilder b)
            {
                var info = context.PackageManager!.GetPackageInfo(context.PackageName!, default);
                if (info == default) return;
#pragma warning disable CS0618 // 类型或成员已过时
                b.AppendFormat("{0}({1})", info.VersionName, Build.VERSION.SdkInt >= BuildVersionCodes.P ? info.LongVersionCode : info.VersionCode);
#pragma warning restore CS0618 // 类型或成员已过时
            }
#else
            b.Append(ThisAssembly.DynamicVersion);
#endif
            b.AppendLine();

            b.Append("[app.flavor] ");
#if __ANDROID__
            b.AppendLine(
#if IS_STORE_PACKAGE
                     "store"
#else
#endif
                    );
#else
            if (DesktopBridge.IsRunningAsUwp)
            {
                b.Append("ms-store");
            }
            b.AppendLine();
#endif

            b.Append("[app.updcha] ");
            b.Append(ApplicationUpdateServiceBaseImpl.UpdateChannelType);
            b.AppendLine();

            b.Append("[app.install] ");
            b.Append(platformService.IsInstall.ToLowerString());
            b.AppendLine();

#if WINDOWS10_0_17763_0_OR_GREATER
            if (OperatingSystem2.IsWindows10AtLeast())
            {
                try
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    var currentPackage = global::Windows.ApplicationModel.Package.Current;
                    var familyName = currentPackage.Id.FamilyName;
                    b.Append("[app.pkg] ");
                    b.Append(familyName);
                    b.AppendLine();
#pragma warning restore CA1416 // 验证平台兼容性
                }
                catch
                {
                }
            }
#endif

            b.Append("[memory.usage] ");
#if __ANDROID__
            var activityManager = activity.GetActivityManager();
            ActivityManager.MemoryInfo memoryInfo = new();
            activityManager.GetMemoryInfo(memoryInfo);
            var nativeHeapSize = memoryInfo.TotalMem;
            var nativeHeapFreeSize = memoryInfo.AvailMem;
            var usedMemInBytes = nativeHeapSize - nativeHeapFreeSize;
            var usedMemInPercentage = usedMemInBytes * 100M / nativeHeapSize;
            b.Append($"{IOPath.GetDisplayFileSizeString(usedMemInBytes)} ({usedMemInPercentage:0.00}%)");
#else
            b.Append(IOPath.GetDisplayFileSizeString(Environment.WorkingSet));
#endif
            b.AppendLine();

            b.Append("[deploy.mode] ");
            b.Append(IApplication.Instance.DeploymentMode);
            b.AppendLine();

            b.Append("[arch.os] ");
            b.Append(RuntimeInformation.OSArchitecture);
            b.AppendLine();

            b.Append("[arch.proc] ");
            b.Append(RuntimeInformation.ProcessArchitecture);
            b.AppendLine();

#if MONOANDROID
            b.Append("[clr.ver] ");
            string? clrVersion;
            try
            {
                clrVersion = GetAssemblyVersion(typeof(object).Assembly);
            }
            catch
            {
                clrVersion = null;
            }
            if (string.IsNullOrEmpty(clrVersion))
                b.Append(Environment.Version);
            else
                b.Append(clrVersion);
            b.AppendLine();
#endif

#if __ANDROID__

#if V2RAY
                b.Append("[v2ray.ver] ");
                b.Append(Libv2ray.Libv2ray.CheckVersionX());
                b.AppendLine();
#endif

            //if (_ThisAssembly.Debuggable)
            //{
            //    b.Append("[app.multi] ");
            //    VirtualApkCheckUtil.GetCheckResult(AndroidApplication.Context, b);
            //    b.AppendLine();
            //}

            //b.Append("[rom.ver] ");
            //AndroidROM.Current.ToString(b);
            //b.AppendLine();

            b.Append("[app.center] ");
            b.AppendLine(VisualStudioAppCenterSDK.TryGetAppSecret(out var appSecret) ? appSecret.Split('-').FirstOrDefault() : string.Empty);

            b.Append("[webview.ver] ");
            GetWebViewImplementationVersionDisplayString(b);
            b.AppendLine();
            static void GetWebViewImplementationVersionDisplayString(StringBuilder b)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var webViewPackage = WebView.CurrentWebViewPackage;
                    if (webViewPackage != default)
                    {
                        var packageName = webViewPackage.PackageName;
                        var packageVersion = webViewPackage.VersionName;
                        if (string.Equals(packageName, "com.android.webview", StringComparison.OrdinalIgnoreCase) || string.Equals(packageName, "com.google.android.webview", StringComparison.OrdinalIgnoreCase))
                        {
                            packageName = "asw"; // Android System Webview
                        }
                        else if (string.Equals(packageName, "com.android.chrome", StringComparison.OrdinalIgnoreCase))
                        {
                            packageName = "chrome"; // Chrome
                        }
                        b.AppendFormat("{0}({1})", packageVersion, packageName);
                        return;
                    }
                }
            }
#endif

            b.Append("[time.start] ");
            GetStartTime(b);
            static void GetStartTime(StringBuilder b)
            {
                string startTimeStr;
                try
                {
                    const string f = "yy-MM-dd HH:mm:ss";
                    const string f2 = "HH:mm:ss";
                    const string f3 = "dd HH:mm:ss";
                    var starttime = ArchiSteamFarm.Core.OS.ProcessStartTime;
                    starttime = starttime.ToLocalTime();
                    var utc_time = starttime.ToUniversalTime();
                    var local = TimeZoneInfo.Local;
                    startTimeStr = utc_time.Hour == starttime.Hour
                        ? starttime.ToString(starttime.Year >= 2100 ? DateTimeFormat.Standard : f)
                        : utc_time.Day == starttime.Day
                        ? $"{utc_time.ToString(f)}({starttime.ToString(f2)} {local.StandardName})"
                        : $"{utc_time.ToString(f)}({starttime.ToString(f3)} {local.StandardName})";
                }
                catch (Exception)
                {
                    startTimeStr = string.Empty;
                }
                b.Append(startTimeStr);
            }
            b.AppendLine();

#if __ANDROID__
            b.Append("[screen] ");
            var metrics = new DisplayMetrics();
#pragma warning disable CS0618 // 类型或成员已过时
            activity.WindowManager?.DefaultDisplay?.GetRealMetrics(metrics);
#pragma warning restore CS0618 // 类型或成员已过时
            GetScreen(activity, metrics, b);
            static void GetScreen(Context context, DisplayMetrics metrics, StringBuilder b)
            {
                var screen_w = metrics.WidthPixels;
                var screen_h = metrics.HeightPixels;
                var screen_max = Math.Max(screen_w, screen_h);
                var screen_min = screen_max == screen_w ? screen_h : screen_w;
                var configuration = context.Resources?.Configuration;
                var screen_dp_w = configuration?.ScreenWidthDp ?? 0;
                var screen_dp_h = configuration?.ScreenHeightDp ?? 0;
                var screen_dp_max = Math.Max(screen_dp_w, screen_dp_h);
                var screen_dp_min = screen_max == screen_dp_w ? screen_dp_h : screen_dp_w;
                b.AppendFormat("{0}x{1}({2}x{3})", screen_max, screen_min, screen_dp_max, screen_dp_min);
                var dpi = (int)metrics.DensityDpi;
                b.AppendFormat(" {0}dpi", dpi);
                if (dpi < (int)DisplayMetricsDensity.Low)
                {
                    b.Append("(<ldpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Low)
                {
                    b.Append("(ldpi)");
                }
                else if (dpi < (int)DisplayMetricsDensity.Medium)
                {
                    b.Append("(ldpi~mdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Medium)
                {
                    b.Append("(mdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Tv)
                {
                    b.Append("(tv)");
                }
                else if (dpi < (int)DisplayMetricsDensity.High)
                {
                    b.Append("(mdpi~hdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.High)
                {
                    b.Append("(hdpi)");
                }
                else if (dpi < (int)DisplayMetricsDensity.Xhigh)
                {
                    b.Append("(hdpi~xhdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Xhigh)
                {
                    b.Append("(xhdpi)");
                }
                else if (dpi < (int)DisplayMetricsDensity.Xxhigh)
                {
                    b.Append("(xhdpi~xxhdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Xxhigh)
                {
                    b.Append("(xxhdpi)");
                }
                else if (dpi < (int)DisplayMetricsDensity.Xxxhigh)
                {
                    b.Append("(xxhdpi~xxxhdpi)");
                }
                else if (dpi == (int)DisplayMetricsDensity.Xxxhigh)
                {
                    b.Append("(xxxhdpi)");
                }
            }
            b.AppendLine();

            //b.Append("[screen.notch] ");
            //b.Append(ScreenCompatUtil.IsNotch(this).ToLowerString());
            //b.AppendLine();

            //b.Append("[screen.notch.hide] ");
            //b.Append(ScreenCompatUtil.IsHideNotch(this).ToLowerString());
            //b.AppendLine();

            //b.Append("[screen.full.gestures] ");
            //b.Append(ScreenCompatUtil.IsFullScreenGesture(this).ToLowerString());
            //b.AppendLine();

            static string GetJavaSystemGetProperty(string propertyKey)
            {
                try
                {
                    return Java.Lang.JavaSystem.GetProperty(propertyKey) ?? "";
                }
                catch
                {
                    return string.Empty;
                }
            }
            b.Append("[jvm.ver] ");
            b.Append(GetJavaSystemGetProperty("java.vm.version"));
            b.AppendLine();

#if MONOANDROID
            b.Append("[mono.ver] ");
            b.Append(Mono.Runtime.GetDisplayName());
            b.AppendLine();
#else
            b.Append("[clr.ver] ");
            b.Append(Environment.Version);
            b.AppendLine();
#endif

            b.Append("[kernel.ver] ");
            b.Append(GetJavaSystemGetProperty("os.version"));
            b.AppendLine();

            b.Append("[device] ");
            b.Append(Build.Device ?? "");
            b.AppendLine();
#endif
            b.Append("[device.name] ");
            b.Append(DeviceInfo2.Name());
            b.AppendLine();
            b.Append("[device.model] ");
            b.Append(
#if __ANDROID__
                    Build.Model ?? ""
#else
                DeviceInfo2.Model()
#endif
                );
            b.AppendLine();
            b.Append("[device.ver] ");
            b.Append(DeviceInfo2.VersionString());
            b.AppendLine();
            b.Append("[device.idiom] ");
            b.Append(DeviceInfo2.Idiom());
            b.AppendLine();
            b.Append("[device.type] ");
            b.Append(DeviceInfo2.DeviceType());
            b.AppendLine();
#if __ANDROID__
            b.Append("[device.product] ");
            b.Append(Build.Product ?? "");
            b.AppendLine();
            b.Append("[device.brand] ");
            b.Append(Build.Brand ?? "");
            b.AppendLine();
#endif
            b.Append("[device.manufacturer] ");
            b.Append(
#if __ANDROID__
                    Build.Manufacturer ?? ""
#else
                DeviceInfo2.Manufacturer()
#endif
                );
            b.AppendLine();
#if __ANDROID__
            b.Append("[device.fingerprint] ");
            b.Append(Build.Fingerprint ?? "");
            b.AppendLine();
            b.Append("[device.hardware] ");
            b.Append(Build.Hardware ?? "");
            b.AppendLine();
            b.Append("[device.tags] ");
            b.Append(Build.Tags ?? "");
            b.AppendLine();
            //if (ThisAssembly.Debuggable)
            //{
            //    b.Append("[device.arc] ");
            //    b.Append(DeviceSecurityCheckUtil.IsCompatiblePC(this).ToLowerString());
            //    b.AppendLine();
            //    b.Append("[device.emulator] ");
            //    b.Append(DeviceSecurityCheckUtil.IsEmulator.ToLowerString());
            //    b.AppendLine();
            //}
            //b.Append("[device.gl.renderer] ");
            //b.Append(GLES20.GlGetString(GLES20.GlRenderer) ?? "");
            //b.AppendLine();
            //b.Append("[device.gl.vendor] ");
            //b.Append(GLES20.GlGetString(GLES20.GlVendor) ?? "");
            //b.AppendLine();
            //b.Append("[device.gl.version] ");
            //b.Append(GLES20.GlGetString(GLES20.GlVersion) ?? "");
            //b.AppendLine();
            //b.Append("[device.gl.extensions] ");
            //b.Append(GLES20.GlGetString(GLES20.GlExtensions) ?? "");
            //b.AppendLine();
            b.Append("[device.biometric] ");
            b.Append(IBiometricService.Instance.IsSupportedAsync().Result.ToLowerString());
            b.AppendLine();
            b.Append("[device.abis]");
            IEnumerable<string>? supportedAbis;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                supportedAbis = Build.SupportedAbis;
            }
            else
            {
                supportedAbis = new[] {
#pragma warning disable CS0618 // 类型或成员已过时
                        Build.CpuAbi!,
                        Build.CpuAbi2!,
#pragma warning restore CS0618 // 类型或成员已过时
                    };
            }
            if (supportedAbis != null)
            {
                supportedAbis = supportedAbis.Where(x => !string.IsNullOrWhiteSpace(x));
                var supportedAbisCount = supportedAbis.Count();
                var i = 0;
                foreach (var item in supportedAbis)
                {
                    b.Append(item);
                    if (i != supportedAbisCount - 1)
                    {
                        b.Append(", ");
                    }
                    i++;
                }
            }
            b.AppendLine();
#endif

#if AVALONIA
            if (OperatingSystem2.Application.UseAvalonia())
            {
                b.Append("[avalonia.ver] ");
                b.Append(GetAssemblyVersion(OperatingSystem2.Application.Types.Avalonia!.Assembly));
                b.AppendLine();
            }
#endif
            if (OperatingSystem2.Application.UseXamarinForms())
            {
                b.Append("[forms.ver] ");
                b.Append(GetAssemblyVersion(OperatingSystem2.Application.Types.XamarinForms!.Assembly));
                b.AppendLine();
            }

            var controllerType = Type.GetType("Microsoft.AspNetCore.Mvc.ControllerBase, Microsoft.AspNetCore.Mvc.Core");
            if (controllerType != null)
            {
                b.Append("[mvc.ver] ");
                b.Append(GetAssemblyVersion(controllerType.Assembly));
                b.AppendLine();
            }

            b.Append("[di.ver] ");
            b.Append(GetAssemblyVersion(typeof(ServiceCollection).Assembly));
            b.AppendLine();

            b.Append("[skia.ver] ");
            b.Append(GetAssemblyVersion(typeof(SkiaSharp.SKColor).Assembly));
            b.AppendLine();

#if AVALONIA
            b.Append("[harfbuzz.ver] ");
            b.Append(GetAssemblyVersion(typeof(HarfBuzzSharp.NativeObject).Assembly));
            b.AppendLine();
#endif

            b.Append("[essentials.supported] ");
            b.Append(Essentials.IsSupported.ToLowerString());
            b.AppendLine();

#if __ANDROID__
            b.Append("[startup.track] ");
            b.AppendLine();
            b.Append(MainApplication.StartupTrack);
            b.AppendLine();
#endif

            var b_str = b.ToString();

            return b_str;
        }

        public static void OnClick()
        {
            var now = DateTime.Now;
            if (show_runtime_info_last_click_time == default || (now - show_runtime_info_last_click_time).TotalSeconds <= show_runtime_info_click_effective_interval)
            {
                show_runtime_info_counter++;
            }
            else
            {
                show_runtime_info_counter = 1;
            }
            show_runtime_info_last_click_time = now;
            if (show_runtime_info_counter >= show_runtime_info_counter_max)
            {
                show_runtime_info_counter = 0;
                show_runtime_info_last_click_time = default;

                MessageBox.Show(GetInfoString(), "");
            }
        }

        static string? GetAssemblyVersion(Assembly assembly)
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
    }
}