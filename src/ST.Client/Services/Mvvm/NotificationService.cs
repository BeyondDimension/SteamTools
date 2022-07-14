using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
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

        public SourceList<NoticeTypeDTO> NoticeTypesSource { get; }

        private ReadOnlyObservableCollection<NoticeTypeDTO>? _NoticeTypes;

        public ReadOnlyObservableCollection<NoticeTypeDTO>? NoticeTypes
        {
            get => _NoticeTypes;
            set => this.RaiseAndSetIfChanged(ref _NoticeTypes, value);
        }

        bool _IsLoading;

        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        bool _IsEmpty;

        public bool IsEmpty
        {
            get => _IsEmpty;
            set => this.RaiseAndSetIfChanged(ref _IsEmpty, value);
        }

        private NoticeTypeDTO? _SelectGroup;

        public NoticeTypeDTO? SelectGroup
        {
            get => _SelectGroup;
            set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
        }

        NoticeDTO _NoticeItem;

        public NoticeDTO NoticeItem
        {
            get => _NoticeItem;
            set => this.RaiseAndSetIfChanged(ref _NoticeItem, value);
        }

        public NotificationService()
        {
            mCurrent = this;
            NoticeTypesSource = new SourceList<NoticeTypeDTO>();
            this.WhenAnyValue(x => x.SelectGroup)
                .Subscribe(async x =>
                {
                    if (x != null)
                    {
                        IsLoading = true;
                        await GetTable(x);
                        IsLoading = false;
                    }
                });

            NoticeTypesSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<NoticeTypeDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _NoticeTypes)
                .Subscribe(_ =>
                {
                    SelectGroup = NoticeTypesSource.Items.FirstOrDefault();
                });

        }

        public async Task InitializeNotice()
        {
            IsLoading = true;
            await GetTypes();
            IsLoading = false;
        }

        public async Task GetNews(int trycount = 0)
        {
            if (NoticeTypesSource.Count > 0)
            {
                var lastTime = await INotificationService.GetLastNotificationTime();
                var basics = NoticeTypesSource.Items.FirstOrDefault();
                var client = ICloudServiceClient.Instance.Notice;
                var result = await client.NewMsg(basics.Id, null);
                if (result.IsSuccess && result.Content != null)
                {
                    var noticeList = result.Content.Where(x => lastTime.HasValue ? x.EnableTime > lastTime : true);
                    if (noticeList.Count() > 1)
                    {
                        await INotificationService.SetLastNotificationTime(noticeList.Max(x => x.EnableTime));
                        INotificationService.Instance.Notify(new NotificationBuilder()
                        {
                            Title = AppResources.NotificationChannelType_Description_Announcement,
                            Content = AppResources.Notice_Tray.Format(result.Content.Length, basics.Name),
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
            else if (trycount <= 2)
            {
                await GetTypes();
                await GetNews(trycount++);
            }
        }

        public async Task GetTypes()
        {
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.Types();
            if (result.IsSuccess)
            {
                NoticeTypesSource.Clear();
                NoticeTypesSource.AddRange(result.Content!.OrderBy(x => x.Index));
            }
        }

        public async Task GetTable(NoticeTypeDTO selectGroup)
        {
            if (selectGroup != null)
            {
                var client = ICloudServiceClient.Instance.Notice;
                var result = await client.Table(selectGroup.Id, selectGroup.Index);
                if (result.IsSuccess)
                {
                    foreach (var item in result.Content!.DataSource)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Picture))
                            item.PictureStream = httpService.GetImageAsync(item.Picture, ImageChannelType.NoticePicture);
                    }
                    SelectGroup!.Items = result.Content!;
                    NoticeItem = SelectGroup!.Items.DataSource.FirstOrDefault();
                }
                IsEmpty = SelectGroup!.Items == null || SelectGroup.Items?.DataSource.Count == 0;

            }
        }
    }
}
