//extern alias AvaloniaSkia;

//using Avalonia.Media.Imaging;
//using Avalonia.Platform;
//using ISkiaGpu = AvaloniaSkia::Avalonia.Skia.ISkiaGpu;
//using @this = AvaloniaSkia::Avalonia.Skia.PlatformRenderInterface;

//// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Skia/Avalonia.Skia/PlatformRenderInterface.cs

//namespace Avalonia.Skia;

//sealed class PlatformRenderInterface : @this, IPlatformRenderInterface
//{
//    public PlatformRenderInterface(long? maxResourceBytes = null) : base(maxResourceBytes)
//    {
//    }

//    IBitmapImpl IPlatformRenderInterface.LoadBitmap(Stream stream)
//    {
//        return new ImmutableBitmap(stream);
//    }

//    IBitmapImpl IPlatformRenderInterface.LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size, Vector dpi, int stride)
//    {
//        return new ImmutableBitmap(size, dpi, stride, format, alphaFormat, data);
//    }

//    IBitmapImpl IPlatformRenderInterface.LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode)
//    {
//        return new ImmutableBitmap(stream, width, true, interpolationMode);
//    }

//    IBitmapImpl IPlatformRenderInterface.LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode)
//    {
//        return new ImmutableBitmap(stream, height, false, interpolationMode);
//    }

//    IBitmapImpl IPlatformRenderInterface.ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode)
//    {
//        if (bitmapImpl is ImmutableBitmap ibmp)
//        {
//            return new ImmutableBitmap(ibmp, destinationSize, interpolationMode);
//        }
//        else
//        {
//            throw new Exception("Invalid source bitmap type.");
//        }
//    }
//}
