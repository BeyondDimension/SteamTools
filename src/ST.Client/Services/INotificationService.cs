using System.Application.Models;
using System.Properties;
using System.Security;
using System.Threading.Tasks;
using static System.Application.KeyConstants;

namespace System.Application.Services
{
    /// <summary>
    /// 本地通知服务
    /// </summary>
    public interface INotificationService
    {
        static INotificationService Instance => DI.Get<INotificationService>();

        /// <inheritdoc cref="NotificationBuilder.DefaultTitle"/>
        protected const string DefaultTitle = NotificationBuilder.DefaultTitle;

        /// <summary>
        /// 获取是否有通知权限
        /// </summary>
        /// <returns></returns>
        bool AreNotificationsEnabled() => true;

        /// <summary>
        /// 显示本地通知，使用 new <see cref="NotificationBuilder"/>() 构建参数
        /// </summary>
        /// <param name="builder"></param>
        void Notify(NotificationBuilder.IInterface builder);

        /// <summary>
        /// 显示本地通知
        /// </summary>
        /// <param name="text">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <param name="autoCancel"></param>
        /// <param name="title">通知标题</param>
        /// <param name="entrance">点击通知的入口点</param>
        /// <param name="requestUri">入口点为 <see cref="Entrance.Browser"/> 时的 HttpUrl</param>
        void Notify(
            string text,
            NotificationType notificationType,
            bool autoCancel = true,
            string? title = default,
            Entrance entrance = default,
            string? requestUri = default)
        {
            var builder = new NotificationBuilder
            {
                Title = string.IsNullOrWhiteSpace(title) ? DefaultTitle : title,
                Content = text,
                AutoCancel = autoCancel,
                Click = entrance switch
                {
                    Entrance.Main => new NotificationBuilder.ClickAction
                    {
                        Entrance = Entrance.Main
                    },
                    Entrance.Browser => new NotificationBuilder.ClickAction
                    {
                        Entrance = Entrance.Browser,
                        RequestUri = requestUri
                    },
                    _ => null,
                },
            };
            Notify(builder);
        }

        /// <summary>
        /// 显示从服务端获取到通知纪录
        /// </summary>
        /// <param name="notification"></param>
        [Obsolete("use NoticeDTO")]
        void Notify(NotificationRecordDTO notification)
            => Notify(notification.Content, notification.Type, title: notification.Title);

        /// <summary>
        /// 取消通知
        /// </summary>
        /// <param name="notificationType"></param>
        void Cancel(NotificationType notificationType);

        /// <summary>
        /// 取消所有通知
        /// </summary>
        void CancelAll();

        /// <summary>
        /// 当前平台是否支持下载进度通知
        /// </summary>
        bool IsSupportNotifyDownload => false;

        /// <summary>
        /// 下载进度通知
        /// </summary>
        Progress<float> NotifyDownload(
            Func<string> text,
            NotificationType notificationType,
            string? title = default) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 显示从服务端获取到通知纪录
        /// </summary>
        /// <param name="rsp"></param>
        /// <param name="type"></param>
        [Obsolete("use NoticeDTO")]
        static async void Notify(IApiResponse<NotificationRecordDTO?> rsp, ActiveUserType type)
        {
            if (rsp.IsSuccess && rsp.Content != null)
            {
                if (type == ActiveUserType.OnStartup)
                {
                    await ISecureStorage.Instance.SetAsync(ON_LAST_STARTUP_NOTIFICATION_RECORD_ID, rsp.Content.Id);
                }

                if (!rsp.Content.Type.IsDefined())
                {
                    // 新增通知类型不能兼容旧版本
                    return;
                }

                if (rsp.Content.Type == NotificationType.Announcement)
                {
                    var announcementService = DI.Get_Nullable<IAnnouncementService>();
                    if (announcementService != null)
                    {
                        var isShow = announcementService.Show(rsp.Content);
                        if (isShow) return;
                    }
                }

                Instance.Notify(rsp.Content);
            }
        }

        /// <summary>
        /// 获取最后一次收到的通知ID
        /// </summary>
        /// <returns></returns>
        static Task<Guid?> GetLastNotificationRecordId() => ISecureStorage.Instance.GetAsync<Guid?>(ON_LAST_STARTUP_NOTIFICATION_RECORD_ID);

        /// <summary>
        /// NotifyIcon / TrayIcon 右下角托盘菜单助手类
        /// </summary>
        public abstract class NotifyIconHelper
        {
            protected NotifyIconHelper() => throw new NotSupportedException();

            /// <summary>
            /// 托盘初始化是否完成
            /// <para>注意：在 Windows 上托盘初始化之前调用气泡消息会导致托盘不显示</para>
            /// </summary>
            public static bool IsInitialized { get; protected set; }
        }
    }
}