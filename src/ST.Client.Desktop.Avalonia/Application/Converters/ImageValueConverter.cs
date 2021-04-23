using Avalonia;
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
            throw new NotSupportedException();
        }

        [Obsolete("use HttpClient")]
        protected static Bitmap DownloadImage(string url, int width = 0)
        {
            using var web = new WebClient();
            var ua = DI.Get<IHttpPlatformHelper>().UserAgent;
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

        protected static Bitmap GetBitmap(ImageClipStream ics, int width = 0)
        {
            TryReset(ics.Stream);
            using var ms = new MemoryStream();
            ics.Stream.CopyTo(ms);
            TryReset(ms);
            using var bitmapSource = SKBitmap.Decode(ms);
            using var bitmapDest = new SKBitmap(bitmapSource.Width, bitmapSource.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            using var canvas = new SKCanvas(bitmapDest);

            var rect = ics.Circle ?
                new SKRect(0, 0, bitmapSource.Width, bitmapSource.Height) :
                new SKRect(ics.Left, ics.Top, ics.Right, ics.Bottom);
            var roundRect = ics.Circle ?
                new SKRoundRect(rect, bitmapSource.Width / 2f, bitmapSource.Height / 2f) :
                new SKRoundRect(rect, ics.Radius_X, ics.Radius_Y);
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

        protected static Bitmap GetDecodeBitmap(Stream s, int width)
        {
            if (width < 1)
            {
                return new Bitmap(s);
            }
            return Bitmap.DecodeToWidth(s, width, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.MediumQuality);
        }

        protected static Bitmap GetDecodeBitmap(string s, int width)
        {
            return GetDecodeBitmap(IOPath.OpenRead(s), width);
        }

        protected static Stream OpenAssets(Uri uri)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var asset = assets.Open(uri);
            return asset;
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