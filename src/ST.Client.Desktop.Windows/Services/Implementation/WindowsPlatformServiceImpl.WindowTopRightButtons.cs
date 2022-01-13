using System.Runtime.InteropServices;
using System.Windows;

namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl
    {
        /// <inheritdoc cref="IPlatformService.DwmIsCompositionEnabled"/>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled();

        bool IPlatformService.DwmIsCompositionEnabled => DwmIsCompositionEnabled();

        #region 窗口右上角的三个按钮(最小化，最大化，关闭)

        // https://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
        // https://blog.magnusmontin.net/2014/11/30/disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-wpf-window/
        // https://straub.as/csharp/wpf/systembuttons.html

        public const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        //public const int CS_DROPSHADOW = 0x20000;

        void IPlatformService.SetWindowSystemButtonsIsVisible(IntPtr hWnd, bool isVisible)
        {
            // https://stackoverflow.com/questions/5114389/how-make-sure-aero-effect-is-enabled
            var value = GetWindowLong(hWnd, GWL_STYLE);
            if (isVisible)
            {
                value |= WS_SYSMENU;
            }
            else
            {
                value &= ~WS_SYSMENU;
            }
            _ = SetWindowLong(hWnd, GWL_STYLE, value);
        }

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        public const int GWL_STYLE = -16,
                    WS_MAXIMIZEBOX = 0x10000,
                    WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        public const int WS_SYSMENU = 0x80000;

        public static void EnableMinimizeButton(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MINIMIZEBOX);
        }

        public static void EnableMaximizeButton(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MAXIMIZEBOX);
        }

        public static void ShowMinimizeAndMaximizeButtons(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_MAXIMIZEBOX | WS_MINIMIZEBOX);
        }

        public static void ShowAllButtons(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) | WS_SYSMENU);
        }

        public static void HideMinimizeAndMaximizeButtons(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        public static void DisableMinimizeButton(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }

        public static void DisableMaximizeButton(IntPtr hWnd)
        {
            _ = SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        }

        public void SetResizeMode(IntPtr hWnd, ResizeMode value)
        {
            switch (value)
            {
                case ResizeMode.NoResize:
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MINIMIZE, MF_BYCOMMAND);
                    HideMinimizeAndMaximizeButtons(hWnd);
                    break;
                case ResizeMode.CanMinimize:
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    DisableMaximizeButton(hWnd);
                    break;
            }
        }

        #endregion
    }
}