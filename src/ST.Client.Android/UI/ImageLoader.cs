using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.OS;
using Square.OkHttp3;
using Square.Picasso;
using System.Application.Services;
using System.IO;
using JFile = Java.IO.File;
using AndroidApplication = Android.App.Application;
using _ThisAssembly = System.Properties.ThisAssembly;
using System.Net.Http;

namespace System.Application.UI
{
    public static class ImageLoader
    {
        const string TAG = nameof(ImageLoader);

        static readonly Lazy<Picasso> _Picasso = new(GetPicasso);
        static Picasso GetPicasso()
        {
            Picasso.Builder picassoBuilder = new(AndroidApplication.Context);
            picassoBuilder.IndicatorsEnabled(_ThisAssembly.Debuggable);
            var cacheDir = CreateDefaultCacheDir();
            var maxSize = CalculateDiskCacheSize(cacheDir);
            var client = CreateOkHttpClient(cacheDir, maxSize);
            OkHttp3Downloader downloader = new(client);
            picassoBuilder.Downloader(downloader);
            return picassoBuilder.Build();
        }

        public static Picasso Picasso => _Picasso.Value;

        #region 高效加载大型位图 https://developer.android.google.cn/topic/performance/graphics/load-bitmap?hl=zh-cn#java

        public static void SetImageSource(this ImageView imageView,
            Stream? stream,
            int targetResIdW = 0,
            int targetResIdH = 0,
            int targetW = 0,
            int targetH = 0,
            Bitmap.Config? inPreferredConfig = null)
        {
            if (stream == null || !stream.CanRead)
            {
                imageView.SetImageDrawable(null);
                return;
            }
            try
            {
                Bitmap? bitmap = null;
                if (stream.CanSeek)
                {
                    if (targetResIdW > 0)
                    {
                        if (targetResIdH <= 0) targetResIdH = targetResIdW;
                        var resources = imageView.Resources!;
                        var reqWidth = resources.GetDimensionPixelSize(targetResIdW);
                        var reqHeight = resources.GetDimensionPixelSize(targetResIdH);
                        bitmap = DecodeSampledBitmapFromStream(stream, reqWidth, reqHeight, inPreferredConfig);
                    }
                    else if (targetW > 0)
                    {
                        if (targetH <= 0) targetH = targetW;
                        bitmap = DecodeSampledBitmapFromStream(stream, targetW, targetH, inPreferredConfig);
                    }
                }
                bitmap ??= BitmapFactory.DecodeStream(stream)!;
#if DEBUG
                Log.Info(TAG,
                    $"Context: {imageView.Context!.GetType().Name}, " +
                    $"Bitmap.Width: {bitmap.Width}, " +
                    $"Bitmap.Height: {bitmap.Height}, " +
                    $"Bitmap.Config: {bitmap.GetConfig()}, " +
                    $"Bitmap.Size1: {IOPath.GetSizeString(bitmap.ByteCount)}, " +
                    $"Bitmap.Size2: {IOPath.GetSizeString(bitmap.AllocationByteCount)}.");
#endif
                imageView.SetImageBitmap(bitmap);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource(Stream) catch.");
            }
        }

        static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        static Bitmap DecodeSampledBitmapFromStream(Stream stream, int reqWidth, int reqHeight, Bitmap.Config? inPreferredConfig = null)
        {
            // First decode with inJustDecodeBounds=true to check dimensions
            BitmapFactory.Options options = new();
            if (inPreferredConfig != null)
            {
                options.InPreferredConfig = inPreferredConfig;
            }
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(stream, null, options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            stream.Position = 0;
            return BitmapFactory.DecodeStream(stream, null, options)!;
        }

        #endregion

        public static void SetImageSource(this ImageView imageView,
            string? requestUri,
            //int targetSize = 0,
            int targetResIdW,
            int targetResIdH = 0,
            ScaleType scaleType = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                imageView.SetImageDrawable(null);
                return;
            }
            try
            {
                var ctx = imageView.Context ?? AndroidApplication.Context;
                var placeholder = new ColorDrawable(new(ctx.GetColorCompat(Resource.Color.grey_background)));
                var errorDrawable = new ColorDrawable(Color.DarkRed);
                var requestCreator = Picasso.Load(requestUri)
                    .Placeholder(placeholder)
                    .Error(errorDrawable);
                var useCenterCropDefault = false;
                //if (targetSize > 0)
                //{
                //    requestCreator = requestCreator.Resize(targetSize, targetSize);
                //    useCenterCropDefault = true;
                //}
                //else
                if (targetResIdW > 0)
                {
                    if (targetResIdH <= 0) targetResIdH = targetResIdW;
                    requestCreator = requestCreator.ResizeDimen(targetResIdW, targetResIdH);
                    useCenterCropDefault = true;
                }
                if (scaleType == ScaleType.CenterCrop || (useCenterCropDefault && scaleType == default))
                {
                    requestCreator = requestCreator.CenterCrop();
                }
                else if (scaleType == ScaleType.CenterInside)
                {
                    requestCreator = requestCreator.CenterInside();
                }
                requestCreator.Into(imageView, null, e =>
                {
                    Log.Error(TAG, e, "SetImageSource(string)|Callback catch, requestUri: {0}", requestUri);
                });
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource(string) catch, requestUri: {0}", requestUri);
            }
        }

        public enum ScaleType
        {
            Default,
            CenterCrop,
            CenterInside,
        }

        // https://github.com/JakeWharton/picasso2-okhttp3-downloader/blob/master/src/main/java/com/jakewharton/picasso/OkHttp3Downloader.java

        static JFile CreateDefaultCacheDir()
        {
            var cachePath = IHttpService.GetImagesCacheDirectory(null);
            JFile cache = new(cachePath);
            if (!cache.Exists())
            {
                //noinspection ResultOfMethodCallIgnored
                cache.Mkdirs();
            }
            return cache;
        }

        const int MIN_DISK_CACHE_SIZE = 5 * 1024 * 1024; // 5MB
        const int MAX_DISK_CACHE_SIZE = 50 * 1024 * 1024; // 50MB

        static long CalculateDiskCacheSize(JFile dir)
        {
            long size = MIN_DISK_CACHE_SIZE;

            try
            {
                var statFs = new StatFs(dir.AbsolutePath);
                long available = statFs.BlockCountLong * statFs.BlockSizeLong;
                // Target 2% of the total space.
                size = available / 50;
            }
            catch (Java.Lang.IllegalArgumentException)
            {
            }

            // Bound inside min/max size for disk cache.
            return Math.Max(Math.Min(size, MAX_DISK_CACHE_SIZE), MIN_DISK_CACHE_SIZE);
        }

        static OkHttpClient CreateOkHttpClient(JFile cacheDir, long maxSize)
        {
            var s = IHttpPlatformHelperService.Instance;
            var client = new OkHttpClient.Builder()
                .Cache(new(cacheDir, maxSize))
                .FollowRedirects(true)
                .FollowSslRedirects(true)
                .CallTimeout(GeneralHttpClientFactory.DefaultTimeoutMilliseconds, Java.Util.Concurrent.TimeUnit.Milliseconds)
                .AddInterceptor(chain =>
                {
                    var newRequest = chain.Request().NewBuilder()
                        .AddHeader("User-Agent", s.UserAgent)
                        .Build();
                    return chain.Proceed(newRequest);
                })
                .Build();
            return client;
        }
    }
}