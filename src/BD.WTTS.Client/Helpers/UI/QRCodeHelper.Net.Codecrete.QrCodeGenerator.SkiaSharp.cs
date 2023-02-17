/* QR code generator library (.NET)
 * https://github.com/manuelbl/QrCodeGenerator
 * Copyright (c) 2021 Manuel Bleichenbacher
 * Licensed under MIT License
 * https://opensource.org/licenses/MIT
 */

using SkiaSharp;

// ReSharper disable once CheckNamespace
namespace Net.Codecrete.QrCodeGenerator;

static class QrCodeBitmapExtensions
{
    public static SKBitmap ToBitmap(this QrCode qrCode, int scale, int border, SKColor foreground, SKColorType colorType = SKColorType.Argb4444, SKAlphaType alphaType = SKAlphaType.Premul)
    {
        // check arguments
        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "Value out of range");
        }
        if (border < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(border), "Value out of range");
        }

        int size = qrCode.Size;
        int dim = (size + (border * 2)) * scale;

        if (dim > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale or border too large");
        }

        // create bitmap
        SKBitmap bitmap = new(dim, dim, colorType, alphaType);

        using (SKCanvas canvas = new(bitmap))
        {
            // draw background
            //using (SKPaint paint = new SKPaint { Color = background })
            //{
            //    canvas.DrawRect(0, 0, dim, dim, paint);
            //}

            // draw modules
            using SKPaint paint = new() { Color = foreground };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (qrCode.GetModule(x, y))
                    {
                        canvas.DrawRect((x + border) * scale, (y + border) * scale, scale, scale, paint);
                    }
                }
            }
        }

        return bitmap;
    }
}