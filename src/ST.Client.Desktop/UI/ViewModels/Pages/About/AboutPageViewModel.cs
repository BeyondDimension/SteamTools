using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.CommandLine;
using System.Properties;
using System.Reactive;

namespace System.Application.UI.ViewModels
{
    public class AboutPageViewModel : TabItemViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public override string Name
        {
            get => AppResources.About;
            protected set { throw new NotImplementedException(); }
        }

        public AboutPageViewModel()
        {
            IconKey = nameof(AboutPageViewModel).Replace("ViewModel", "Svg");

            OpenBrowserCommand = ReactiveCommand.Create<string>(
              (link) => System.Application.Services.CloudService.Constants.BrowserOpen(link));

            CopyLinkCommand = ReactiveCommand.Create<string>(
                (link) => DI.Get<IDesktopAppService>().SetClipboardText(link));
        }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public ReactiveCommand<string, Unit> CopyLinkCommand { get; }


        public Version ClientVersion => ThisAssembly.ClientVersion;

        public static string ClearnetLink => "https://steampp.net/";

        public static string SourceCodeLink => "https://github.com/rmbadmin/SteamTools";

        public static string UserSupportLink => "https://steampp.net/contact";

        public static string BugReportLink => "https://github.com/rmbadmin/SteamTools/issues";

        public static string FAQLink => "https://steampp.net/faq";

        public static string DocsLink => "https://docs.wasabiwallet.io/";

        public static string LicenseLink => "https://github.com/rmbadmin/SteamTools/blob/develop/LICENSE";
    }
}
