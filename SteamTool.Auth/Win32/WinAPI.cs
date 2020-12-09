using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamTool.Auth.Win32
{
    public class WinAPI
    {
        public const string LibraryName = "user32.dll";

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetOpenClipboardWindow();

        [DllImport(LibraryName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(LibraryName, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    }
}
