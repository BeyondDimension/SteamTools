using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 通知纪录
    /// </summary>
    [MPObj]
    [Obsolete("use NoticeDTO or NotificationBuilder")]
    public class NotificationRecordDTO
    {
        [MPKey(0)]
        public Guid Id { get; set; }

        /// <summary>
        /// 通知标题
        /// <para>https://developer.android.google.cn/reference/androidx/core/app/NotificationCompat.Builder?hl=zh-cn#setContentTitle(java.lang.CharSequence)</para>
        /// </summary>
        [MPKey(1)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知正文
        /// <para>https://developer.android.google.cn/reference/androidx/core/app/NotificationCompat.Builder?hl=zh-cn#setContentText(java.lang.CharSequence)</para>
        /// </summary>
        [MPKey(2)]
        public string Content { get; set; } = string.Empty;

        [MPKey(3)]
        public NotificationType Type { get; set; }

        /// <summary>
        /// 公告内容
        /// </summary>
        [MPKey(4)]
        public string? Announcement { get; set; }
    }
}