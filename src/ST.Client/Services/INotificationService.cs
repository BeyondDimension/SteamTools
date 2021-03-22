using System.Application.Properties;

namespace System.Application.Services
{
    /// <inheritdoc cref="INotificationService{TNotificationType}"/>
    public interface INotificationService : INotificationService<NotificationType>
    {
        /// <summary>
        /// 下载新版本进度通知
        /// </summary>
        /// <returns></returns>
        public Progress<float> NotifyNewVersionDownloadProgress()
        {
            return NotifyDownload(
                SR.Downloading_,
                NotificationType.NewVersionDownloadProgress,
                SR.AppName + SR.NewVersionUpdate);
        }
    }
}