using Android.Content;
using AndroidX.Emoji.Text;
using JObject = Java.Lang.Object;
using JThread = Java.Lang.Thread;
using Runnable = Java.Lang.IRunnable;
using Throwable = Java.Lang.Throwable;

namespace System.Application.UI
{
    internal sealed class EmojiCompatLibraryConfig : EmojiCompat.Config
    {
        public EmojiCompatLibraryConfig(Context context, string assetName) : base(new AssetMetadataLoader(context, assetName))
        {
        }

        class AssetMetadataLoader : JObject, EmojiCompat.IMetadataRepoLoader
        {
            readonly Context mContext;
            readonly string assetName;

            public AssetMetadataLoader(Context context, string assetName)
            {
                mContext = context;
                this.assetName = assetName;
            }

            public void Load(EmojiCompat.MetadataRepoLoaderCallback loaderCallback)
            {
                var runnable = new InitRunnable(mContext, loaderCallback, assetName);
                var thread = new JThread(runnable)
                {
                    Daemon = false
                };
                thread.Start();
            }
        }

        class InitRunnable : JObject, Runnable
        {
            readonly string FONT_NAME;
            readonly EmojiCompat.MetadataRepoLoaderCallback loaderCallback;
            readonly Context context;

            public InitRunnable(Context context, EmojiCompat.MetadataRepoLoaderCallback loaderCallback, string FONT_NAME)
            {
                this.context = context;
                this.loaderCallback = loaderCallback;
                this.FONT_NAME = FONT_NAME;
            }

            public void Run()
            {
                try
                {
                    var assetManager = context.Assets;
                    var resourceIndex = MetadataRepo.Create(assetManager, FONT_NAME);
                    loaderCallback.OnLoaded(resourceIndex);
                }
                catch (Throwable t)
                {
                    loaderCallback.OnFailed(t);
                }
            }
        }
    }
}