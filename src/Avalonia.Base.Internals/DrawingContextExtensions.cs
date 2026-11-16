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
        var platform = context.PlatformImpl;
        platform.PushRenderOptions(new RenderOptions
        {
            BitmapInterpolationMode = interpolationMode,
            BitmapBlendingMode = bitmapBlending,
        });
        try
        {
            platform.DrawBitmap(source.PlatformImpl.Item, opacity, sourceRect, destRect);
        }
        finally
        {
            platform.PopRenderOptions();
        }
    }
}
