/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 *
 */

using SkiaSharp;
using System;

namespace Net.Codecrete.QrCodeGenerator
{
    public static class QrCodeBitmapExtensions
    {
        /// <inheritdoc cref="ToBitmap(QrCode, int, int)"/>
        /// <param name="background">The background color.</param>
        /// <param name="foreground">The foreground color.</param>
        public static SKBitmap ToBitmap(this QrCode qrCode, int scale, int border, SKColor foreground, SKColor background)
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
            int dim = (size + border * 2) * scale;

            if (dim > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale or border too large");
            }

            // create bitmap
            SKBitmap bitmap = new SKBitmap(dim, dim, SKColorType.Argb4444, SKAlphaType.Premul);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                // draw background
                using (SKPaint paint = new SKPaint { Color = background })
                {
                    canvas.DrawRect(0, 0, dim, dim, paint);
                }

                // draw modules
                using (SKPaint paint = new SKPaint { Color = foreground })
                {
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
            }

            return bitmap;
        }
    }
}