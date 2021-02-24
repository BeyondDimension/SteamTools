using System.Application.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed class WindowsDesktopPlatformServiceImpl : IDesktopPlatformService
    {
        const string TAG = "WindowsDesktopPlatformS";

        #region 窗口右上角的三个按钮(最小化，最大化，关闭)

        // https://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
        // https://blog.magnusmontin.net/2014/11/30/disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-wpf-window/

        public const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        public const int GWL_STYLE = -16,
                    WS_MAXIMIZEBOX = 0x10000,
                    WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        public extern static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public extern static int SetWindowLong(IntPtr hwnd, int index, int value);

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

        public void SetResizeMode(IntPtr hWnd, int value)
        {
            switch (value)
            {
                case IDesktopPlatformService.ResizeMode_NoResize:
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MINIMIZE, MF_BYCOMMAND);
                    HideMinimizeAndMaximizeButtons(hWnd);
                    break;
                case IDesktopPlatformService.ResizeMode_CanMinimize:
                    _ = DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                    DisableMaximizeButton(hWnd);
                    break;
            }
        }

        #endregion

        public string GetCommandLineArgs(Process process)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                      "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                using var objects = searcher.Get();
                var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                return @object?["CommandLine"]?.ToString() ?? "";
            }
            catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                // 没有对该进程的安全访问权限。
                return string.Empty;
            }
        }

        static readonly Lazy<string> mHostsFilePath = new Lazy<string>(() =>
        {
            return Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
        });

        public string HostsFilePath => mHostsFilePath.Value;

        public string? GetFileName(TextReaderProvider provider)
        {
            switch (provider)
            {
                case TextReaderProvider.Notepad:
                    return "notepad";
                case TextReaderProvider.NotepadPlusPlus:
                    return "notepad++";
                case TextReaderProvider.VSCode:
                    var vsCodePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "Microsoft VS Code",
                        "Code.exe");
                    return File.Exists(vsCodePath) ? vsCodePath : null;
                default:
                    return null;
            }
        }
    }
}