using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
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

        public NoticeWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            OpenNotice = ReactiveCommand.CreateFromTask<string>(Browser2.OpenAsync);
        }

        public void OpenNoticeWeb(NoticeDTO item)
        {
            if (item.IsOpenBrowser)
                Browser2.Open(string.Format(UrlConstants.OfficialWebsite_Notice, item.Id));
        }

        public override async void Activation()
        {
            if (IsFirstActivation && NotificationService.Current.NoticeTypes.Count == 0)
                await NotificationService.Current.InitializeNotice();
            base.Activation();
        }

    }
}