using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System.Application.Models;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace System.Application.Converters
{
    public class BitmapAssetValueConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            int width = 0;
            if (parameter is string para)
            {
                if (int.TryParse(para, out int w))
                    width = w;
            }

            if (value is string rawUri)
            {
                if (rawUri == string.Empty) return null;

                Uri uri;
                // Allow for assembly overrides
                if (File.Exists(rawUri))
                {
                    return GetDecodeBitmap(rawUri, width);
                }
                //在列表中使用此方法性能极差
                else if (rawUri.StartsWith("http://") || rawUri.StartsWith("https://"))
                {
                    return DownloadImage(rawUri, width);
                }
                else if (rawUri.StartsWith("avares://"))
                {
                    uri = new Uri(rawUri);
                }
                else
                {
                    string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                    uri = new Uri($"avares://{assemblyName}{rawUri}");
                }
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets.Open(uri);
                return GetDecodeBitmap(asset, width);
            }
            else if (value is Stream s)
            {
                TryReset(s);
                return GetDecodeBitmap(s, width);
            }
            else if (value is ImageClipStream ics)
            {
                return GetBitmap(ics);
            }
            else if (value is Guid imageid)
            {
                if (Guid.Empty == imageid)
                {
                    return null;
                }
                return DownloadImage(ImageUrlHelper.GetImageApiUrlById(imageid), width);
            }
            throw new NotSupportedException();
        }

        [Obsolete("use HttpClient")]
        static Bitmap DownloadImage(string url, int width = 0)
        {
            using var web = new WebClient();
            var bt = web.DownloadData(url);
            using var stream = new MemoryStream(bt);
            return GetDecodeBitmap(stream, width);
        }

        static void TryReset(Stream s)
        {
            if (s.CanSeek)
            {
                if (s.Position > 0)
                {
                    s.Position = 0;
                }
            }
        }

        static Bitmap GetBitmap(ImageClipStream ics, int width = 0)
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

        static Bitmap GetDecodeBitmap(Stream s, int width)
        {
            if (width < 1)
            {
                return new Bitmap(s);
            }
            return Bitmap.DecodeToWidth(s, width, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.MediumQuality);
        }

        static Bitmap GetDecodeBitmap(string s, int width)
        {
            return GetDecodeBitmap(IOPath.OpenRead(s), width);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}