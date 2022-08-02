// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/wpf/blob/v6.0.6/src/Microsoft.DotNet.Wpf/src/Shared/MS/Win32/UnsafeNativeMethodsCLR.cs

#if !NETFRAMEWORK && WINDOWS

using System;
using System.Runtime.InteropServices;

namespace MS.Win32;

internal static class UnsafeNativeMethods
{
    [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetActiveWindow();

    [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
    public static extern int MessageBox(HandleRef hWnd, string text, string caption, int type);
}

#endif