extern alias AvaloniaSkia;

using System.IO;
using SkiaSharp;

namespace Avalonia.Skia
{
    public static class SkiaSharpHelpers
    {
        public static SKCodec Create(this Stream stream)
        {
            if (stream is IFileStreamWrapper fileStreamWrapper)
            {
                return SKCodec.Create(fileStreamWrapper.Name);
            }
            else if (stream is FileStream fileStream)
            {
                var filename = fileStream.Name;
                fileStream.Dispose();
                return SKCodec.Create(filename);
            }
            else
            {
                using var skStream = new SKManagedStream(stream);
                return SKCodec.Create(skStream);
            }
        }

        public static SKImage FromEncodedData(this Stream stream)
        {
            if (stream is IFileStreamWrapper fileStreamWrapper)
            {
                return SKImage.FromEncodedData(fileStreamWrapper.Name);
            }
            else if (stream is FileStream fileStream)
            {
                var filename = fileStream.Name;
                fileStream.Dispose();
                return SKImage.FromEncodedData(filename);
            }
            else
            {
                using var skStream = new SKManagedStream(stream);
                using var data = SKData.Create(skStream);
                return SKImage.FromEncodedData(data);
            }
        }
    }
}
