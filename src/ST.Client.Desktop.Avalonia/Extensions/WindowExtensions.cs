using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class WindowExtensions
    {
        public static bool IsActiveWindow(this Window? window)
        {
            if (window == null)
                return false;
            if (IDesktopAvaloniaAppService.Instance.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Windows.Any_Nullable(x => x == window && x.IsActive && x.IsVisible))
                {
                    return true;
                }
            }
            return false;
        }

#if DEBUG
        //public static void AttachDevTools2(this TopLevel root)
        //{
        //    if (DI.Platform == Platform.Apple && DI.DeviceIdiom == DeviceIdiom.Desktop) return;
        //    root.AttachDevTools();
        //}
#endif
    }
}