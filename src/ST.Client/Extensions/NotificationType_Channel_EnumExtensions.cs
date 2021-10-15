using System.Application.Properties;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <summary>
    /// Enum 扩展 <see cref="NotificationType"/> AND <see cref="NotificationChannelType"/>
    /// </summary>
    public static partial class NotificationType_Channel_EnumExtensions
    {
        /// <summary>
        /// 获取所属的通知渠道
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static NotificationChannelType GetChannel(this NotificationType value) => value switch
        {
            NotificationType.Announcement => NotificationChannelType.Announcement,
            NotificationType.NewVersion => NotificationChannelType.NewVersion,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

        /// <summary>
        /// 获取渠道的用户可见名称
        /// <para>建议的最大长度为40个字符，如果该值太长，可能会被截断</para>
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setName%28java.lang.CharSequence%29 </para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetName(this NotificationChannelType value)
        {
            return value switch
            {
                NotificationChannelType.Announcement => SR.NotificationChannelType_Name_Announcement,
                NotificationChannelType.NewVersion => SR.NotificationChannelType_Name_NewVersion,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }

        /// <summary>
        /// 获取渠道的用户可见描述
        /// <para>建议的最大长度为300个字符，如果该值太长，可能会被截断</para>
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setDescription%28java.lang.String%29 </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetDescription(this NotificationChannelType value)
        {
            return value switch
            {
                NotificationChannelType.Announcement => SR.NotificationChannelType_Description_Announcement,
                NotificationChannelType.NewVersion => SR.NotificationChannelType_Description_NewVersion,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }

        /// <summary>
        /// 获取渠道的重要性级别
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#getImportance%28%29 </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static NotificationImportanceLevel GetImportanceLevel(this NotificationChannelType value)
            => value switch
            {
                NotificationChannelType.Announcement => NotificationImportanceLevel.High,
                NotificationChannelType.NewVersion => NotificationImportanceLevel.Medium,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
    }
}