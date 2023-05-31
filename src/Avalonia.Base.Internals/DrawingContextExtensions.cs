using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Utilities;

namespace Avalonia;

public static class DrawingContextExtensions
{
    public static void DrawBitmap2(this DrawingContext context, IRef<IBitmapImpl> source, double opacity, Rect sourceRect, Rect destRect)
    {
        context.DrawBitmap(source, opacity, sourceRect, destRect);
    }
}
