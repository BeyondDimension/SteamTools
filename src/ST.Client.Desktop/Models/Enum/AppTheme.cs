using System.Application.Models;

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
        public static string ToString2(this AppTheme value)
        {
            return value switch
            {
                AppTheme.FollowingSystem => "auto",
                AppTheme.Light => "light",
                _ => "dark",
            };
        }
    }
}