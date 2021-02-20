using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Linq;
using AndroidOSDebug = Android.OS.Debug;

namespace System.Application.Security
{
    /// <summary>
    /// 安全检查
    /// <para>https://github.com/lamster2018/EasyProtector/blob/master/library/src/main/java/com/lahm/library/SecurityCheckUtil.java</para>
    /// </summary>
    internal static class SecurityCheckUtil
    {
        /// <summary>
        /// 检测当前APP是否为调试版本
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CheckIsDebugVersion(Context context)
        {
            var appInfo = context.ApplicationInfo;
            if (appInfo != null)
            {
                return appInfo.Flags.HasFlag(ApplicationInfoFlags.Debuggable);
            }
            return false;
        }

        /// <summary>
        /// 检测是否连接上调试器
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsDebuggerConnected() => AndroidOSDebug.IsDebuggerConnected;

        static int GetroSecureProp()
        {
            string? roSecureObj = null;
            try
            {
                if (SystemProperties.TryGet("ro.secure", out var value))
                {
                    roSecureObj = value;
                }
            }
            catch
            {
            }
            return string.IsNullOrWhiteSpace(roSecureObj) ? 1 : (roSecureObj == "0" ? 0 : 1);
        }

        static readonly string[] su_paths = new[] {
            "/sbin/su",
            "/system/bin/su",
            "/system/xbin/su",
            "/data/local/xbin/su",
            "/data/local/bin/su",
            "/system/sd/xbin/su",
            "/system/bin/failsafe/su",
            "/data/local/su",
        };

        static bool IsSUExist() => su_paths.Any(x => new Java.IO.File(x).Exists());

        /// <summary>
        /// 检测是否有ROOT权限
        /// </summary>
        /// <returns></returns>
        public static bool IsRoot()
        {
            int secureProp = GetroSecureProp();
            if (secureProp == 0) //eng/userdebug版本，自带root权限
                return true;
            return IsSUExist(); //user版本，继续查su文件
        }

        const string XPOSED_HELPERS = "de.robv.android.xposed.XposedHelpers";
        const string XPOSED_BRIDGE = "de.robv.android.xposed.XposedBridge";

        /// <summary>
        /// 当前是否装有 Xposed 框架
        /// </summary>
        /// <returns></returns>
        public static bool IsXposedExists()
        {
            var classLoader = Java.Lang.ClassLoader.SystemClassLoader;
            if (classLoader == null) throw new NullReferenceException("Java.Lang.ClassLoader.SystemClassLoader is null.");

            try
            {
                var xpHelperObj = classLoader.LoadClass(XPOSED_HELPERS)?.NewInstance();
            }
            catch (Java.Lang.InstantiationException e)
            {
                e.PrintStackTraceWhenDebug();
                return true;
            }
            catch (Java.Lang.IllegalAccessException e)
            {
                e.PrintStackTraceWhenDebug();
                return true;
            }
            catch (Java.Lang.ClassNotFoundException e)
            {
                e.PrintStackTraceWhenDebug();
            }

            try
            {
                var xpBridgeObj = classLoader.LoadClass(XPOSED_BRIDGE)?.NewInstance();
            }
            catch (Java.Lang.InstantiationException e)
            {
                e.PrintStackTraceWhenDebug();
                return true;
            }
            catch (Java.Lang.IllegalAccessException e)
            {
                e.PrintStackTraceWhenDebug();
                return true;
            }
            catch (Java.Lang.ClassNotFoundException e)
            {
                e.PrintStackTraceWhenDebug();
            }

            try
            {
                throw new Java.Lang.Exception(Random2.GenerateRandomString(Random2.Next(5, 10)));
                // 通过主动抛出异常，检查堆栈信息来判断是否存在XP框架。
            }
            catch (Java.Lang.Exception e)
            {
                var hasFind = e.GetStackTrace()
                    .Any(x => x.ClassName != null &&
                    x.ClassName.Contains(XPOSED_BRIDGE, StringComparison.OrdinalIgnoreCase));
                if (hasFind) return true;
            }

            return false;
        }
    }
}