extern alias AvaloniaSkia;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaSkia::Avalonia.Skia;
using SkiaSharp;

namespace Avalonia;

public static class DrawingContextExtensions
{
    public static void DrawBitmap2(this IDrawingContextImpl context, IBitmapImpl source, double opacity,
        Rect sourceRect, Rect destRect, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality,
        BitmapBlendingMode bitmapBlending = BitmapBlendingMode.Source)
    {
        var drawableImage = (IDrawableBitmapImpl)source;
        var s = sourceRect.ToSKRect();
        var d = destRect.ToSKRect();

        var paint = SKPaintCache.Shared.Get();
        paint.Color = new SKColor(255, 255, 255, (byte)(255 * opacity * 1));
        paint.FilterQuality = interpolationMode.ToSKFilterQuality();
        paint.BlendMode = bitmapBlending.ToSKBlendMode();

        if (context is DrawingContextImpl c)
        {
            drawableImage.Draw(c, s, d, paint);
        }
        else
        {
            // CompositorDrawingContextProxy
            // https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Base/Rendering/Composition/Server/DrawingContextProxy.cs
            FieldInfo? implField = context.GetType().GetField("_impl", BindingFlags.NonPublic | BindingFlags.Instance);
            if (implField != null)
            {
                object? implValue = implField.GetValue(context);
                if (implValue != null && implValue is IDrawingContextImpl contextImpl)
                {
                    drawableImage.Draw((DrawingContextImpl)contextImpl, s, d, paint);
                }
            }
        }

        SKPaintCache.Shared.ReturnReset(paint);
    }
}
