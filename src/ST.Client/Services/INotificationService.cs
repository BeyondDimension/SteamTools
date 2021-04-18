using System.Application.Models;
using System.Security;
using System.Threading.Tasks;
using static System.Application.KeyConstants;

namespace System.Application.Services
{
    /// <inheritdoc cref="INotificationService{TNotificationType, TEntrance}"/>
    public interface INotificationService : INotificationService<NotificationType, Entrance>
    {
        public static INotificationService Instance => DI.Get<INotificationService>();

        /// <summary>
        /// 显示从服务端获取到通知纪录
        /// </summary>
        /// <param name="notification"></param>
        private void Notify(NotificationRecordDTO notification)
        {
            Notify(notification.Content, notification.Type, title: notification.Title);
        }

        /// <summary>
        /// 显示从服务端获取到通知纪录
        /// </summary>
        /// <param name="rsp"></param>
        /// <param name="type"></param>
        static async void Notify(IApiResponse<NotificationRecordDTO?> rsp, ActiveUserType type)
        {
            if (DI.DeviceIdiom == DeviceIdiom.Desktop)
            {
                return;
                //throw new NotImplementedException();
            }
            if (rsp.IsSuccess && rsp.Content != null)
            {
                if (type == ActiveUserType.OnStartup)
                {
                    await IStorage.Instance.SetAsync(ON_LAST_STARTUP_NOTIFICATION_RECORD_ID, rsp.Content.Id);
                }

                if (!rsp.Content.Type.IsDefined())
                {
                    // 新增通知类型不能兼容旧版本
                    return;
                }

                if (rsp.Content.Type == NotificationType.Announcement)
                {
                    var isShow = IAnnouncementService.Instance.Show(rsp.Content);
                    if (isShow) return;
                }

                Instance.Notify(rsp.Content);
            }
        }

        /// <summary>
        /// 获取最后一次收到的通知ID
        /// </summary>
        /// <returns></returns>
        static Task<Guid?> GetLastNotificationRecordId() => IStorage.Instance.GetAsync<Guid?>(ON_LAST_STARTUP_NOTIFICATION_RECORD_ID);
    }
}