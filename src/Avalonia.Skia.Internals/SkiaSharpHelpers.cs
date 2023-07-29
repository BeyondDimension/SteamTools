//extern alias AvaloniaSkia;

//using SkiaSharp;
//using Avalonia.Media.Imaging;
//using Avalonia.Utilities;

//namespace Avalonia.Skia;

//public static class SkiaSharpHelpers
//{
//    static SKCodec CreateCore(IFileStreamWrapper fileStreamWrapper)
//    {
//        return SKCodec.Create(fileStreamWrapper.Name);
//    }

//    static SKCodec CreateCore(FileStream fileStream, bool disposeManagedStream = false)
//    {
//        try
//        {
//            var filename = fileStream.Name;
//            return SKCodec.Create(filename);
//        }
//        finally
//        {
//            if (disposeManagedStream) fileStream.Dispose();
//        }
//    }

//    static SKCodec CreateCore(Stream stream, bool disposeManagedStream = false)
//    {
//        using var skStream = new SKManagedStream(stream, disposeManagedStream);
//        return SKCodec.Create(skStream);
//    }

//    public static SKCodec Create(Stream stream)
//    {
//        if (stream is IFileStreamWrapper fileStreamWrapper)
//        {
//            return CreateCore(fileStreamWrapper);
//        }
//        else if (stream is FileStream fileStream)
//        {
//            return CreateCore(fileStream);
//        }
//        else
//        {
//            return CreateCore(stream);
//        }
//    }

//    public static SKImage FromEncodedDataCore(IFileStreamWrapper fileStreamWrapper)
//    {
//        return SKImage.FromEncodedData(fileStreamWrapper.Name);
//    }

//    public static SKImage FromEncodedDataCore(FileStream fileStream, bool disposeManagedStream = false)
//    {
//        try
//        {
//            var filename = fileStream.Name;
//            return SKImage.FromEncodedData(filename);
//        }
//        finally
//        {
//            if (disposeManagedStream) fileStream.Dispose();
//        }
//    }

//    public static SKImage FromEncodedDataCore(MemoryStream memoryStream, bool disposeManagedStream = false)
//    {
//        try
//        {
//            using var data = SKData.CreateCopy(memoryStream.ToArray());
//            return SKImage.FromEncodedData(data);
//        }
//        finally
//        {
//            if (disposeManagedStream) memoryStream.Dispose();
//        }
//    }

//    public static SKImage FromEncodedDataCore(Stream stream, bool disposeManagedStream = false)
//    {
//        using var skStream = new SKManagedStream(stream, disposeManagedStream);
//        using var data = SKData.Create(skStream);
//        if (data != null) return SKImage.FromEncodedData(data);

//        if (stream.CanSeek) stream.Position = 0;
//        using var ms = new MemoryStream();
//        stream.CopyTo(ms);
//        ms.Seek(0, SeekOrigin.Begin);
//        // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/basics/bitmaps#loading-a-bitmap-from-the-web
//        return FromEncodedDataCore(ms, disposeManagedStream: true);
//    }

//    public static SKImage FromEncodedData(Stream stream)
//    {
//        if (stream is IFileStreamWrapper fileStreamWrapper)
//        {
//            return FromEncodedDataCore(fileStreamWrapper);
//        }
//        else if (stream is FileStream fileStream)
//        {
//            return FromEncodedDataCore(fileStream);
//        }
//        else if (stream is MemoryStream memoryStream)
//        {
//            try
//            {
//                return FromEncodedDataCore(memoryStream);
//            }
//            catch
//            {

//            }
//        }
//        return FromEncodedDataCore(stream);
//    }

//    static Bitmap GetBitmap(this ImmutableBitmap bitmap) => new(RefCountable.Create(bitmap));

//    public static Bitmap GetBitmap(SKImage image) => new ImmutableBitmap(image).GetBitmap();

//    public static Bitmap GetBitmap(SKBitmap bitmap)
//    {
//        var image = SKImage.FromBitmap(bitmap);
//        return GetBitmap(image);
//    }

//    public static Bitmap DecodeToWidth(SKBitmap bitmap, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality) => new ImmutableBitmap(bitmap, width, true, interpolationMode).GetBitmap();
//}
