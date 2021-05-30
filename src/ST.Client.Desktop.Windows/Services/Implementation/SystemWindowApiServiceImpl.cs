using System.Application.Models;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed class SystemWindowApiServiceImpl : ISystemWindowApiService
    {
        /// <summary>
        /// 拖拽指针获取目标窗口
        /// </summary>
        /// <param name="action">目标窗口回调</param>
        public void GetMoveMouseDownWindow(Action<HandleWindow> action)
        {
            void MouseHook_OnMouseUp(object? sender, PointDouble p)
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
                    var window = new HandleWindow
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
                    var errorMessage = e.GetAllMessage();
                    Toast.Show(errorMessage);
                }
            }
            MouseHook.SetSystemCursor(MouseHook.LoadCursor(IntPtr.Zero, MouseHook.OCR_CROSS), MouseHook.OCR_NORMAL);
            MouseHook.OnMouseUp += MouseHook_OnMouseUp;
        }

        /// <summary>
        /// 将传入窗口设置为无边框窗口化
        /// </summary>
        /// <param name="window"></param>
        public void BorderlessWindow(HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                int p1 = User32Window.GetWindowLongA(window.Handle, (int)WindowLongFlags.GWL_STYLE);
                p1 &= ~13500416;
                User32Window.SetWindowLong(window.Handle, (int)WindowLongFlags.GWL_STYLE, (int)p1);
            }
        }

        public void MaximizeWindow(HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                //最大化启动的程序
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_MAXIMIZE);
            }
        }

        public void NormalWindow(HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_RESTORE);
                int p1 = User32Window.GetWindowLongA(window.Handle, (int)WindowLongFlags.GWL_STYLE);
                User32Window.SetWindowLong(window.Handle, (int)WindowLongFlags.GWL_STYLE, (int)p1);
            }
        }

        public void ShowWindow(HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_SHOW);
            }
        }

        public void HideWindow(HandleWindow window)
        {
            if (!window.IsHasProcessExits())
            {
                User32Window.ShowWindow(window.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_HIDE);
            }
        }

        public void ToWallerpaperWindow(HandleWindow window)
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
                User32Window.SetParent(window.Handle, workerw);


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

        public void ResetWallerpaper()
        {
            //string result = string.Empty;
            IntPtr p = IntPtr.Zero;
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_GETDESKWALLPAPER, 300, p, (uint)MouseHook.SystemParamtersInfoFlags.None);
            //result = Marshal.PtrToStringAuto(p);  //默认桌面路径
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_SETDESKWALLPAPER, 1, p, (uint)MouseHook.SystemParamtersInfoFlags.SPIF_UPDATEINIFILE | (uint)MouseHook.SystemParamtersInfoFlags.SPIF_SENDWININICHANGE);
            //Marshal.FreeHGlobal(p);
        }


        public void SetActiveWindow(HandleWindow window) 
        {
            User32Window.SetActiveWindow(window.Handle);
            User32Window.SetForegroundWindow(window.Handle);
        }
    }
}
