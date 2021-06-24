using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;

namespace System.Application
{
    /// <summary>
    /// 全面屏/刘海屏兼容适配
    /// </summary>
    public static class ScreenCompatUtil
    {
        /// <summary>
        /// 是否为刘海屏设备
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static bool? IsNotch(Activity activity)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                return activity?.Window?.DecorView.RootWindowInsets?.DisplayCutout != null;
            }
            else if (AndroidROM.Current.IsMIUI)
            {
                return MI.IsNotch();
            }
            else if (AndroidROM.Current.IsEMUI)
            {
                return HUAWEI.HasNotchInScreen(activity);
            }
            else if (AndroidROM.Current.IsColorOS)
            {
                return OPPO.IsNotch(activity);
            }
            else if (AndroidROM.Current.IsFuntouchOS)
            {
                return Vivo.IsNotch(activity);
            }
            return null;
        }

        /// <summary>
        /// 是否隐藏屏幕刘海
        /// </summary>
        /// <returns></returns>
        public static bool? IsHideNotch(Context context)
        {
            if (AndroidROM.Current.IsMIUI) // https://dev.mi.com/console/doc/detail?pId=1293#_4
            {
                return MI.IsHideNotch(context);
            }
            else if (AndroidROM.Current.IsEMUI)
            {
                return HUAWEI.IsHideNotch(context);
            }
            return null;
        }

        /// <summary>
        /// 是否开启了全面屏手势
        /// </summary>
        /// <returns></returns>
        public static bool? IsFullScreenGesture(Context context)
        {
            if (AndroidROM.Current.IsMIUI)
            {
                return MI.IsFullScreenGesture(context);
            }
            return null;
        }

        /// <summary>
        /// 小米/MIUI 参考资料：
        /// <para>https://dev.mi.com/console/doc/detail?pId=1293#_3_0</para>
        /// </summary>
        public static class MI
        {
            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)判断设备为 Notch 机型
            /// <para><see langword="true"/>：是刘海屏；<see langword="false"/>：非刘海屏。</para>
            /// </summary>
            /// <returns></returns>
            public static bool IsNotch()
            {
                SystemProperties.TryGet("ro.miui.notch", out var value);
                return value == "1";
            }

            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)获取刘海尺寸：width、height
            /// <para>int[0]值为刘海宽度 int[1]值为刘海高度。</para>
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static int[] GetNotchSize(Context context)
            {
                var result = new[] { 0, 0 };
                var resourceId = context.Resources!.GetIdentifier("notch_width", "dimen", "android");
                if (resourceId > 0) result[0] = context.Resources.GetDimensionPixelSize(resourceId);
                resourceId = context.Resources.GetIdentifier("notch_height", "dimen", "android");
                if (resourceId > 0) result[1] = context.Resources.GetDimensionPixelSize(resourceId);
                return result;
            }

            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)获取默认和隐藏刘海区开关值接口
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static bool IsHideNotch(Context context)
            {
                return Settings.Global.GetInt(context.ContentResolver, "force_black", 0) == 1;
            }

            /// <summary>
            /// 是否打开了全面屏手势
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static bool IsFullScreenGesture(Context context)
            {
                // https://www.sogou.com/web?query=force_fsg_nav_bar
                return Settings.Global.GetInt(context.ContentResolver, "force_fsg_nav_bar", 0) != 0;
            }
        }

        /// <summary>
        /// 华为/EMUI 参考资料：
        /// <para>https://developer.huawei.com/consumer/cn/devservice/doc/50114</para>
        /// </summary>
        public static class HUAWEI
        {
            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)是否是刘海屏手机：
            /// <para><see langword="true"/>：是刘海屏；<see langword="false"/>：非刘海屏。</para>
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static bool HasNotchInScreen(Context context)
            {
                try
                {
                    var cl = context.ClassLoader;
                    var HwNotchSizeUtil = cl?.LoadClass("com.huawei.android.util.HwNotchSizeUtil");
                    var get = HwNotchSizeUtil?.GetMethod("hasNotchInScreen");
                    if (get != null) return (bool)get.Invoke(HwNotchSizeUtil)!;
                }
                catch
                {
                }
                return false;
            }

            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)获取刘海尺寸：width、height
            /// <para>int[0]值为刘海宽度 int[1]值为刘海高度。</para>
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static int[] GetNotchSize(Context context)
            {
                try
                {
                    var cl = context.ClassLoader;
                    var HwNotchSizeUtil = cl?.LoadClass("com.huawei.android.util.HwNotchSizeUtil");
                    var get = HwNotchSizeUtil?.GetMethod("getNotchSize");
                    if (get != null) return (int[])get.Invoke(HwNotchSizeUtil)!;
                }
                catch
                {
                }
                return new[] { 0, 0 };
            }

            public const string DISPLAY_NOTCH_STATUS = "display_notch_status";

            /// <summary>
            /// (仅适用于<see cref="BuildVersionCodes.O"/>)获取默认和隐藏刘海区开关值接口
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public static bool IsHideNotch(Context context)
            {
                var mIsNotchSwitchOpen = Settings.Secure.GetInt(context.ContentResolver, DISPLAY_NOTCH_STATUS, 0);
                // 0表示“默认”，1表示“隐藏显示区域”
                return mIsNotchSwitchOpen == 1;
            }
        }

        /// <summary>
        /// OPPO/ColorOS 参考资料：
        /// <para>https://open.oppomobile.com/wiki/doc#id=10159</para>
        /// </summary>
        public static class OPPO
        {
            public static bool IsNotch(Context context)
            {
                // 返回 true为凹形屏 ，可识别OPPO的手机是否为凹形屏。
                return context.PackageManager?.HasSystemFeature("com.oppo.feature.screen.heteromorphism") ?? false;
            }
        }

        /// <summary>
        /// vivo/FuntouchOS 参考资料：
        /// <para>https://dev.vivo.com.cn/documentCenter/doc/103</para>
        /// </summary>
        public static class Vivo
        {
            public const int NOTCH_IN_SCREEN_VOIO_MARK = 0x00000020;//是否有凹槽
            public const int ROUNDED_IN_SCREEN_VOIO_MARK = 0x00000008;//是否有圆角

            public static bool IsFeatureSupport(Context context, int mark)
            {
                try
                {
                    var cl = context.ClassLoader;
                    var FtFeature = cl.LoadClass("android.util.FtFeature");
                    var get = FtFeature.GetMethod("isFeatureSupport", typeof(int).GetJClass());
                    return (bool)get.Invoke(FtFeature, mark);
                }
                catch
                {
                    return false;
                }
            }

            public static bool IsNotch(Context context)
            {
                return IsFeatureSupport(context, NOTCH_IN_SCREEN_VOIO_MARK) || IsFeatureSupport(context, ROUNDED_IN_SCREEN_VOIO_MARK);
            }
        }
    }
}