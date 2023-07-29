// ReSharper disable once CheckNamespace
namespace Avalonia.Controls;

public static partial class WindowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActiveWindow(this Window? window)
    {
        if (window == null)
            return false;
        if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.Windows.Any_Nullable(x => x == window && x.IsActive && x.IsVisible))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 设置调整大小模式。
    /// </summary>
    /// <param name="window"></param>
    /// <param name="value"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetResizeMode(this Window window, ResizeMode value)
    {
        var p = Ioc.Get<IPlatformService>();
        switch (value)
        {
            case ResizeMode.NoResize:
            case ResizeMode.CanMinimize:
                window.CanResize = false;
                break;
            case ResizeMode.CanResize:
#pragma warning disable CS0618 // 类型或成员已过时
            case ResizeMode.CanResizeWithGrip:
#pragma warning restore CS0618 // 类型或成员已过时
                window.CanResize = true;
                break;
        }
        p.SetResizeMode(window.TryGetPlatformHandle().Handle, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ActivateWorkaround(this Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        // Call default Activate() anyway.
        window.Activate();

        // Skip workaround for non-windows platforms.
#if WINDOWS
        var platformImpl = window.TryGetPlatformHandle();
        if (ReferenceEquals(platformImpl, null)) return;

        var platformHandle = platformImpl.Handle;
        if (ReferenceEquals(platformHandle, null)) return;

        var handle = platformHandle;
        if (handle == IntPtr.Zero) return;

        try
        {
            PInvoke.User32.SetForegroundWindow(handle);
        }
        catch
        {
            // ignored
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShowActivate(this Window window)
    {
        if (window.WindowState != WindowState.Normal) window.WindowState = WindowState.Normal;
        window.Activate();
    }
}