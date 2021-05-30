using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;
using System.Reactive;
using System.Windows;
using static System.Application.Services.CloudService.Constants;

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

            OpenBrowserCommand = ReactiveCommand.Create<string>(BrowserOpen);

            CopyLinkCommand = ReactiveCommand.Create<string>(IDesktopAppService.Instance.SetClipboardText);

            CheckUpdateCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await IAppUpdateService.Instance.CheckUpdateAsync(showIsExistUpdateFalse: true);
            });

            DelAccountCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!UserService.Current.IsAuthenticated) return;
                var r = await MessageBoxCompat.ShowAsync(AppResources.DelAccountTips, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
                if (r == MessageBoxResultCompat.OK)
                {
                    UserService.Current.DelAccount();
                }
            });
        }

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public ReactiveCommand<string, Unit> CopyLinkCommand { get; }

        public ReactiveCommand<Unit, Unit> DelAccountCommand { get; }

        public string VersionDisplay => ThisAssembly.VersionDisplay;

        public string LabelVersionDisplay => ThisAssembly.IsBetaRelease ? "Beta Version:" : "Current Version:";

        public string Copyright
        {
            get
            {
                // https://www.w3cschool.cn/html/html-copyright.html
                int startYear = 2020, thisYear = 2021;
                var nowYear = DateTime.Now.Year;
                if (nowYear < thisYear) nowYear = thisYear;
                return $"© {startYear}{(nowYear == startYear ? startYear : "-" + nowYear)} {ThisAssembly.AssemblyCompany}. All Rights Reserved.";
            }
        }

        public static string RmbadminSteamLink => SteamApiUrls.MY_PROFILE_URL;

        public static string RmbadminLink => "https://github.com/rmbadmin";

        public static string AigioLLink => "https://github.com/AigioL";

        public static string MossimosLink => "https://github.com/Mossimos";

        public static string CliencerLink => "https://space.bilibili.com/30031316";

        public static string PrivacyLink => "https://steampp.net/privacy";

        public static string AgreementLink => "https://steampp.net/agreement";

        public static string OfficialLink => "https://steampp.net";

        public static string SourceCodeLink => "https://github.com/rmbadmin/SteamTools";

        public static string UserSupportLink => "https://steampp.net/contact";

        public static string BugReportLink => "https://github.com/rmbadmin/SteamTools/issues";

        public static string FAQLink => "https://steampp.net/faq";

        public static string LicenseLink => "https://github.com/rmbadmin/SteamTools/blob/develop/LICENSE";
    }
}