using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Properties;
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
        static bool IsCompatiblePC(Context context)
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
        public static bool IsSupported(bool enableEmulator)
        {
            if (!enableEmulator && AndroidROM.IsEmulator)
                return Exit(ExitCode.IsEmulator);

            if (!enableEmulator && SecurityCheckUtil.CheckIsDebuggerConnected())
                return Exit(ExitCode.IsDebuggerConnected);

            if (SecurityCheckUtil.IsXposedExists())
                return Exit(ExitCode.IsXposedExists);

            if (SecurityCheckUtil.IsRoot())
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
                    _ => null,
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
    }
}