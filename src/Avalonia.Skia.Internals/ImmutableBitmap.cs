//extern alias AvaloniaSkia;

//using Avalonia.Platform;
//using AvaloniaSkia::Avalonia.Skia.Helpers;
//using SkiaSharp;
//using AvaloniaSkia::Avalonia.Skia;
//using Avalonia.Media.Imaging;
//using DrawingContextImpl = AvaloniaSkia::Avalonia.Skia.DrawingContextImpl;
//using IDrawableBitmapImpl = AvaloniaSkia::Avalonia.Skia.IDrawableBitmapImpl;

//// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Skia/Avalonia.Skia/ImmutableBitmap.cs

//namespace Avalonia.Skia;

///// <summary>
///// Immutable Skia bitmap.
///// </summary>
//internal class ImmutableBitmap : IDrawableBitmapImpl, IReadableBitmapImpl
//{
//    private readonly SKImage _image;
//    private readonly SKBitmap? _bitmap;

//    public ImmutableBitmap(SKImage image)
//    {
//        _image = image;

//        if (_image == null)
//        {
//            throw new ArgumentException("Unable to load bitmap from provided data");
//        }

//        PixelSize = new PixelSize(_image.Width, _image.Height);

//        // TODO: Skia doesn't have an API for DPI.
//        Dpi = new Vector(96, 96);
//    }

//    /// <summary>
//    /// Create immutable bitmap from given stream.
//    /// </summary>
//    /// <param name="stream">Stream containing encoded data.</param>
//    public ImmutableBitmap(Stream stream) : this(SkiaSharpHelpers.FromEncodedData(stream))
//    {

//    }

//    public ImmutableBitmap(ImmutableBitmap src, PixelSize destinationSize, BitmapInterpolationMode interpolationMode)
//    {
//        SKImageInfo info = new SKImageInfo(destinationSize.Width, destinationSize.Height, SKColorType.Bgra8888);
//        SKImage output = SKImage.Create(info);
//        src._image.ScalePixels(output.PeekPixels(), interpolationMode.ToSKFilterQuality());

//        _image = output;

//        PixelSize = new PixelSize(_image.Width, _image.Height);

//        // TODO: Skia doesn't have an API for DPI.
//        Dpi = new Vector(96, 96);
//    }

//    public ImmutableBitmap(Stream stream, int decodeSize, bool horizontal, BitmapInterpolationMode interpolationMode) : this(SkiaSharpHelpers.Create(stream), decodeSize, horizontal, interpolationMode)
//    {

//    }

//    public ImmutableBitmap(SKBitmap bmp, int decodeSize, bool horizontal, BitmapInterpolationMode interpolationMode)
//    {
//        var info = bmp.Info;

//        // now scale that to the size that we want
//        var realScale = horizontal ? ((double)info.Height / info.Width) : ((double)info.Width / info.Height);

//        SKImageInfo desired;

//        if (horizontal)
//        {
//            desired = new SKImageInfo(decodeSize, (int)(realScale * decodeSize));
//        }
//        else
//        {
//            desired = new SKImageInfo((int)(realScale * decodeSize), decodeSize);
//        }

//        if (bmp.Width != desired.Width || bmp.Height != desired.Height)
//        {
//            var scaledBmp = bmp.Resize(desired, interpolationMode.ToSKFilterQuality());
//            bmp.Dispose();
//            bmp = scaledBmp;
//        }

//        _image = SKImage.FromBitmap(bmp);
//        bmp.Dispose();

//        if (_image == null)
//        {
//            throw new ArgumentException("Unable to load bitmap from provided data");
//        }

//        PixelSize = new PixelSize(_image.Width, _image.Height);

//        // TODO: Skia doesn't have an API for DPI.
//        Dpi = new Vector(96, 96);
//    }

//    static SKBitmap Decode(SKCodec codec, int decodeSize, bool horizontal, bool leaveOpen)
//    {
//        try
//        {
//            var info = codec.Info;

//            // get the scale that is nearest to what we want (eg: jpg returned 512)
//            var supportedScale = codec.GetScaledDimensions(horizontal ? ((float)decodeSize / info.Width) : ((float)decodeSize / info.Height));

//            // decode the bitmap at the nearest size
//            var nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);
//            var bmp = SKBitmap.Decode(codec, nearest);

//            return bmp;
//        }
//        finally
//        {
//            if (!leaveOpen) codec.Dispose();
//        }
//    }

//    public ImmutableBitmap(SKCodec codec, int decodeSize, bool horizontal, BitmapInterpolationMode interpolationMode, bool leaveOpen = false)
//        : this(Decode(codec, decodeSize, horizontal, leaveOpen), decodeSize, horizontal, interpolationMode)
//    {

//    }

//    /// <summary>
//    /// Create immutable bitmap from given pixel data copy.
//    /// </summary>
//    /// <param name="size">Size of the bitmap.</param>
//    /// <param name="dpi">DPI of the bitmap.</param>
//    /// <param name="stride">Stride of data pixels.</param>
//    /// <param name="format">Format of data pixels.</param>
//    /// <param name="alphaFormat">Alpha format of data pixels.</param>
//    /// <param name="data">Data pixels.</param>
//    public ImmutableBitmap(PixelSize size, Vector dpi, int stride, PixelFormat format, AlphaFormat alphaFormat, IntPtr data)
//    {
//        var imageInfo = new SKImageInfo(size.Width, size.Height, format.ToSkColorType(), alphaFormat.ToSkAlphaType());

//        _image = SKImage.FromPixelCopy(imageInfo, data, stride);

//        if (_image == null)
//        {
//            throw new ArgumentException("Unable to create bitmap from provided data");
//        }

//        PixelSize = size;
//        Dpi = dpi;
//    }

//    public Vector Dpi { get; }

//    public PixelSize PixelSize { get; }

//    public int Version { get; } = 1;

//    /// <inheritdoc />
//    public void Dispose()
//    {
//        _image.Dispose();
//    }

//    /// <inheritdoc />
//    public void Save(string fileName, int? quality = null)
//    {
//        ImageSavingHelper.SaveImage(_image, fileName);
//    }

//    /// <inheritdoc />
//    public void Save(Stream stream, int? quality = null)
//    {
//        ImageSavingHelper.SaveImage(_image, stream);
//    }

//    /// <inheritdoc />
//    public void Draw(DrawingContextImpl context, SKRect sourceRect, SKRect destRect, SKPaint paint)
//    {
//        context.Canvas.DrawImage(_image, sourceRect, destRect, paint);
//    }

//    public PixelFormat? Format => _bitmap?.ColorType.ToAvalonia();

//    public ILockedFramebuffer Lock()
//    {
//        if (_bitmap is null)
//            throw new NotSupportedException("A bitmap is needed for locking");

//        if (_bitmap.ColorType.ToAvalonia() is not { } format)
//            throw new NotSupportedException($"Unsupported format {_bitmap.ColorType}");

//        return new LockedFramebuffer(_bitmap.GetPixels(), PixelSize, _bitmap.RowBytes, Dpi, format, null);
//    }
//}