using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Utilities;

namespace Avalonia;

public static class DrawingContextExtensions
{
    public static void DrawBitmap2(this DrawingContext context, object source, double opacity, Rect sourceRect, Rect destRect)
    {
        if (source is IBitmap bitmap)
            context.DrawBitmap(bitmap.PlatformImpl, opacity, sourceRect, destRect);
    }
}
