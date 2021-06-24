using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Application.Properties;
using System.IO;
using Xamarin.Essentials;
using AndroidApplication = Android.App.Application;
using JString = Java.Lang.String;

namespace System.Application.Security
{
    /// <summary>
    /// 设备安全检查
    /// </summary>
    public static class DeviceSecurityCheckUtil
    {
        /// <summary>
        /// 是否运行在兼容的PC中，例如Chromebook中的Chrome OS
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsCompatiblePC(Context context)
        {
            const string ARC_DEVICE_PATTERN = ".+_cheets|cheets_.+";
            var device = Build.Device;
            if (device != null)
            {
                var j_device = new JString(device);
                if (j_device.Matches(ARC_DEVICE_PATTERN))
                {
                    return true;
                }
            }
            var packageManager = context.PackageManager;
            if (packageManager != null)
            {
                if (packageManager.HasSystemFeature("org.chromium.arc.device_management"))
                {
                    return true;
                }
                if (packageManager.HasSystemFeature(PackageManager.FeaturePc))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 当前设备是否经过了安全检查
        /// </summary>
        /// <param name="enableEmulator"></param>
        /// <returns></returns>
        public static bool IsSupported(bool enableEmulator, bool allowXposed = false, bool allowRoot = false)
        {
            if (!enableEmulator && IsEmulator)
                return Exit(ExitCode.IsEmulator);

            if (!enableEmulator && SecurityCheckUtil.CheckIsDebuggerConnected())
                return Exit(ExitCode.IsDebuggerConnected);

            if (!allowXposed && SecurityCheckUtil.IsXposedExists())
                return Exit(ExitCode.IsXposedExists);

            if (!enableEmulator && !allowRoot && SecurityCheckUtil.IsRoot())
                return Exit(ExitCode.IsRoot);

            var context = AndroidApplication.Context;

            if (!enableEmulator && SecurityCheckUtil.CheckIsDebugVersion(context))
                return Exit(ExitCode.IsDebugVersion);

            if (IsCompatiblePC(context))
                return Exit(ExitCode.IsCompatiblePC);

            if (VirtualApkCheckUtil.Check(context))
                return Exit(ExitCode.IsVirtualApk);

            return true;

            static bool Exit(ExitCode code)
            {
                var str = code switch
                {
                    ExitCode.IsVirtualApk => SR.ExitCode_IsVirtualApk,
                    ExitCode.IsRoot => SR.ExitCode_IsRoot,
                    ExitCode.IsXposedExists => SR.ExitCode_IsXposedExists,
                    ExitCode.IsCompatiblePC or ExitCode.IsEmulator => SR.ExitCode_IsEmulator,
                    _ => $"Incompatible device error {(int)code}",
                };
                if (str != null)
                {
                    Toast.Show(str, ToastLength.Long);
                }
                else
                {
                    Log.Error("Security", "exit code: {0}", (int)code);
                }
                return false;
            }
        }

        enum ExitCode
        {
            /// <summary>
            /// (423)不支持运行在 [模拟器] 中
            /// </summary>
            IsEmulator = 423,

            /// <summary>
            /// (451)不支持运行在 [兼容的PC] 中
            /// </summary>
            IsCompatiblePC = 451,

            /// <summary>
            /// (452)不允许链接上调试器运行
            /// </summary>
            IsDebuggerConnected,

            /// <summary>
            /// (453)当前是调试版本不允许在非调试环境下允许
            /// </summary>
            IsDebugVersion,

            /// <summary>
            /// (454)不允许多开/分身
            /// </summary>
            IsVirtualApk,

            /// <summary>
            /// (455)存在Xposed框架
            /// </summary>
            IsXposedExists,

            /// <summary>
            /// (456)设备已开放Root权限
            /// </summary>
            IsRoot,

            /// <summary>
            /// (457)设备是手表⌚
            /// </summary>
            IsWear,
        }

        #region Emulator

        static int rating = -1;
        static readonly Lazy<bool> mIsEmulator = new(GetIsEmulator);

        static bool GetIsEmulator()
        {
            // 参考 https://github.com/gingo/android-emulator-detector/blob/master/EmulatorDetectorProject/EmulatorDetector/src/main/java/net/skoumal/emulatordetector/EmulatorDetector.java
            return DeviceInfo.DeviceType == DeviceType.Virtual || _();
            static bool contains(string l, string r)
            {
                return l.Contains(r, StringComparison.OrdinalIgnoreCase);
            }
            static bool equals(string l, string r) => string.Equals(l, r, StringComparison.OrdinalIgnoreCase);
            static bool _()
            {
                var newRating = 0;
                if (rating < 0)
                {
                    var PRODUCT = Build.Product;
                    if (PRODUCT == null ||
                        contains(PRODUCT, "sdk") ||
                        contains(PRODUCT, "Andy") ||
                        contains(PRODUCT, "ttVM_Hdragon") ||
                        contains(PRODUCT, "google_sdk") ||
                        contains(PRODUCT, "Droid4X") ||
                        contains(PRODUCT, "nox") ||
                        contains(PRODUCT, "sdk_x86") ||
                        contains(PRODUCT, "sdk_google") ||
                        contains(PRODUCT, "vbox86p"))
                    {
                        newRating++;
                    }

                    var MANUFACTURER = Build.Manufacturer;
                    if (MANUFACTURER == null ||
                        equals(MANUFACTURER, "unknown") ||
                        equals(MANUFACTURER, "Genymotion") ||
                        contains(MANUFACTURER, "VS Emulator") ||
                        contains(MANUFACTURER, "Andy") ||
                        contains(MANUFACTURER, "MIT") ||
                        contains(MANUFACTURER, "nox") ||
                        contains(MANUFACTURER, "TiantianVM"))
                    {
                        newRating++;
                    }

                    var BRAND = Build.Brand;
                    if (BRAND == null ||
                        equals(BRAND, "generic") ||
                        equals(BRAND, "generic_x86") ||
                        equals(BRAND, "TTVM") ||
                        contains(BRAND, "Andy"))
                    {
                        newRating++;
                    }

                    var DEVICE = Build.Device;
                    if (DEVICE == null ||
                        contains(DEVICE, "generic") ||
                        contains(DEVICE, "generic_x86") ||
                        contains(DEVICE, "Andy") ||
                        contains(DEVICE, "ttVM_Hdragon") ||
                        contains(DEVICE, "Droid4X") ||
                        contains(DEVICE, "nox") ||
                        contains(DEVICE, "generic_x86_64") ||
                        contains(DEVICE, "vbox86p"))
                    {
                        newRating++;
                    }

                    var MODEL = Build.Model;
                    if (MODEL == null ||
                        equals(MODEL, "sdk") ||
                        equals(MODEL, "google_sdk") ||
                        contains(MODEL, "Droid4X") ||
                        contains(MODEL, "TiantianVM") ||
                        contains(MODEL, "Andy") ||
                        equals(MODEL, "Android SDK built for x86_64") ||
                        equals(MODEL, "Android SDK built for x86"))
                    {
                        newRating++;
                    }

                    var HARDWARE = Build.Hardware;
                    if (HARDWARE == null ||
                        equals(HARDWARE, "goldfish") ||
                        equals(HARDWARE, "vbox86") ||
                        contains(HARDWARE, "nox") ||
                        contains(HARDWARE, "ttVM_x86"))
                    {
                        newRating++;
                    }

                    var FINGERPRINT = Build.Fingerprint;
                    if (FINGERPRINT == null ||
                        contains(FINGERPRINT, "generic/sdk/generic") ||
                        contains(FINGERPRINT, "generic_x86/sdk_x86/generic_x86") ||
                        contains(FINGERPRINT, "Andy") ||
                        contains(FINGERPRINT, "ttVM_Hdragon") ||
                        contains(FINGERPRINT, "generic_x86_64") ||
                        contains(FINGERPRINT, "generic/google_sdk/generic") ||
                        contains(FINGERPRINT, "vbox86p") ||
                        contains(FINGERPRINT, "generic/vbox86p/vbox86p"))
                    {
                        newRating++;
                    }

                    try
                    {
                        var opengl = Android.Opengl.GLES20.GlGetString(Android.Opengl.GLES20.GlRenderer);
                        if (!string.IsNullOrWhiteSpace(opengl))
                        {
#pragma warning disable CS8604 // 可能的 null 引用参数。
                            if (contains(opengl, "Bluestacks") ||
                                contains(opengl, "Translator") ||
                                contains(opengl, "youwave")
                            )
#pragma warning restore CS8604 // 可能的 null 引用参数。
                                newRating += 10;
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
#pragma warning disable CS0618 // 类型或成员已过时
                        var esDir = Android.OS.Environment.ExternalStorageDirectory;
#pragma warning restore CS0618 // 类型或成员已过时
                        if (esDir != null)
                        {
                            var path = Path.Combine(esDir.ToString(), "windows", "BstSharedFolder");
                            var sharedFolder = new Java.IO.File(path);
                            if (sharedFolder.Exists())
                            {
                                newRating += 10;
                            }
                        }
                    }
                    catch
                    {
                    }
                    rating = newRating;
                }
                return rating > 3;
            }
        }

        /// <summary>
        /// 当前设备是否为模拟器
        /// </summary>
        public static bool IsEmulator => mIsEmulator.Value;

        #endregion
    }
}