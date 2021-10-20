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
    public partial interface IApplication : IService<IApplication>
    {
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

        protected AppType GetType();

        enum AppType : byte
        {
            /// <summary>
            /// https://github.com/AvaloniaUI/Avalonia
            /// </summary>
            Avalonia = 1,

            /// <summary>
            /// Maui / Xamarin Forms + Platform Native UI
            /// <para>https://github.com/dotnet/maui</para>
            /// <para>https://developer.android.google.cn/training/basics/firstapp/building-ui</para>
            /// </summary>
            HybridAndroid,

            /// <summary>
            /// Maui / Xamarin Forms
            /// <para></para>
            /// https://github.com/dotnet/maui
            /// </summary>
            [Obsolete]
            Maui,

            /// <summary>
            /// https://github.com/unoplatform/uno
            /// </summary>
            [Obsolete]
            Uno,

            /// <summary>
            /// https://developer.android.google.cn/training/basics/firstapp/building-ui
            /// </summary>
            [Obsolete]
            NativeAndroid,

            /// <summary>
            /// https://docs.microsoft.com/zh-cn/xamarin/ios/user-interface/ios-ui
            /// </summary>
            [Obsolete]
            NativeiOS,

            /// <summary>
            /// https://github.com/dotnet/winforms
            /// <para></para>
            /// https://github.com/mono/winforms
            /// </summary>
            [Obsolete]
            WinForms,

            /// <summary>
            /// https://github.com/dotnet/wpf
            /// </summary>
            [Obsolete]
            WPF,

            /// <summary>
            /// WinUI 2
            /// <para></para>
            /// https://github.com/Microsoft/microsoft-ui-xaml
            /// <para></para>
            /// https://docs.microsoft.com/zh-cn/windows/apps/winui/winui2
            /// </summary>
            [Obsolete]
            UWP,

            /// <summary>
            /// https://docs.microsoft.com/zh-cn/windows/apps/winui/winui3/desktop-winui3-app-with-basic-interop
            /// </summary>
            [Obsolete]
            WinUI3,
        }

        /// <summary>
        /// 获取当前平台 UI Host
        /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
        /// </summary>
        object CurrentPlatformUIHost { get; }
    }
}