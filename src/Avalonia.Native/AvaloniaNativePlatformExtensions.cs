using System;
using Avalonia.Controls;

namespace Avalonia
{
    public static class AvaloniaNativePlatformExtensions
    {
        public static T UseAvaloniaNative<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            throw new PlatformNotSupportedException();
        }
    }
}