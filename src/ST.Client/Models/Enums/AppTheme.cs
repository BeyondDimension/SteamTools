using System.Application.Models;
using System.Application.UI.Resx;

// ReSharper disable once CheckNamespace
namespace System.Application.Models
{
    /// <summary>
    /// 应用程序主题
    /// </summary>
    public enum AppTheme : byte
    {
        /// <summary>
        /// 跟随系统
        /// </summary>
        FollowingSystem,

        /// <summary>
        /// 亮色主题
        /// </summary>
        Light,

        /// <summary>
        /// 暗色主题
        /// </summary>
        Dark,

        /// <summary>
        /// 高对比度主题
        /// </summary>
        HighContrast,

        /// <summary>
        /// 自定义主题
        /// </summary>
        [Obsolete]
        Custom,
    }
}

namespace System
{
    public static partial class AppThemeEnumExtensions
    {
        /// <summary>
        /// auto/light/dark
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToString2(this AppTheme value) => value switch
        {
            AppTheme.FollowingSystem => "auto",
            AppTheme.Light => "light",
            AppTheme.HighContrast => "highContrast",
            _ => "dark",
        };

        /// <summary>
        /// Resx / AppResources
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToString3(this AppTheme value) => value switch
        {
            AppTheme.FollowingSystem => AppResources.Settings_UI_SystemDefault,
            AppTheme.Light => AppResources.Settings_UI_Light,
            AppTheme.HighContrast => AppResources.Settings_UI_HighContrast,
            _ => AppResources.Settings_UI_Dark,
        };
    }
}