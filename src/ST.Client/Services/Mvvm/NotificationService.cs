using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Repositories;
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
        readonly INotificationRepository notificationRepository = DI.Get<INotificationRepository>();

        public SourceCache<NoticeDTO, Guid> NoticesSource { get; }

        [Reactive]
        public int UnreadNotificationsCount { get; set; }

        [Reactive]
        public ObservableCollection<NoticeTypeDTO> NoticeTypes { get; set; }

        public NoticeTypeDTO DefaultType { get; } = new NoticeTypeDTO() { Name = AppResources.AllTyoe };

        public NotificationService()
        {
            mCurrent = this;
            NoticesSource = new SourceCache<NoticeDTO, Guid>(x => x.Id);
            NoticeTypes = new ObservableCollection<NoticeTypeDTO>();
        }

        public async Task GetNews(int trycount = 0)
        {
            var data = await notificationRepository.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
            UnreadNotificationsCount = data.Count(x => !x.HasRead);
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.NewMsg(null, null);
            if (result.IsSuccess && result.Content != null)
            {
                var noticeList = result.Content.Where(x => !data.Any(d => d.Id == x.Id));
                if (noticeList.Any_Nullable())
                {
                    if (noticeList.Count() > 1)
                    {
                        INotificationService.Instance.Notify(new NotificationBuilder()
                        {
                            Title = AppResources.Notice_Tray_Title,
                            Content = AppResources.Notice_Tray_Content.Format(result.Content.Length),
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
                        var notice = noticeList.First();
                        INotificationService.Instance.Notify(notice);
                    }

                    await InsertNotificationRecord(noticeList);
                }

                DeleteExpiredRecordAsync();
            }
        }

        public async Task LoadNoticeTypes()
        {
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.Types();
            if (result.IsSuccess)
            {
                NoticeTypes.Clear();
                NoticeTypes.Add(DefaultType);
                NoticeTypes.AddRange(result.Content!.OrderBy(x => x.Order));
            }
        }

        public async Task LoadNotification(NoticeTypeDTO? selectGroup)
        {
            var client = ICloudServiceClient.Instance.Notice;
            var typeid = selectGroup == DefaultType ? null : selectGroup?.Id;
            var result = await client.Table(typeid, 1, 30);
            if (result.IsSuccess && result.Content != null)
            {
                //foreach (var item in result.Content!.DataSource)
                //{
                //    if (!string.IsNullOrWhiteSpace(item.Picture))
                //        item.PictureStream = httpService.GetImageAsync(item.Picture, ImageChannelType.NoticePicture);
                //}
                NoticesSource.Clear();
                var data = await LoadHasReadRecord(result.Content.DataSource);
                NoticesSource.AddOrUpdate(data);
            }
        }

        public async Task<List<NoticeDTO>> LoadHasReadRecord(List<NoticeDTO> notices)
        {
            var data = await notificationRepository.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
            if (data.Any_Nullable())
            {
                UnreadNotificationsCount = data.Count(x => !x.HasRead);

                foreach (var notice in notices)
                {
                    var n = data.FirstOrDefault(f => f.Id == notice.Id);
                    notice.HasRead = n.HasRead;
                }
            }
            return notices;
        }

        public async Task InsertNotificationRecord(IEnumerable<NoticeDTO> notices)
        {
            var data = await notificationRepository.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
            var newData = notices
                    .Where(w => !data.Any(d => d.Id == w.Id))
                    .Select(s => new Entities.Notification()
                    {
                        Id = s.Id,
                        HasRead = false,
                        ExpirationTime = s.OverdueTime,
                    });
            var num = await notificationRepository.InsertRangeAsync(newData);
            if (num > 0)
            {
                UnreadNotificationsCount += num;
            }
        }

        public async Task MarkNotificationHasRead(params Entities.Notification[] notice)
        {
            await notificationRepository.UpdateRangeAsync(notice);
            UnreadNotificationsCount -= notice.Length;
            UnreadNotificationsCount = UnreadNotificationsCount < 0 ? 0 : UnreadNotificationsCount;
        }

        public async void DeleteExpiredRecordAsync()
        {
            var data = await notificationRepository.GetAllAsync(w => w.ExpirationTime < DateTimeOffset.Now);
            await notificationRepository.DeleteRangeAsync(data);
        }
    }
}
