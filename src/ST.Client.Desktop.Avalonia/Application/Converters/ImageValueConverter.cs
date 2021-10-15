using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System.Application.Models;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace System.Application.Converters
{
    public abstract class ImageValueConverter : IValueConverter
    {
        public abstract object? Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }

        [Obsolete("use HttpClient")]
        protected static Bitmap? DownloadImage(string? url, int width = 0)
        {
            if (url == null) return null;
            using var web = new WebClient();
            var ua = DI.Get<IHttpPlatformHelperService>().UserAgent;
            web.Headers.Add("User-Agent", ua);
            var bt = web.DownloadData(url);
            using var stream = new MemoryStream(bt);
            return GetDecodeBitmap(stream, width);
        }

        protected static void TryReset(Stream s)
        {
            if (s.CanSeek)
            {
                if (s.Position > 0)
                {
                    s.Position = 0;
                }
            }
        }

        protected static Bitmap? GetBitmap(ImageSouce.ClipStream clipStream, int width = 0)
        {
            if (clipStream.Stream == null)
                return null;
            TryReset(clipStream.Stream);
            using var ms = new MemoryStream();
            clipStream.Stream.CopyTo(ms);
            TryReset(ms);
            using var bitmapSource = SKBitmap.Decode(ms);
            using var bitmapDest = new SKBitmap(bitmapSource.Width, bitmapSource.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            using var canvas = new SKCanvas(bitmapDest);

            var rect = clipStream.Circle ?
                new SKRect(0, 0, bitmapSource.Width, bitmapSource.Height) :
                new SKRect(clipStream.Left, clipStream.Top, clipStream.Right, clipStream.Bottom);
            var roundRect = clipStream.Circle ?
                new SKRoundRect(rect, bitmapSource.Width / 2f, bitmapSource.Height / 2f) :
                new SKRoundRect(rect, clipStream.Radius_X, clipStream.Radius_Y);
            canvas.ClipRoundRect(roundRect, antialias: true);

            canvas.DrawBitmap(bitmapSource, 0, 0);

            var stream = bitmapDest.Encode(SKEncodedImageFormat.Png, 100).AsStream();
            TryReset(stream);

            //var tempFilePath = Path.Combine(IOPath.CacheDirectory, Path.GetFileName(Path.GetTempFileName() + ".png"));
            //using (var fs = File.Create(tempFilePath))
            //{
            //    stream.CopyTo(fs);
            //    TryReset(stream);
            //}

            return GetDecodeBitmap(stream, width);
        }

        protected static Bitmap? GetDecodeBitmap(Stream s, int width)
        {
            if (s == null) 
            {
                return null;
            }
            if (width < 1)
            {
                return new Bitmap(s);
            }
            return Bitmap.DecodeToWidth(s, width, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.MediumQuality);
        }

        protected static Bitmap GetDecodeBitmap(string s, int width)
        {
            if (IOPath.TryOpenRead(s, out var stream, out var ex))
                return GetDecodeBitmap(stream, width);
            else
                return null;
        }

        protected static Stream? OpenAssets(Uri uri)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Stream? stream = null;
            if (assets.Exists(uri))
                stream = assets.Open(uri);
            return stream;
        }

        protected static Uri GetResUri(string relativeUri, string? assemblyName = null)
        {
            assemblyName ??= (Assembly.GetEntryAssembly() ?? typeof(ImageValueConverter).Assembly).GetName().Name;
            if (assemblyName == null) throw new ArgumentNullException(assemblyName);
            var uri = new Uri($"avares://{assemblyName}{relativeUri}");
            return uri;
        }
    }
}