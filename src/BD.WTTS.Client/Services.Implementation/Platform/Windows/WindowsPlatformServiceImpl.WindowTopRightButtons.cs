#if WINDOWS
using PInvoke;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    // 窗口右上角的三个按钮(最小化，最大化，关闭)

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
        var value = User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE);
        if (isVisible)
        {
            value |= WS_SYSMENU;
        }
        else
        {
            value &= ~WS_SYSMENU;
        }
        _ = User32.SetWindowLong(hWnd,
            User32.WindowLongIndexFlags.GWL_STYLE,
            (User32.SetWindowLongFlags)value);
    }

    public const int WS_MAXIMIZEBOX = 0x10000;
    public const int WS_MINIMIZEBOX = 0x20000;
    public const int WS_CAPTION = 0x00C00000;
    public const int WS_BORDER = 0x00800000;
    public const int WS_SYSMENU = 0x80000;

    public static void EnableMinimizeButton(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) | WS_MINIMIZEBOX));
    }

    public static void EnableMaximizeButton(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) | WS_MAXIMIZEBOX));
    }

    public static void ShowMinimizeAndMaximizeButtons(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) | WS_MAXIMIZEBOX | WS_MINIMIZEBOX));
    }

    public static void ShowAllButtons(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) | WS_SYSMENU));
    }

    public static void HideMinimizeAndMaximizeButtons(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
    }

    public static void DisableMinimizeButton(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) & ~WS_MINIMIZEBOX));
    }

    public static void DisableMaximizeButton(IntPtr hWnd)
    {
        _ = User32.SetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE, (User32.SetWindowLongFlags)(User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE) & ~WS_MAXIMIZEBOX));
    }

    public void SetResizeMode(IntPtr hWnd, ResizeMode value)
    {
        switch (value)
        {
            case ResizeMode.NoResize:
                _ = Interop.User32.DeleteMenu(User32.GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                _ = Interop.User32.DeleteMenu(User32.GetSystemMenu(hWnd, false), SC_MINIMIZE, MF_BYCOMMAND);
                HideMinimizeAndMaximizeButtons(hWnd);
                break;
            case ResizeMode.CanMinimize:
                _ = Interop.User32.DeleteMenu(User32.GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                DisableMaximizeButton(hWnd);
                break;
        }
    }
}
#endif