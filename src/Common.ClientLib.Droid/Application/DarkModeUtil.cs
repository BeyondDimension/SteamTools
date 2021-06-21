using Android.Content;
using Android.Content.Res;
//using Android.OS;
//using AndroidApplication = Android.App.Application;

namespace System.Application
{
    /// <summary>
    /// DarkMode 深色主题 / 夜间模式
    /// <para>https://developer.android.google.cn/about/versions/10/highlights#dark_theme</para>
    /// </summary>
    public static class DarkModeUtil
    {
        /// <summary>
        /// 是否正在使用夜间模式
        /// </summary>
        /// <param name="context"></param>
        /// <returns>
        /// <para><see langword="true"/> 夜间模式已激活，我们正在使用深色主题(Dark)</para>
        /// <para><see langword="false"/> 夜间模式未激活，我们正在使用亮色主题(Light)</para>
        /// </returns>
        static bool? IsNight(Context context)
        {
            // https://developer.android.google.cn/guide/topics/ui/look-and-feel/darktheme#%E9%85%8D%E7%BD%AE%E5%8F%98%E6%9B%B4
            var cfg = context.Resources?.Configuration;
            if (cfg == null) return null;
            var mode = cfg.UiMode & UiMode.NightMask;
            return mode switch
            {
                UiMode.NightNo => false,// Night mode is not active, we're using the light theme
                UiMode.NightYes => true,// Night mode is active, we're using dark theme
                _ => null, // default null
            };
        }

        /// <summary>
        /// 是否正在使用深色主题(Dark)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsDarkMode(Context context)
        {
            var isNight = IsNight(context);
            return isNight.HasValue && isNight.Value;
        }

        ///// <summary>
        ///// 是否正在安卓Q中使用深色主题(Dark)
        ///// </summary>
        //public static bool IsDarkModeQ
        //    => Build.VERSION.SdkInt >= BuildVersionCodes.Q && IsDarkMode(AndroidApplication.Context);
    }
}