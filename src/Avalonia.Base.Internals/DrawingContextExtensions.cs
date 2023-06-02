using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Utilities;

namespace Avalonia;

public static class DrawingContextExtensions
{
    public static void DrawBitmap2(this DrawingContext context, Bitmap source, double opacity, Rect sourceRect, Rect destRect)
    {
        context.DrawBitmap(source.PlatformImpl, opacity, sourceRect, destRect);
    }
}
