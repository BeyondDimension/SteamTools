#if WINDOWS
using PInvoke;
using System.Windows;
using Vanara.PInvoke;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 拖拽指针获取目标窗口
    /// </summary>
    /// <param name="updateWindowCallback">目标窗口回调</param>
    public void GetMoveMouseDownWindow(Action<NativeWindowModel> updateWindowCallback)
    {
        void MouseHook_OnMouseUp(object? sender, PointD p)
        {
            //Point GetMousePos = Mouse.GetPosition(App.Current.MainWindow);
            //Name = System.Windows.Forms.Cursor.Position.X + "," + System.Windows.Forms.Cursor.Position.Y;
            User32.SystemParametersInfo(
                User32.SystemParametersInfoAction.SPI_SETCURSORS,
                0,
                nint.Zero,
                User32.SystemParametersInfoFlags.SPIF_SENDWININICHANGE);
            MouseHook.OnMouseUp -= MouseHook_OnMouseUp;
            //Name = Control.MousePosition.X + "," + Control.MousePosition.Y;
            User32.GetCursorPos(out var pointInt);
            var handle = User32.WindowFromPoint(pointInt);
            var title = User32.GetWindowText(handle); // 得到窗口的标题
            var className = User32.GetClassName(handle);
            // 得到窗口的句柄 类名
            //SelectWindow.Title = title.ToString();
            //SelectWindow.ClassName = className.ToString();
            User32.GetWindowThreadProcessId(handle, out int pid);
            //SelectWindow.Process = Process.GetProcessById(pid);
            try
            {
                var process = Process.GetProcessById(pid);
                if (process.Id == Environment.ProcessId)
                {
                    updateWindowCallback?.Invoke(null!);
                    return;
                }

                string? path = null;
                if (process != null)
                {
                    try
                    {
                        path = process.MainModule?.FileName;
                    }
                    catch // 32 位进程无法访问 64 位进程
                    {
                        path = Interop.Kernel32.QueryFullProcessImageName(process);
                    }
                }

                var window = new NativeWindowModel(handle,
                    new string(title),
                    new string(className),
                    process,
                    path,
                    process?.ProcessName);
                updateWindowCallback?.Invoke(window);
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
        }
        MouseHook.SetSystemCursor(User32.LoadCursor(IntPtr.Zero, MouseHook.OCR_CROSS).DangerousGetHandle(), MouseHook.OCR_NORMAL);
        MouseHook.OnMouseUp += MouseHook_OnMouseUp;
    }

    /// <summary>
    /// 将传入句柄窗口设置无标题栏和标题栏区域按钮
    /// </summary>
    /// <param name="window"></param>
    public void BeautifyTheWindow(nint hWnd)
    {
        if (hWnd != nint.Zero)
        {
            var windowStyle = User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE);

            // 移除最大化、关闭和最小化按钮
            windowStyle = windowStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX & ~WS_CAPTION;

            User32.SetWindowLongPtr(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, windowStyle);
        }
    }

    /// <summary>
    /// 将传入窗口设置为无边框窗口化
    /// </summary>
    /// <param name="window"></param>
    public void BorderlessWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            int p1 = User32.GetWindowLong(window.Handle,
                User32.WindowLongIndexFlags.GWL_STYLE);
            p1 &= ~13500416;
            User32.SetWindowLong(window.Handle,
                User32.WindowLongIndexFlags.GWL_STYLE,
                (User32.SetWindowLongFlags)p1);
        }
    }

    public void MaximizeWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            // 最大化启动的程序
            User32.ShowWindow(window.Handle,
                User32.WindowShowStyle.SW_MAXIMIZE);
        }
    }

    public void NormalWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_RESTORE);
            int p1 = User32.GetWindowLong(window.Handle,
                User32.WindowLongIndexFlags.GWL_STYLE);
            User32.SetWindowLong(window.Handle,
                User32.WindowLongIndexFlags.GWL_STYLE,
                (User32.SetWindowLongFlags)p1);
        }
    }

    public void ShowWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_SHOW);
        }
    }

    public void HideWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_HIDE);
        }
    }

    public void ToWallerpaperWindow(NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            // 最大化启动的程序
            User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_RESTORE);
            nint progman = User32.FindWindow("Progman", null);
            nint result = User32.SendMessage(progman, (User32.WindowMessage)0x052C, nint.Zero, nint.Zero);
            nint workerw = nint.Zero;

            User32.EnumWindows((tophandle, topparamhandle) =>
            {
                nint p = User32.FindWindowEx(tophandle,
                    nint.Zero,
                    "SHELLDLL_DefView",
                    null);

                if (p != nint.Zero)
                {
                    workerw = User32.FindWindowEx(nint.Zero,
                        tophandle,
                        "WorkerW",
                        null);
                }
                return true;
            }, nint.Zero);
            SetParentWindow(window.Handle, workerw);

            int p1 = User32.GetWindowLong(window.Handle, User32.WindowLongIndexFlags.GWL_STYLE);
            p1 &= ~13500416;
            User32.SetWindowLong(window.Handle, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)p1);

            // 最大化启动的程序
            User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_MAXIMIZE);
            //User32.MoveWindow(window.Handle, 0, 0, Screen.AllScreens[0].WorkingArea.Width,
            //    Screen.AllScreens[0].WorkingArea.Height, false);

            Interop.User32.SetActiveWindow(window.Handle);
        }
    }

    public string? GetWallerpaperImagePath()
    {
        IntPtr p = IntPtr.Zero;
        User32.SystemParametersInfo(
            User32.SystemParametersInfoAction.SPI_GETDESKWALLPAPER,
            300,
            p,
            User32.SystemParametersInfoFlags.None);
        var result = Marshal.PtrToStringAuto(p);  //默认桌面路径
        Marshal.FreeHGlobal(p);
        return result;
    }

    public void ResetWallerpaper()
    {
        //string result = string.Empty;
        IntPtr p = IntPtr.Zero;
        User32.SystemParametersInfo(
            User32.SystemParametersInfoAction.SPI_GETDESKWALLPAPER,
            300,
            p,
            User32.SystemParametersInfoFlags.None);
        //result = Marshal.PtrToStringAuto(p);  //默认桌面路径
        User32.SystemParametersInfo(
            User32.SystemParametersInfoAction.SPI_SETDESKWALLPAPER,
            1,
            p,
            User32.SystemParametersInfoFlags.SPIF_UPDATEINIFILE | User32.SystemParametersInfoFlags.SPIF_SENDWININICHANGE);
        Marshal.FreeHGlobal(p);
    }

    public void SetParentWindow(IntPtr source, IntPtr dest)
    {
        User32.SetParent(source, dest);
        //PInvoke.User32.SetWindowLong(source, (int)WindowLongParam.GWL_HWNDPARENT, (int)dest);
    }

    /// <summary>
    /// 激活窗口并置顶
    /// </summary>
    /// <param name="window"></param>
    public void SetActiveWindow(NativeWindowModel window)
    {
        Interop.User32.SetActiveWindow(window.Handle);
        User32.SetForegroundWindow(window.Handle);
    }

    /// <summary>
    /// 设置窗口点击穿透
    /// </summary>
    /// <param name="dest"></param>
    public void SetWindowPenetrate(IntPtr dest)
    {
        var style = (User32.SetWindowLongFlags)User32.GetWindowLong(dest, User32.WindowLongIndexFlags.GWL_EXSTYLE);
        User32.SetWindowLong(dest,
            User32.WindowLongIndexFlags.GWL_EXSTYLE,
            style | User32.SetWindowLongFlags.WS_EX_TRANSPARENT | User32.SetWindowLongFlags.WS_EX_LAYERED);
        Interop.User32.SetLayeredWindowAttributes(dest, 0, 255, 0x2);
        //PInvoke.User32.SetLayeredWindowAttributes(dest, 0, 100, 0);
    }

    /// <summary>
    /// 设置缩略图到指定窗口句柄
    /// </summary>
    public IntPtr SetDesktopBackgroundToWindow(IntPtr dest, int width, int height)
    {
        //backgroundPath = null;
        //IntPtr wep = IntPtr.Zero;
        //User32Window.EnumWindows(((hwnd, e) =>
        //{
        //    IntPtr p = User32Window.FindWindowEx(hwnd,
        //          IntPtr.Zero,
        //          "SHELLDLL_DefView",
        //          null);
        //    if (p != IntPtr.Zero)
        //    {
        //        IntPtr workerw = User32Window.FindWindowEx(IntPtr.Zero,
        //            hwnd,
        //            "WorkerW",
        //            null);

        //        User32Window.EnumChildWindows(workerw, ((hwnd2, e2) =>
        //        {
        //            wep = hwnd2;
        //            return true;
        //        }), IntPtr.Zero);
        //    }
        //    return true;
        //}), IntPtr.Zero);

        //if (wep == IntPtr.Zero)
        //{
        //    IntPtr r = IntPtr.Zero;
        //    MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_GETDESKWALLPAPER, 300, r, (uint)MouseHook.SystemParamtersInfoFlags.None);

        //    backgroundPath = Marshal.PtrToStringAuto(r);  //默认桌面路径
        //    return;
        //}
        //User32Window.SetWindowPos(dest, HWND_BOTTOM, 0, 0, 0, 0,
        //    SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);

        nint p = User32.FindWindowEx(nint.Zero, nint.Zero, "Progman", null);

        ReleaseBackground(dest);

        var temp = Interop.DWMApi.DwmRegisterThumbnail(dest, p, out dest);

        if (temp == 0)
            BackgroundUpdate(dest, width, height);
        return dest;
    }

    public void BackgroundUpdate(IntPtr dest, int width, int height)
    {
        if (dest == IntPtr.Zero)
            return;

        Interop.DWMApi.DwmQueryThumbnailSourceSize(dest, out Interop.DWMApi.PSIZE size);

        var props = new Interop.DWMApi.DWM_THUMBNAIL_PROPERTIES
        {
            fVisible = true,
            dwFlags = Interop.DWMApi.DWM_TNP_VISIBLE | Interop.DWMApi.DWM_TNP_RECTDESTINATION | Interop.DWMApi.DWM_TNP_OPACITY,
            opacity = 255,
            rcDestination = new Interop.DWMApi.RECT(0, 0, width, height),
        };

        if (size.x < width)
            props.rcDestination.Right = props.rcDestination.Left + size.x;

        if (size.y < height)
            props.rcDestination.Bottom = props.rcDestination.Top + size.y;

        Interop.DWMApi.DwmUpdateThumbnailProperties(dest, ref props);
    }

    public void ReleaseBackground(IntPtr dest)
    {
        if (dest != IntPtr.Zero)
            Interop.DWMApi.DwmUnregisterThumbnail(dest);
    }
}
#endif