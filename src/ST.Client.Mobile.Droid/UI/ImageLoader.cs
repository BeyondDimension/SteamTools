using Android.Content;
using Android.Graphics;
using Android.Widget;
using Square.Picasso;
using System.Application.Services;
using System.IO;
using System.Properties;
using JFile = Java.IO.File;

namespace System.Application.UI
{
    public static class ImageLoader
    {
        const string TAG = nameof(ImageLoader);

        static Picasso? _Picasso;
        public static Picasso Picasso => _Picasso ?? throw new NullReferenceException("ImageLoader.Init must be called.");

        public static void Init(Context context)
        {
            Picasso.Builder picassoBuilder = new(context);
            picassoBuilder.IndicatorsEnabled(ThisAssembly.Debuggable);
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
            OkHttp3Downloader downloader = new(CreateDefaultCacheDir());
            picassoBuilder.Downloader(downloader);
            _Picasso = picassoBuilder.Build();
        }

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
            ScaleType? scaleType = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                imageView.SetImageDrawable(null);
                return;
            }
            try
            {
                var requestCreator = Picasso.Load(requestUri);
                var useCenterCropDefault = false;
                if (targetSize > 0)
                {
                    requestCreator.Resize(targetSize, targetSize);
                    useCenterCropDefault = true;
                }
                else if (targetResId > 0)
                {
                    requestCreator.ResizeDimen(targetResId, targetResId);
                    useCenterCropDefault = true;
                }
                if (scaleType == ScaleType.CenterCrop || (useCenterCropDefault && scaleType == default))
                {
                    requestCreator.CenterCrop();
                }
                else if (scaleType == ScaleType.CenterInside)
                {
                    requestCreator.CenterInside();
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
    }
}