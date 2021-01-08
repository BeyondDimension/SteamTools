using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using SteamTools.Win32;
using MetroRadiance.Interop.Win32;
using System.Diagnostics;
using SteamTool.Core.Common;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SteamTools.ViewModels
{
    public class GameCommonViewModel : Livet.ViewModel
    {
        public class HandleWindow
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
            public string ClassName { get; set; }
            public Process Process { get; set; }
            public string Path => Process?.MainModule.FileName;
            public string Name => Process?.ProcessName;
        }

        #region SelectWindow 变更通知 

        private HandleWindow _SelectWindow;

        public HandleWindow SelectWindow
        {
            get { return this._SelectWindow; }
            set
            {
                if (this._SelectWindow != value)
                {
                    this._SelectWindow = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void Cross_MouseDown()
        {
            //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
            MouseHook.SetSystemCursor(MouseHook.LoadCursor(IntPtr.Zero, MouseHook.OCR_CROSS), MouseHook.OCR_NORMAL);
            MouseHook.OnMouseUp += MouseHook_OnMouseUp;
        }

        private void MouseHook_OnMouseUp(object sender, Point p)
        {
            //Point GetMousePos = Mouse.GetPosition(App.Current.MainWindow);
            //Name = System.Windows.Forms.Cursor.Position.X + "," + System.Windows.Forms.Cursor.Position.Y;
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_SETCURSORS, 0, IntPtr.Zero, (uint)MouseHook.SystemParametersDesktopInfo.SPIF_SENDWININICHANGE);
            MouseHook.OnMouseUp -= MouseHook_OnMouseUp;
            //Name = Control.MousePosition.X + "," + Control.MousePosition.Y;
            var handle = MouseHook.WindowFromPoint(new POINT(Control.MousePosition.X, Control.MousePosition.Y));
            var title = new StringBuilder(256);
            User32Window.GetWindowText(handle, title, title.Capacity);//得到窗口的标题            
            var className = new StringBuilder(256);
            User32Window.GetClassName(handle, className, className.Capacity);//得得到窗口的句柄 类名
            //SelectWindow.Title = title.ToString();
            //SelectWindow.ClassName = className.ToString();
            User32Window.GetWindowThreadProcessId(handle, out int pid);
            //SelectWindow.Process = Process.GetProcessById(pid);
            SelectWindow = new HandleWindow
            {
                Title = title.ToString(),
                ClassName = className.ToString(),
                Handle = handle,
                Process = Process.GetProcessById(pid)
            };
        }

        private bool IsHasProcessExits()
        {
            if (SelectWindow?.Process?.HasExited == false && SelectWindow.Name != Process.GetCurrentProcess().ProcessName)
            {
                return false;
            }
            return true;
        }

        public void BorderlessWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                int p1 = User32.GetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE);
                p1 &= ~13500416;
                User32.SetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE, (int)p1);
            }
        }

        public void MaximizeWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                //最大化启动的程序
                User32Window.ShowWindow(SelectWindow.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_MAXIMIZE);
            }
        }

        public void NormalWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                User32Window.ShowWindow(SelectWindow.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_RESTORE);
                int p1 = User32.GetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE);
                User32.SetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE, (int)p1);
            }
        }

        public void WindowKill_Click()
        {
            if (!IsHasProcessExits())
            {
                SelectWindow.Process.Kill();
            }
        }

        public void ShowWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                User32Window.ShowWindow(SelectWindow.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_SHOW);
            }
        }

        public void HideWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                User32Window.ShowWindow(SelectWindow.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_HIDE);
            }
        }

        public void ToWallerpaperWindow_Click()
        {
            if (!IsHasProcessExits())
            {
                //最大化启动的程序
                User32Window.ShowWindow(SelectWindow.Handle, (short)User32Window.Cmd_SHOWWINDOWS.SW_RESTORE);
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
                User32Window.SetParent(SelectWindow.Handle, workerw);


                int p1 = User32.GetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE);
                p1 &= ~13500416;
                User32.SetWindowLong(SelectWindow.Handle, (int)WindowLongFlags.GWL_STYLE, p1);
                User32Window.MoveWindow(SelectWindow.Handle, 0, 0, Screen.AllScreens[0].WorkingArea.Width,
                    Screen.AllScreens[0].WorkingArea.Height, false);
                User32.SetActiveWindow(SelectWindow.Handle);
            }
        }

        public void ResetWallerpaperWindow_Click()
        {
            //string result = string.Empty;
            IntPtr p = IntPtr.Zero;
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_GETDESKWALLPAPER, 300, p, (uint)MouseHook.SystemParamtersInfoFlags.None);
            //result = Marshal.PtrToStringAuto(p);  //默认桌面路径
            MouseHook.SystemParametersInfo((uint)MouseHook.SystemParametersDesktopInfo.SPI_SETDESKWALLPAPER, 1, p, (uint)MouseHook.SystemParamtersInfoFlags.SPIF_UPDATEINIFILE | (uint)MouseHook.SystemParamtersInfoFlags.SPIF_SENDWININICHANGE);
            //Marshal.FreeHGlobal(p);
        }
    }
}
