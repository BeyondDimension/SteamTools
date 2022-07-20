using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class NoticeWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.NotificationChannelType_Name_Announcement;

        private ReadOnlyObservableCollection<NoticeDTO> _Notices;

        public ReadOnlyObservableCollection<NoticeDTO> Notices => _Notices;

        [Reactive]
        public bool IsLoading { get; set; }

        public bool IsEmpty => !Notices.Any_Nullable();

        [Reactive]
        public NoticeTypeDTO? SelectType { get; set; }

        [Reactive]
        public NoticeDTO? NoticeItem { get; set; }

        public ReactiveCommand<Unit, Unit>? MarkAllRead { get; }

        public NoticeWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            MarkAllRead = ReactiveCommand.CreateFromTask(async () =>
            {
                var data = NotificationService.Current.NoticesSource.Items.Select(s => new Entities.Notification
                {
                    Id = s.Id,
                    ExpirationTime = s.OverdueTime,
                    HasRead = true
                }).ToArray();
                await NotificationService.Current.MarkNotificationHasRead(data);

                foreach (var item in NotificationService.Current.NoticesSource.Items)
                {
                    item.HasRead = true;
                }
            });

            NotificationService.Current.NoticesSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<NoticeDTO>.Ascending(x => x.EnableTime).ThenBy(x => x.Title))
                .Bind(out _Notices)
                .Subscribe(_ =>
                {
                    NoticeItem = Notices.FirstOrDefault();
                    this.RaisePropertyChanged(nameof(IsEmpty));
                });

            this.WhenValueChanged(x => x.SelectType, false)
                .Subscribe(async x =>
                {
                    IsLoading = true;
                    await NotificationService.Current.LoadNotification(x);
                    IsLoading = false;
                });

            this.WhenValueChanged(x => x.NoticeItem)
                .Subscribe(x =>
                {
                    if (x != null && !x.HasRead)
                    {
                        _ = NotificationService.Current.MarkNotificationHasRead(new Entities.Notification
                        {
                            Id = x.Id,
                            ExpirationTime = x.OverdueTime,
                            HasRead = true
                        });
                        x.HasRead = true;
                    }
                });
        }

        public override async void Activation()
        {
            //if (IsFirstActivation)
            IsLoading = true;
            await NotificationService.Current.LoadNoticeTypes();
            SelectType = NotificationService.Current.NoticeTypes.FirstOrDefault();
            await NotificationService.Current.LoadNotification(SelectType);
            IsLoading = false;
            base.Activation();
        }

        public void OpenNoticeWeb(NoticeDTO item)
        {
            if (item.IsOpenBrowser)
                Browser2.Open(string.Format(UrlConstants.OfficialWebsite_Notice, item.Id));
        }
    }
}