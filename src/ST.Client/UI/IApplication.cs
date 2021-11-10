using System;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Collections.Generic;
using System.Text;

namespace System.Application.UI
{
    /// <summary>
    /// 当前应用程序
    /// </summary>
    public partial interface IApplication
    {
        /// <summary>
        /// IsWindows or IsMacOS or (IsLinux and !IsAndroid)
        /// </summary>
        public static readonly bool IsDesktopPlatform = OperatingSystem2.IsWindows || OperatingSystem2.IsMacOS || (OperatingSystem2.IsLinux && !OperatingSystem2.IsAndroid);

        static IApplication Instance => DI.Get<IApplication>();

        /// <summary>
        /// 获取或设置当前应用的主题
        /// </summary>
        AppTheme Theme { get; set; }

        /// <summary>
        /// 获取当前应用的实际主题
        /// </summary>
        AppTheme ActualTheme => Theme switch
        {
            AppTheme.FollowingSystem => GetActualThemeByFollowingSystem(),
            AppTheme.Light => AppTheme.Light,
            AppTheme.Dark => AppTheme.Dark,
            _ => DefaultActualTheme,
        };

        /// <summary>
        /// 获取当前应用的默认主题
        /// </summary>
        protected AppTheme DefaultActualTheme { get; }

        /// <summary>
        /// 获取当前应用主题跟随系统时的实际主题
        /// </summary>
        /// <returns></returns>
        protected AppTheme GetActualThemeByFollowingSystem();

        /// <summary>
        /// 获取当前平台 UI Host
        /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
        /// </summary>
        object CurrentPlatformUIHost { get; }

        /// <summary>
        /// 初始化设置项变更时监听
        /// </summary>
        void InitSettingSubscribe()
        {
            UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
        }
    }
}