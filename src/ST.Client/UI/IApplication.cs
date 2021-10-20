using System;
using System.Collections.Generic;
using System.Text;
using System.Application.Models;
using System.Application.Services;

namespace System.Application.UI
{
    /// <summary>
    /// 当前应用程序
    /// </summary>
    public partial interface IApplication
    {
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

        private static readonly Lazy<bool> mIsAvaloniaApp = new(() => Type.GetType(TypeNames.Avalonia) != null);
        static bool IsAvaloniaApp => mIsAvaloniaApp.Value;

        private static readonly Lazy<bool> mIsXamarinForms = new(() => Type.GetType(TypeNames.XamarinForms) != null);
        static bool IsXamarinForms => mIsXamarinForms.Value;

        static class TypeNames
        {
            public const string Avalonia = "Avalonia.Application, Avalonia.Controls";
            public const string XamarinForms = "Xamarin.Forms.Application, Xamarin.Forms.Core";
        }
    }
}