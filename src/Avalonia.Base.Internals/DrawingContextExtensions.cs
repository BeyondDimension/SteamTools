using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia;

public static class DrawingContextExtensions
{
    public static void DrawBitmap2(this DrawingContext context, Bitmap source, double opacity, Rect sourceRect, Rect destRect)
    {
        context.DrawBitmap(source.PlatformImpl, opacity, sourceRect, destRect);
    }

    public static void DrawBitmap2(this ImmediateDrawingContext context, Bitmap source, double opacity, Rect sourceRect, Rect destRect,
        BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality,
        BitmapBlendingMode bitmapBlending = BitmapBlendingMode.Source)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        context.PlatformImpl.DrawBitmap2(source.PlatformImpl.Item, opacity, sourceRect, destRect, interpolationMode, bitmapBlending);
    }
}
