using static BD.WTTS.Services.IMicroServiceClient;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class NoticeService : ReactiveObject
{
    static readonly Lazy<NoticeService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static NoticeService Current => mCurrent.Value;

    //readonly INotificationRepository notificationRepo = Ioc.Get<INotificationRepository>();

    [Reactive]
    public ObservableCollection<OfficialMessageItemDTO> Notices { get; set; }

    [Reactive]
    public int UnreadNotificationsCount { get; set; }

    //[Reactive]
    //public ObservableCollection<NoticeGroupDTO> NoticeTypes { get; set; }

    //public NoticeGroupDTO DefaultType { get; } = new NoticeGroupDTO() { Name = AppResources.AllTyoe };

    public NoticeService()
    {
        Notices = new ObservableCollection<OfficialMessageItemDTO>();
        //NoticesSource = new(x => x.Id);
        //NoticeTypes = new();
    }

    public async void GetNewsAsync()
    {
        var client = Instance.Message;
        var result = await client.GetMessage(IApplication.ClientPlatform, OfficialMessageType.News);
        if (result.IsSuccess && result.Content != null)
        {
            Notices.Clear();
            Notices.Add(result.Content.DataSource);
        }

        VerifyNotificationsHasRead();
    }

    public void VerifyNotificationsHasRead()
    {
        if (Notices.Any())
        {
            UnreadNotificationsCount = 0;
            foreach (var item in Notices)
            {
                if (item.PushTime > GeneralSettings.LastLookNoticeDateTime.Value)
                {
                    item.Unread = true;

                    UnreadNotificationsCount++;
                }
            }

            //只弹最新的一条通知
            if (UnreadNotificationsCount != 0 && GeneralSettings.MessagePopupNotification.Value)
            {
                //if (UnreadNotificationsCount > 1)
                //{
                //    INotificationService.Instance.Notify($"{UnreadNotificationsCount}条通知未读", NotificationType.Announcement);
                //    return;
                //}
                var notice = Notices.First(x => x.Unread == true);
                INotificationService.Instance.Notify(notice.Content, NotificationType.Announcement, title: notice.Title);
            }
        }
    }

    public void MarkNotificationsHasRead()
    {
        GeneralSettings.LastLookNoticeDateTime.Value = DateTimeOffset.Now;
        VerifyNotificationsHasRead();
    }

    //    public async Task GetNewsAsync()
    //    {
    //        var data = await notificationRepo.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
    //        UnreadNotificationsCount = data.Count(x => !x.HasRead);
    //        var client = IMicroServiceClient.Instance.Notice;
    //        var result = await client.NewMsg(null, null);
    //        if (result.IsSuccess && result.Content != null)
    //        {
    //            var noticeList = result.Content.Where(x => !data.Any(d => d.Id == x.Id));
    //            if (noticeList.Any_Nullable())
    //            {
    //                if (noticeList.Count() > 1)
    //                {
    //                    INotificationService.Instance.Notify(new NotificationBuilder()
    //                    {
    //                        Title = AppResources.Notice_Tray_Title,
    //                        Content = AppResources.Notice_Tray_Content_.Format(result.Content.Length),
    //                        AutoCancel = NotificationBuilder.DefaultAutoCancel,
    //                        Type = NotificationType.Announcement,
    //#pragma warning disable CS0618 // 类型或成员已过时
    //                        Click = new NotificationBuilderClickAction(() =>
    //                        {
    //                            IWindowManager.Instance.ShowAsync(AppEndPoint.Notice);
    //                        }),
    //#pragma warning restore CS0618 // 类型或成员已过时
    //                    });
    //                }
    //                else
    //                {
    //                    var notice = noticeList.First();
    //                    INotificationService.Instance.Notify(notice);
    //                }

    //                await InsertNotificationRecordAsync(noticeList);
    //            }

    //            DeleteExpiredRecord();
    //        }
    //    }

    //public async Task LoadNoticeTypesAsync()
    //{
    //    await Task.CompletedTask;
    //    //var client = IMicroServiceClient.Instance.Notice;
    //    //var result = await client.Types();
    //    //if (result.IsSuccess)
    //    //{
    //    //    NoticeTypes.Clear();
    //    //    NoticeTypes.Add(DefaultType);
    //    //    foreach (var item in result.Content!.OrderBy(x => x.Order)) // 服务端排序后返回？
    //    //        NoticeTypes.Add(item);
    //    //}
    //}

    //public async Task LoadNotificationAsync(NoticeGroupDTO? selectGroup)
    //{
    //    var client = IMicroServiceClient.Instance.Notice;
    //    var typeid = selectGroup == DefaultType ? null : selectGroup?.Id;
    //    var result = await client.Table(typeid, 1, 30);
    //    if (result.IsSuccess && result.Content != null)
    //    {
    //        //foreach (var item in result.Content!.DataSource)
    //        //{
    //        //    if (!string.IsNullOrWhiteSpace(item.Picture))
    //        //        item.PictureStream = httpService.GetImageAsync(item.Picture, ImageChannelType.NoticePicture);
    //        //}
    //        NoticesSource.Clear();
    //        var data = await LoadHasReadRecordAsync(result.Content.DataSource);
    //        NoticesSource.AddOrUpdate(data);
    //    }
    //}

    //public async Task<IEnumerable<NoticeDTO>> LoadHasReadRecordAsync(IEnumerable<NoticeDTO> notices)
    //{
    //    var data = await notificationRepo.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
    //    if (data.Any_Nullable())
    //    {
    //        UnreadNotificationsCount = data.Count(x => !x.HasRead);

    //        foreach (var notice in notices)
    //        {
    //            var d = data.FirstOrDefault(f => f.Id == notice.Id);
    //            if (d != null)
    //                notice.HasRead = d.HasRead;
    //            else
    //                notice.HasRead = true;
    //        }
    //    }
    //    return notices;
    //}

    //public async Task InsertNotificationRecordAsync(IEnumerable<NoticeDTO> notices)
    //{
    //    var data = await notificationRepo.GetAllAsync(w => w.ExpirationTime > DateTimeOffset.Now);
    //    var newData = notices
    //            .Where(w => !data.Any(d => d.Id == w.Id))
    //            .Select(s => new Notification()
    //            {
    //                Id = s.Id,
    //                HasRead = false,
    //                ExpirationTime = s.OverdueTime,
    //            });
    //    var num = await notificationRepo.InsertRangeAsync(newData);
    //    if (num > 0)
    //    {
    //        UnreadNotificationsCount += num;
    //    }
    //}

    //public async Task MarkNotificationHasReadAsync(params Notification[] notice)
    //{
    //    await notificationRepo.UpdateRangeAsync(notice);
    //    UnreadNotificationsCount -= notice.Length;
    //    UnreadNotificationsCount = UnreadNotificationsCount < 0 ? 0 : UnreadNotificationsCount;
    //}

    //public async void DeleteExpiredRecord()
    //{
    //    var data = await notificationRepo.GetAllAsync(w => w.ExpirationTime < DateTimeOffset.Now);
    //    await notificationRepo.DeleteRangeAsync(data);
    //}
}
