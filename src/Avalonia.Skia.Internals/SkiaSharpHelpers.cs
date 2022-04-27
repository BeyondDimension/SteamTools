extern alias AvaloniaSkia;

using System.IO;
using SkiaSharp;
using Avalonia.Media.Imaging;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;

namespace Avalonia.Skia
{
    public static class SkiaSharpHelpers
    {
        public static SKCodec Create(Stream stream)
        {
            if (stream is IFileStreamWrapper fileStreamWrapper)
            {
                return SKCodec.Create(fileStreamWrapper.Name);
            }
            else if (stream is FileStream fileStream)
            {
                var filename = fileStream.Name;
                return SKCodec.Create(filename);
            }
            else
            {
                using var skStream = new SKManagedStream(stream);
                return SKCodec.Create(skStream);
            }
        }

        public static SKImage FromEncodedData(Stream stream)
        {
            if (stream is IFileStreamWrapper fileStreamWrapper)
            {
                return SKImage.FromEncodedData(fileStreamWrapper.Name);
            }
            else if (stream is FileStream fileStream)
            {
                var filename = fileStream.Name;
                return SKImage.FromEncodedData(filename);
            }
            else
            {
                using var skStream = new SKManagedStream(stream);
                using var data = SKData.Create(skStream);
                return SKImage.FromEncodedData(data);
            }
        }

        static Bitmap GetBitmap(this ImmutableBitmap bitmap) => new(RefCountable.Create(bitmap));

        public static Bitmap GetBitmap(SKImage image) => new ImmutableBitmap(image).GetBitmap();

        public static Bitmap GetBitmap(SKBitmap bitmap)
        {
            var image = SKImage.FromBitmap(bitmap);
            return GetBitmap(image);
        }

        public static Bitmap DecodeToWidth(SKBitmap bitmap, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => new ImmutableBitmap(bitmap, width, true, interpolationMode).GetBitmap();
    }
}
