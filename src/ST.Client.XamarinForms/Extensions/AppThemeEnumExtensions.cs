using System.Application.Models;
using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class AppThemeEnumExtensions
    {
        public static OSAppTheme Convert(this AppTheme theme) => theme switch
        {
            AppTheme.Dark => OSAppTheme.Dark,
            AppTheme.Light => OSAppTheme.Light,
            _ => OSAppTheme.Unspecified,
        };
    }
}
