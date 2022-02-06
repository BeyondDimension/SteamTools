using System.Application.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using static UnmanagedMethods;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows7.0")]
    internal sealed class NativeWindowApiServiceImpl : INativeWindowApiService
    {
        const string TAG = "NativeWindowApiS";

        /// <summary>
        /// 拖拽指针获取目标窗口
        /// </summary>
        /// <param name="action">目标窗口回调</param>
        public void GetMoveMouseDownWindow(Action<NativeWindowModel> action)
        {
            void MouseHook_OnMouseUp(object? sender, Point p)
            {
                //Point GetMousePos = Mouse.GetPosition(App.Current.MainWindow);
                //Name = System.Windows.Forms.Cursor.Position.X + "," + System.Windows.Forms.Cursor.Position.Y;
                MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_SETCURSORS, 0, IntPtr.Zero, (uint)MouseHook.SystemParametersDesktopInfo.SPIF_SENDWININICHANGE);
                MouseHook.OnMouseUp -= MouseHook_OnMouseUp;
                //Name = Control.MousePosition.X + "," + Control.MousePosition.Y;
                MouseHook.GetCursorPos(out var pointInt);
                var handle = MouseHook.WindowFromPoint(pointInt);
                var title = new StringBuilder(256);
                User32Window.GetWindowText(handle, title, title.Capacity);//得到窗口的标题            
                var className = new StringBuilder(256);
                User32Window.GetClassName(handle, className, className.Capacity);//得得到窗口的句柄 类名
                //SelectWindow.Title = title.ToString();
                //SelectWindow.ClassName = className.ToString();
                User32Window.GetWindowThreadProcessId(handle, out int pid);
                //SelectWindow.Process = Process.GetProcessById(pid);
                try
                {
                    var process = Process.GetProcessById(pid);
                    string? path = null;
                    if (process != null)
                    {
                        try
                        {
                            path = process.MainModule?.FileName;
                        }
                        catch // 32位进程无法访问64位进程
                        {
                            path = NativeMethods.QueryFullProcessImageName(process);
                        }
                    }
                    var window = new NativeWindowModel
                    {
                        Title = title.ToString(),
                        ClassName = className.ToString(),
                        Handle = handle,
                        Process = process,
                        Path = path,
                        Name = process?.ProcessName,
                    };
                    action?.Invoke(window);
                }
                catch (Exception e)
                {
                    e.LogAndShowT(TAG);
                }
            }
            MouseHook.SetSystemCursor(MouseHook.LoadCursor(IntPtr.Zero, MouseHook.OCR_CROSS), MouseHook.OCR_NORMAL);
            MouseHook.OnMouseUp += MouseHook_OnMouseUp;
        }

        /// <summary>
        /// 将传入窗口设置为无边框窗口化
        /// </summary>
        /// <param name="window"></param>
        public void BorderlessWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                int p1 = User32Window.GetWindowLongA(window.Handle, (int)WindowLongFlags.GWL_STYLE);
                p1 &= ~13500416;
                User32Window.SetWindowLong(window.Handle, (int)WindowLongFlags.GWL_STYLE, p1);
            }
        }

        public void MaximizeWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                //最大化启动的程序
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_MAXIMIZE);
            }
        }

        public void NormalWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_RESTORE);
                int p1 = User32Window.GetWindowLongA(window.Handle, (int)WindowLongFlags.GWL_STYLE);
                User32Window.SetWindowLong(window.Handle, (int)WindowLongFlags.GWL_STYLE, p1);
            }
        }

        public void ShowWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_SHOW);
            }
        }

        public void HideWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_HIDE);
            }
        }

        public void ToWallerpaperWindow(NativeWindowModel window)
        {
            if (!window.IsHasProcessExits())
            {
                //最大化启动的程序
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_RESTORE);
                IntPtr progman = User32Window.FindWindow("Progman", null);
                IntPtr result = User32Window.SendMessage(progman, 0x052C, new IntPtr(0), IntPtr.Zero);
                IntPtr workerw = IntPtr.Zero;

                User32Window.EnumWindows(((tophandle, topparamhandle) =>
                {
                    IntPtr p = User32Window.FindWindowEx(tophandle,
                        IntPtr.Zero,
                        "SHELLDLL_DefView",
                        null);

                    if (p != IntPtr.Zero)
                    {
                        workerw = User32Window.FindWindowEx(IntPtr.Zero,
                            tophandle,
                            "WorkerW",
                            null);
                    }
                    return true;
                }), IntPtr.Zero);
                SetParentWindow(window.Handle, workerw);


                int p1 = User32Window.GetWindowLongA(window.Handle, (int)WindowLongFlags.GWL_STYLE);
                p1 &= ~13500416;
                User32Window.SetWindowLong(window.Handle, (int)WindowLongFlags.GWL_STYLE, p1);

                //最大化启动的程序
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_MAXIMIZE);
                //User32Window.MoveWindow(window.Handle, 0, 0, Screen.AllScreens[0].WorkingArea.Width,
                //    Screen.AllScreens[0].WorkingArea.Height, false);

                User32Window.SetActiveWindow(window.Handle);
            }
        }

        public string? GetWallerpaperImagePath()
        {
            IntPtr p = IntPtr.Zero;
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_GETDESKWALLPAPER, 300, p, (uint)MouseHook.SystemParamtersInfoFlags.None);
            var result = Marshal.PtrToStringAuto(p);  //默认桌面路径
            Marshal.FreeHGlobal(p);
            return result;
        }

        public void ResetWallerpaper()
        {
            //string result = string.Empty;
            IntPtr p = IntPtr.Zero;
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_GETDESKWALLPAPER, 300, p, (uint)MouseHook.SystemParamtersInfoFlags.None);
            //result = Marshal.PtrToStringAuto(p);  //默认桌面路径
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_SETDESKWALLPAPER, 1, p, (uint)MouseHook.SystemParamtersInfoFlags.SPIF_UPDATEINIFILE | (uint)MouseHook.SystemParamtersInfoFlags.SPIF_SENDWININICHANGE);
            Marshal.FreeHGlobal(p);
        }

        public void SetParentWindow(IntPtr source, IntPtr dest)
        {
            User32Window.SetParent(source, dest);
            //User32Window.SetWindowLong(source, (int)WindowLongParam.GWL_HWNDPARENT, (int)dest);
        }

        /// <summary>
        /// 激活窗口并置顶
        /// </summary>
        /// <param name="window"></param>
        public void SetActiveWindow(NativeWindowModel window)
        {
            User32Window.SetActiveWindow(window.Handle);
            User32Window.SetForegroundWindow(window.Handle);
        }

        /// <summary>
        /// 设置窗口点击穿透
        /// </summary>
        /// <param name="window"></param>
        public void SetWindowPenetrate(IntPtr dest)
        {
            int style = User32Window.GetWindowLongA(dest, (int)WindowLongFlags.GWL_EXSTYLE);
            User32Window.SetWindowLong(dest, (int)WindowLongFlags.GWL_EXSTYLE, style | (int)UnmanagedMethods.WindowStyles.WS_EX_TRANSPARENT | (int)UnmanagedMethods.WindowStyles.WS_EX_LAYERED);
            User32Window.SetLayeredWindowAttributes(dest, 0, 255, 0x2);
            //User32Window.SetLayeredWindowAttributes(dest, 0, 100, 0);
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

            IntPtr p = User32Window.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);

            ReleaseBackground(dest);

            var temp = DWMApi.DwmRegisterThumbnail(dest, p, out dest);

            if (temp == 0)
                BackgroundUpdate(dest, width, height);
            return dest;
        }

        public void BackgroundUpdate(IntPtr dest, int width, int height)
        {
            if (dest == IntPtr.Zero)
                return;

            DWMApi.DwmQueryThumbnailSourceSize(dest, out PSIZE size);

            var props = new DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = DWMApi.DWM_TNP_VISIBLE | DWMApi.DWM_TNP_RECTDESTINATION | DWMApi.DWM_TNP_OPACITY,
                opacity = 255,
                rcDestination = new RECT(0, 0, width, height),
            };

            if (size.x < width)
                props.rcDestination.Right = props.rcDestination.Left + size.x;

            if (size.y < height)
                props.rcDestination.Bottom = props.rcDestination.Top + size.y;

            DWMApi.DwmUpdateThumbnailProperties(dest, ref props);
        }

        public void ReleaseBackground(IntPtr dest)
        {
            if (dest != IntPtr.Zero)
                DWMApi.DwmUnregisterThumbnail(dest);
        }
    }
}
