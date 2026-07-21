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
        paint.BlendMode = bitmapBlending.ToSKBlendMode();
        var sampling = interpolationMode switch
        {
            BitmapInterpolationMode.None => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None),
            BitmapInterpolationMode.LowQuality => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
            BitmapInterpolationMode.MediumQuality => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Nearest),
            _ => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
        };

        if (context is DrawingContextImpl c)
        {
            drawableImage.Draw(c, s, d, sampling, paint);
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
                    drawableImage.Draw((DrawingContextImpl)contextImpl, s, d, sampling, paint);
                }
            }
        }

        SKPaintCache.Shared.ReturnReset(paint);
    }
}
