using System.Application.Models;
using System.Application.UI.Resx;

namespace System
{
    public static partial class AppThemeEnumExtensions
    {
        /// <summary>
        /// Resx
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToString3(this AppTheme value) => value switch
        {
            AppTheme.FollowingSystem => AppResources.Settings_UI_SystemDefault,
            AppTheme.Light => AppResources.Settings_UI_Light,
            _ => AppResources.Settings_UI_Dark,
        };
    }
}