#if WINDOWS
using Avalonia.Threading;
using System.Windows;
using Vanara.PInvoke;
using User32 = PInvoke.User32;

namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    readonly WindowToolsController windowTools = new();
    readonly Dictionary<nint, nint> desktopThumbnails = new();
    MouseUpEventHandler? windowPicker;

    public void GetMoveMouseDownWindow(Action<NativeWindowModel> updateWindowCallback)
    {
        if (windowPicker is not null)
            return;

        void WindowPicked(object? sender, PointD point)
        {
            MouseHook.OnMouseUp -= WindowPicked;
            windowPicker = null;
            Dispatcher.UIThread.Post(() =>
            {
                RestoreSystemCursor();
                try
                {
                    using var dpi = new WindowToolsNative.DpiScope();
                    var hit = WindowToolsNative.WindowFromPoint(new() { X = (int)point.X, Y = (int)point.Y });
                    var handle = WindowToolsNative.GetAncestor(hit, 2);
                    var className = new string(User32.GetClassName(handle));
                    if (className is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd" ||
                        WindowToolsNative.GetWindowThreadProcessId(handle, out var pid) == 0 || pid == Environment.ProcessId)
                        return;

                    var process = Process.GetProcessById((int)pid);
                    string? path;
                    try
                    {
                        path = process.MainModule?.FileName;
                    }
                    catch (Win32Exception)
                    {
                        path = Interop.Kernel32.QueryFullProcessImageName(process);
                    }

                    updateWindowCallback(new NativeWindowModel(handle, new string(User32.GetWindowText(handle)),
                        className, process, path, process.ProcessName));
                }
                catch (Exception e) when (e is ArgumentException or InvalidOperationException or Win32Exception)
                {
                    e.LogAndShowT(TAG);
                }
            });
        }

        MouseHook.OnMouseUp += WindowPicked;
        windowPicker = WindowPicked;
        var cursor = User32.LoadCursor(IntPtr.Zero, MouseHook.OCR_CROSS).DangerousGetHandle();
        var copy = WindowToolsNative.CopyImage(cursor, 2, 0, 0, 0); // IMAGE_CURSOR; SetSystemCursor destroys its argument.
        if (copy != 0)
            MouseHook.SetSystemCursor(copy, MouseHook.OCR_NORMAL);
    }

    static void RestoreSystemCursor() => User32.SystemParametersInfo(User32.SystemParametersInfoAction.SPI_SETCURSORS,
        0, nint.Zero, User32.SystemParametersInfoFlags.None);

    public void BeautifyTheWindow(nint hWnd)
    {
        if (hWnd == 0)
            return;

        var style = User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE);
        User32.SetWindowLongPtr(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, style & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX & ~WS_CAPTION);
    }

    public void BorderlessWindow(NativeWindowModel window) => windowTools.Borderless(window);

    public void MaximizeWindow(NativeWindowModel window) => windowTools.Show(window, 3); // SW_MAXIMIZE

    public void NormalWindow(NativeWindowModel window) => RestoreWindow(window);

    public void ShowWindow(NativeWindowModel window) => windowTools.Show(window, 5); // SW_SHOW

    public void HideWindow(NativeWindowModel window) => windowTools.Show(window, 0); // SW_HIDE

    public void ToWallerpaperWindow(NativeWindowModel window)
    {
        if (GetWindowMonitors().FirstOrDefault() is { } monitor)
            SetWallpaperWindow(window, monitor, mouse: true, keyboard: false);
    }

    public unsafe string? GetWallerpaperImagePath()
    {
        const uint length = 260;
        var buffer = stackalloc char[(int)length];
        return User32.SystemParametersInfo(User32.SystemParametersInfoAction.SPI_GETDESKWALLPAPER, length,
            (nint)buffer, User32.SystemParametersInfoFlags.None) ? new string(buffer) : null;
    }

    public void ResetWallerpaper()
    {
        windowTools.ResetWallpaper();
        ReleaseDesktopThumbnails();
    }

    public void SetParentWindow(nint source, nint dest) => User32.SetParent(source, dest);

    public void SetActiveWindow(NativeWindowModel window)
    {
        Interop.User32.SetActiveWindow(window.Handle);
        User32.SetForegroundWindow(window.Handle);
    }

    public void SetWindowPenetrate(nint dest)
    {
        var style = WindowToolsNative.GetWindowLong(dest, WindowToolsNative.ExStyle);
        WindowToolsNative.SetWindowLong(dest, WindowToolsNative.ExStyle, style | WindowToolsNative.Transparent | WindowToolsNative.Layered);
        WindowToolsNative.SetLayeredWindowAttributes(dest, 0, 255, 2);
    }

    public IReadOnlyList<NativeWindowMonitor> GetWindowMonitors() => WindowToolsController.GetMonitors();

    public NativeWindowToolInfo? GetWindowToolInfo(NativeWindowModel window) => windowTools.GetInfo(window);

    public bool SetWindowToolOptions(NativeWindowModel window, NativeWindowToolOptions options) => windowTools.SetOptions(window, options);

    public bool SetWindowPenetrate(NativeWindowModel window, bool enabled) => windowTools.SetClickThrough(window, enabled);

    public bool SetWindowToolBounds(NativeWindowModel window, NativeWindowBounds bounds) => windowTools.SetBounds(window, bounds);

    public bool MoveWindowToMonitor(NativeWindowModel window, NativeWindowMonitor monitor, NativeWindowMonitorAction action) =>
        windowTools.MoveToMonitor(window, monitor, action);

    public bool SetWallpaperWindow(NativeWindowModel window, NativeWindowMonitor monitor, bool mouse, bool keyboard) =>
        windowTools.SetWallpaper(window, monitor, mouse, keyboard);

    public bool SetWallpaperInput(NativeWindowModel window, bool mouse, bool keyboard) => windowTools.ChangeWallpaperInput(window, mouse, keyboard);

    public bool InteractWithWallpaper(NativeWindowModel window) => windowTools.InteractWithWallpaper(window);

    public bool PauseWallpaperWindow(NativeWindowModel window) => windowTools.PauseWallpaper(window);

    public bool RestoreWindow(NativeWindowModel window) => windowTools.Restore(window);

    public void SetWindowRules(IReadOnlyList<NativeWindowRule> rules) => windowTools.SetRules(rules);

    public void ShutDownWindowTools()
    {
        if (windowPicker is { } picker)
        {
            MouseHook.OnMouseUp -= picker;
            windowPicker = null;
            RestoreSystemCursor();
        }
        ResetWallerpaper();
        windowTools.ShutDown();
    }

    public nint SetDesktopBackgroundToWindow(nint dest, int width, int height)
    {
        lock (desktopThumbnails)
        {
            if (desktopThumbnails.Remove(dest, out var previous))
                Interop.DWMApi.DwmUnregisterThumbnail(previous);

            var progman = User32.FindWindow("Progman", null);
            if (Interop.DWMApi.DwmRegisterThumbnail(dest, progman, out var thumbnail) != 0)
                return 0;

            desktopThumbnails.Add(dest, thumbnail);
            BackgroundUpdate(thumbnail, width, height);
            return thumbnail;
        }
    }

    public void BackgroundUpdate(nint dest, int width, int height)
    {
        if (dest == 0 || Interop.DWMApi.DwmQueryThumbnailSourceSize(dest, out var size) != 0)
            return;

        var props = new Interop.DWMApi.DWM_THUMBNAIL_PROPERTIES
        {
            fVisible = true,
            dwFlags = Interop.DWMApi.DWM_TNP_VISIBLE | Interop.DWMApi.DWM_TNP_RECTDESTINATION | Interop.DWMApi.DWM_TNP_OPACITY,
            opacity = 255,
            rcDestination = new Interop.DWMApi.RECT(0, 0, Math.Min(width, size.x), Math.Min(height, size.y)),
        };
        Interop.DWMApi.DwmUpdateThumbnailProperties(dest, ref props);
    }

    public void ReleaseBackground(nint dest)
    {
        if (dest == 0)
            return;
        lock (desktopThumbnails)
        {
            var key = desktopThumbnails.FirstOrDefault(item => item.Value == dest).Key;
            if (key != 0)
                desktopThumbnails.Remove(key);
            Interop.DWMApi.DwmUnregisterThumbnail(dest);
        }
    }

    void ReleaseDesktopThumbnails()
    {
        lock (desktopThumbnails)
        {
            foreach (var thumbnail in desktopThumbnails.Values)
                Interop.DWMApi.DwmUnregisterThumbnail(thumbnail);
            desktopThumbnails.Clear();
        }
    }
}
#endif
