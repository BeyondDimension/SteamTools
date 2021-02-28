namespace System.Windows
{
    /// <summary>
    /// 指定消息框所显示的图标。
    /// </summary>
    public enum MessageBoxImageCompat
    {
#pragma warning disable CA1069 // 不应复制枚举值
        Asterisk = 64,
        Error = 16,
        Exclamation = 48,
        Hand = 16,
        Information = 64,
        None = 0,
        [Obsolete("The question mark message icon is no longer recommended because it does not clearly represent a specific type of message and because the phrasing of a message as a question could apply to any message type. In addition, users can confuse the question mark symbol with a help information symbol. Therefore, do not use this question mark symbol in your message boxes. The system continues to support its inclusion only for backward compatibility.")]
        Question = 32,
        Stop = 16,
        Warning = 48,

        [Obsolete("non-standard api")]
        Battery = 1000,
        [Obsolete("non-standard api")]
        Database,
        [Obsolete("non-standard api")]
        Folder,
        [Obsolete("non-standard api")]
        Forbidden,
        [Obsolete("non-standard api")]
        Plus,
        [Obsolete("non-standard api")]
        Setting,
        [Obsolete("non-standard api")]
        SpeakerLess,
        [Obsolete("non-standard api")]
        SpeakerMore,
        [Obsolete("non-standard api")]
        Stop2,
        [Obsolete("non-standard api")]
        Stopwatch,
        [Obsolete("non-standard api")]
        Wifi,
#pragma warning restore CA1069 // 不应复制枚举值
    }
}