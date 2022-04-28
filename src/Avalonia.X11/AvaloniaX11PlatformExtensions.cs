using System;
using Avalonia.Controls;

namespace Avalonia
{
    public static class AvaloniaX11PlatformExtensions
    {
        public static T UseX11<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            throw new PlatformNotSupportedException();
        }
    }
}
