namespace System.Application.UI
{
    public enum EmojiFontType
    {
        /// <summary>
        /// Android 默认字体(由于低版本字体缺少字，嵌入最新版本的默认字体文件可在低版本上显示后出的表情)
        /// </summary>
        NotoColorEmoji = 1,

        /// <summary>
        /// Android 果冻风格Emoji
        /// </summary>
        Blobmoji,

        /// <summary>
        /// Twitter Emoji
        /// </summary>
        Twemoji,

        /// <summary>
        /// https://github.com/joypixels/emojione
        /// </summary>
        EmojiOne,
    }
}