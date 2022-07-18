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

        public ReactiveCommand<string, Unit>? OpenNotice { get; }

        private ReadOnlyObservableCollection<NoticeDTO> _Notices;

        public ReadOnlyObservableCollection<NoticeDTO> Notices => _Notices;

        [Reactive]
        public bool IsLoading { get; set; }

        public bool IsEmpty => !Notices.Any_Nullable();

        [Reactive]
        public NoticeTypeDTO? SelectGroup { get; set; }

        [Reactive]
        public NoticeDTO NoticeItem { get; set; }

        public NoticeWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            OpenNotice = ReactiveCommand.CreateFromTask<string>(x => Browser2.OpenAsync(x));

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

            this.WhenValueChanged(x => x.SelectGroup, false)
                .Subscribe(async x =>
                {
                    IsLoading = true;
                    await NotificationService.Current.LoadNotification(x);
                    IsLoading = false;
                });
        }

        public void OpenNoticeWeb(NoticeDTO item)
        {
            if (item.IsOpenBrowser)
                Browser2.Open(string.Format(UrlConstants.OfficialWebsite_Notice, item.Id));
        }

        public override async void Activation()
        {
            //if (IsFirstActivation)
            await NotificationService.Current.LoadNoticeTypes();
            await NotificationService.Current.LoadNotification(SelectGroup);
            base.Activation();
        }

    }
}