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
using Titanium.Web.Proxy.Models;

namespace System.Application.Services
{
    public sealed class NotificationService : ReactiveObject
    {
        static NotificationService? mCurrent;
        public static NotificationService Current => mCurrent ?? new();

        readonly IHttpService httpService = DI.Get<IHttpService>();
        public SourceList<NoticeTypeDTO> NoticeTypes { get; }

        private ReadOnlyObservableCollection<NoticeTypeDTO>? _ObservableItems;
        public ReadOnlyObservableCollection<NoticeTypeDTO>? ObservableItems
        {
            get => _ObservableItems;
            set => this.RaiseAndSetIfChanged(ref _ObservableItems, value);
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
        public async Task GetNews(int trycount = 0)
        {
            if (NoticeTypes.Count > 0)
            {
                var lastTime = await INotificationService.GetLastNotificationTime();
                var basics = NoticeTypes.Items.FirstOrDefault();
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
                NoticeTypes.Clear();
                NoticeTypes.AddRange(result.Content!.OrderBy(x => x.Index));
            }
        }
        public async Task InitializeNotice()
        {
            using (var tk = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken()))
            {
                new Task(() =>
                {
                    Thread.Sleep(500);
                    if (!tk.IsCancellationRequested)
                        IsLoading = true;
                }, tk.Token).Start();
                await GetTypes();
                tk.Cancel();
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
                }
                IsEmpty = SelectGroup!.Items == null || SelectGroup.Items?.DataSource.Count == 0;

            }
        }

        public NotificationService()
        {
            mCurrent = this;
            NoticeTypes = new SourceList<NoticeTypeDTO>();
            this.WhenAnyValue(x => x.SelectGroup)
               .Subscribe(async x =>
               {
                   if (x != null)
                   {
                       //延迟500ms显示加载
                       using (var tk = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken()))
                       {
                           new Task(() =>
                           {
                               Thread.Sleep(500);
                               if (!tk.IsCancellationRequested)
                                   IsLoading = true;
                           }, tk.Token).Start();
                           await GetTable(x);
                           tk.Cancel();
                           IsLoading = false;
                       }
                   }
               });
            NoticeTypes
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<NoticeTypeDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _ObservableItems)
                .Subscribe(_ =>
               {
                   SelectGroup = NoticeTypes.Items.FirstOrDefault();

               });

        }
    }
}
