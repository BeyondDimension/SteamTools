using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Properties;
using System.Reactive.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public sealed class NotificationService : ReactiveObject
    {
        static NotificationService? mCurrent;

        public static NotificationService Current => mCurrent ?? new();

        readonly IHttpService httpService = DI.Get<IHttpService>();

        public SourceCache<NoticeDTO, Guid> NoticesSource { get; }

        [Reactive]
        public int UnreadNotificationsCount { get; set; }

        [Reactive]
        public ObservableCollection<NoticeTypeDTO> NoticeTypes { get; set; }

        public NotificationService()
        {
            mCurrent = this;
            NoticesSource = new SourceCache<NoticeDTO, Guid>(x => x.Id);
            NoticeTypes = new ObservableCollection<NoticeTypeDTO>();
        }

        public async Task GetNews(int trycount = 0)
        {
            var lastTime = await INotificationService.GetLastNotificationTime();
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.NewMsg(null, null);
            if (result.IsSuccess && result.Content != null)
            {
                var noticeList = result.Content.Where(x => lastTime.HasValue ? x.EnableTime > lastTime : true);
                if (noticeList.Count() > 1)
                {
                    await INotificationService.SetLastNotificationTime(noticeList.Max(x => x.EnableTime));
                    INotificationService.Instance.Notify(new NotificationBuilder()
                    {
                        Title = AppResources.NotificationChannelType_Description_Announcement,
                        Content = AppResources.Notice_Tray.Format(result.Content.Length),
                        AutoCancel = NotificationBuilder.DefaultAutoCancel,
                        Type = NotificationType.Announcement,
                        Click = new NotificationBuilder.ClickAction(() =>
                        {
                            IWindowManager.Instance.Show(CustomWindow.Notice);
                        })
                    });
                }
                else
                {
                    var notice = noticeList.FirstOrDefault();
                    if (notice != null && notice.EnableTime > lastTime)
                    {
                        await INotificationService.SetLastNotificationTime(notice.EnableTime);
                        INotificationService.Instance.Notify(notice);
                    }

                }

            }
        }

        public async Task LoadNoticeTypes()
        {
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.Types();
            if (result.IsSuccess)
            {
                NoticeTypes.Clear();
                NoticeTypes.AddRange(result.Content!.OrderBy(x => x.Order));
            }
        }

        public async Task LoadNotification(NoticeTypeDTO? selectGroup)
        {
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.Table(selectGroup?.Id, 0, 30);
            if (result.IsSuccess && result.Content != null)
            {
                //foreach (var item in result.Content!.DataSource)
                //{
                //    if (!string.IsNullOrWhiteSpace(item.Picture))
                //        item.PictureStream = httpService.GetImageAsync(item.Picture, ImageChannelType.NoticePicture);
                //}
                NoticesSource.Clear();
                NoticesSource.AddOrUpdate(result.Content.DataSource);
            }
        }
    }
}
