using System;
using Avalonia.Controls;

namespace Avalonia
{
    public static class Win32ApplicationExtensions
    {
        public static T UseWin32<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            throw new PlatformNotSupportedException();
        }
    }
}
