using AppTheme = System.Application.Models.AppTheme;
#if MAUI
using OSAppTheme = Microsoft.Maui.ApplicationModel.AppTheme;
#else
using Xamarin.Forms;
#endif

// ReSharper disable once CheckNamespace
namespace System;

public static class AppThemeEnumExtensions
{
    public static OSAppTheme Convert(this AppTheme theme) => theme switch
    {
        AppTheme.Dark => OSAppTheme.Dark,
        AppTheme.Light => OSAppTheme.Light,
        _ => OSAppTheme.Unspecified,
    };

    public static AppTheme Convert(this OSAppTheme theme) => theme switch
    {
        OSAppTheme.Dark => AppTheme.Dark,
        OSAppTheme.Light => AppTheme.Light,
        _ => AppTheme.FollowingSystem,
    };
}
