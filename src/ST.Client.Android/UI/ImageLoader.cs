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
using JObject = Java.Lang.Object;
using JException = Java.Lang.Exception;
using AndroidApplication = Android.App.Application;
using Size = System.Drawing.Size;
using _ThisAssembly = System.Properties.ThisAssembly;
using System.Net.Http;
using System.Threading.Tasks;

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
                    $"Bitmap.Size1: {IOPath.GetDisplayFileSizeString(bitmap.ByteCount)}, " +
                    $"Bitmap.Size2: {IOPath.GetDisplayFileSizeString(bitmap.AllocationByteCount)}.");
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

        static Drawable ErrorDrawable => new ColorDrawable(Color.DarkRed);

        static Drawable Placeholder => new ColorDrawable(new(AndroidApplication.Context.GetColorCompat(Resource.Color.grey_background)));

        static RequestCreator? GetRequestCreator(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default,
            bool useErrorDrawable = true,
            bool usePlaceholder = true)
        {
            try
            {
                if (Browser2.IsHttpUrl(requestUri))
                {
                    var requestCreator = Picasso.Load(requestUri);
                    if (usePlaceholder) requestCreator = requestCreator.Placeholder(Placeholder);
                    if (useErrorDrawable) requestCreator = requestCreator.Error(ErrorDrawable);
                    var useCenterCropDefault = false;
                    if (targetSize != default)
                    {
                        if (targetSize.Width > 0)
                        {
                            if (targetSize.Height <= 0) targetSize.Height = targetSize.Width;
                            requestCreator = requestCreator.Resize(targetSize.Width, targetSize.Height);
                            useCenterCropDefault = true;
                        }
                    }
                    else if (targetSizeResId != default)
                    {
                        if (targetSizeResId.Width > 0)
                        {
                            if (targetSizeResId.Height <= 0) targetSizeResId.Height = targetSizeResId.Width;
                            requestCreator = requestCreator.ResizeDimen(targetSizeResId.Width, targetSizeResId.Height);
                            useCenterCropDefault = true;
                        }
                    }
                    if (scaleType == ScaleType.CenterCrop || (useCenterCropDefault && scaleType == default))
                    {
                        requestCreator = requestCreator.CenterCrop();
                    }
                    else if (scaleType == ScaleType.CenterInside)
                    {
                        requestCreator = requestCreator.CenterInside();
                    }
                    return requestCreator;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetRequestCreator catch, requestUri: {0}", requestUri);
            }

            return null;
        }

        public static void SetImageSource(this ImageView imageView,
            string? requestUri,
            int targetResIdW,
            int targetResIdH = 0,
            ScaleType scaleType = default)
        {
            try
            {
                var requestCreator = GetRequestCreator(requestUri, default, new(targetResIdW, targetResIdH), scaleType);

                if (requestCreator == null)
                {
                    imageView.SetImageDrawable(null);
                }
                else
                {
                    requestCreator.Into(imageView, null, e =>
                    {
                        Log.Error(TAG, e, "SetImageSource.Callback catch, requestUri: {0}", requestUri);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource catch, requestUri: {0}", requestUri);
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

        static Task<Bitmap?> GetBitmapCoreAsync(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default)
        {
            try
            {
                var requestCreator = GetRequestCreator(requestUri, targetSize, targetSizeResId, scaleType, useErrorDrawable: false, usePlaceholder: false);

                if (requestCreator != null)
                {
                    var tcs = new TaskCompletionSource<Bitmap?>();

                    requestCreator.Into(new TaskCompletionSourceTarget(tcs));

                    return tcs.Task;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetBitmapCoreAsync catch, requestUri: {0}", requestUri);
            }

            return Task.FromResult<Bitmap?>(null);
        }

        /// <summary>
        /// 从 HttpUrl 中加载图片并返回 <see cref="Bitmap"/> 实例，如果 Url 不合法或出现 <see cref="Exception"/> 将返回 <see langword="null"/>
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="targetSize">目标图片大小宽高</param>
        /// <param name="targetSizeResId">目标图片大小宽高(R.dimen)</param>
        /// <param name="scaleType">图片缩放类型</param>
        /// <returns></returns>
        public static async Task<Bitmap?> GetBitmapAsync(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default)
        {
            try
            {
                var bitmap = await GetBitmapCoreAsync(requestUri, targetSize, targetSizeResId, scaleType);
                return bitmap;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetBitmapAsync catch, requestUri: {0}", requestUri);
            }

            return null;
        }

        sealed class TaskCompletionSourceTarget : JObject, ITarget
        {
            readonly TaskCompletionSource<Bitmap?> tcs;

            public TaskCompletionSourceTarget(TaskCompletionSource<Bitmap?> tcs) => this.tcs = tcs;

            void ITarget.OnBitmapFailed(JException exception, Drawable _)
            {
                tcs.TrySetException(exception);
            }

            void ITarget.OnBitmapLoaded(Bitmap bitmap, Picasso.LoadedFrom _)
            {
                tcs.TrySetResult(bitmap);
            }

            void ITarget.OnPrepareLoad(Drawable _)
            {
            }
        }
    }
}