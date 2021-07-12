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
            Stream stream)
        {
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
            string requestUri,
            int targetSize = 0,
            ScaleType? scaleType = default)
        {
            try
            {
                var requestCreator = Picasso.Load(requestUri);
                if (targetSize > 0)
                {
                    requestCreator.Resize(targetSize, targetSize);
                }
                if (scaleType == ScaleType.CenterCrop)
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