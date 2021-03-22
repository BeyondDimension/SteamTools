using Android.Content;
using AndroidX.Emoji.Text;

namespace System.Application.UI
{
    /// <summary>
    /// Android Emoji 官方兼容库
    /// </summary>
    public static class EmojiCompatLibrary
    {
        /// <summary>
        /// 获取当前使用的 Emoji 字体
        /// </summary>
        public static EmojiFontType CurrentFontType { get; private set; }

        /// <summary>
        /// 初始化 Emoji 库
        /// </summary>
        /// <param name="context"></param>
        /// <param name="emojiFontType"></param>
        public static void Init(Context context, EmojiFontType emojiFontType = EmojiFontType.Twemoji)
        {
            var config = emojiFontType switch
            {
                EmojiFontType.NotoColorEmoji => Create("fonts/NotoColorEmojiCompat.ttf"),
                EmojiFontType.Blobmoji => Create("fonts/BlobmojiCompat.ttf").SetReplaceAll(true),
                EmojiFontType.Twemoji => Create("fonts/TwemojiCompat.ttf").SetReplaceAll(true),
                EmojiFontType.EmojiOne => Create("fonts/emojione-android.ttf").SetReplaceAll(true),
                _ => throw new ArgumentOutOfRangeException(nameof(emojiFontType), emojiFontType, null),
            };
            CurrentFontType = emojiFontType;
            EmojiCompat.Init(config);

            EmojiCompat.Config Create(string assetName) => new EmojiCompatLibraryConfig(context, assetName);
        }
    }
}