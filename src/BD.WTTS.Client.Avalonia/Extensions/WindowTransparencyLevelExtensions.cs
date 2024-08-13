// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static class WindowTransparencyLevelExtensions
{
    [Mobius(
"""
Mobius.Enums.UI.WindowTransparencyLevelExtensions
""")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WindowTransparencyLevel ToWindowTransparencyLevel(
        this WindowBackgroundMaterial value)
    {
        return value switch
        {
            WindowBackgroundMaterial.None => WindowTransparencyLevel.None,
            WindowBackgroundMaterial.Blur => WindowTransparencyLevel.Blur,
            WindowBackgroundMaterial.AcrylicBlur => WindowTransparencyLevel.AcrylicBlur,
            WindowBackgroundMaterial.Mica => WindowTransparencyLevel.Mica,
            _ => WindowTransparencyLevel.None,
        };
    }
}
