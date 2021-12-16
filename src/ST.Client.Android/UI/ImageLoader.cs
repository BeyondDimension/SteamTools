using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Square.Picasso;
using System.Application.Services;
using System.IO;
using JFile = Java.IO.File;
using AndroidApplication = Android.App.Application;
using _ThisAssembly = System.Properties.ThisAssembly;
using Android.OS;
using Square.OkHttp3;

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

        public static void SetImageSource(this ImageView imageView,
            Stream? stream)
        {
            if (stream == null)
            {
                imageView.SetImageDrawable(null);
                return;
            }
            try
            {
                var bitmap = BitmapFactory.DecodeStream(stream);
                imageView.SetImageBitmap(bitmap);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource(Stream) catch.");
            }
        }

        public static void SetImageSource(this ImageView imageView,
            string? requestUri,
            int targetSize = 0,
            int targetResId = 0,
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
                if (targetSize > 0)
                {
                    requestCreator = requestCreator.Resize(targetSize, targetSize);
                    useCenterCropDefault = true;
                }
                else if (targetResId > 0)
                {
                    requestCreator = requestCreator.ResizeDimen(targetResId, targetResId);
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
                requestCreator.Into(imageView);
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
            var client = new OkHttpClient.Builder()
                .Cache(new(cacheDir, maxSize))
                .FollowRedirects(true)
                .FollowSslRedirects(true)
                .Build();
            return client;
        }
    }
}