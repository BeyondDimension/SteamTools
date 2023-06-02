namespace BD.WTTS.Extensions;

public static class WindowTransparencyLevelExtensions
{
    public static WindowTransparencyLevel ToWindowTransparencyLevel(this WindowBackgroundMaterial value)
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
