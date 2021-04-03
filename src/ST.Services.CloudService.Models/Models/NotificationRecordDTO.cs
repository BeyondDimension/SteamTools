using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 通知纪录
    /// </summary>
    [MPObj]
    public class NotificationRecordDTO
    {
        [MPKey(0)]
        public Guid Id { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        [MPKey(1)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知内容
        /// </summary>
        [MPKey(2)]
        public string Content { get; set; } = string.Empty;

        [MPKey(3)]
        public NotificationType Type { get; set; }
    }
}