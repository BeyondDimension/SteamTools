using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AboutPageViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public ReactiveCommand<Unit, Unit> DelAccountCommand { get; }

        public AboutPageViewModel()
        {
            IconKey = nameof(AboutPageViewModel);

            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(Browser2.OpenAsync);

            CheckUpdateCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await IApplicationUpdateService.Instance.CheckUpdateAsync(showIsExistUpdateFalse: true);
            });

            DelAccountCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!UserService.Current.IsAuthenticated) return;
                var r = await MessageBox.ShowAsync(AppResources.DelAccountTips, button: MessageBox.OKCancel);
                if (r.IsOK())
                {
                    await UserService.Current.DelAccountAsync();
                }
            });

            if (IsMobileLayout)
            {
                preferenceButtons = new(Enum2.GetAll<PreferenceButton>().Select(x => PreferenceButtonViewModel.Create(x, this)));

                UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
                {
                    if (value == null)
                    {
                        PreferenceButtonViewModel.RemoveAuthorized(preferenceButtons, this);
                    }
                    else
                    {
                        var delAccount = preferenceButtons.FirstOrDefault(x => x.Id == PreferenceButton.账号注销);
                        if (delAccount == null)
                        {
                            delAccount = PreferenceButtonViewModel.Create(PreferenceButton.账号注销, this);
                            preferenceButtons.Add(delAccount);
                        }
                    }
                }).AddTo(this);
            }
        }

        public string VersionDisplay => $"{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";

        public string LabelVersionDisplay => ThisAssembly.IsAlphaRelease ? "Alpha Version:" : (ThisAssembly.IsBetaRelease ? "Beta Version:" : "Current Version:");

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

        public const string Zhengye = "Zhengye";
        public const string 沙中金 = "沙中金";
        public const string EspRoy = "EspRoy";

        //public ICommand ContributorsCommand { get; } = ReactiveCommand.CreateFromTask<string?>(async (p, _) =>
        //{
        //    switch (p)
        //    {
        //        case 沙中金:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //        case EspRoy:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //    }
        //});

        #region Urls

        public static string RmbadminSteamLink => SteamApiUrls.MY_PROFILE_URL;

        public static string RmbadminLink => UrlConstants.GitHub_User_Rmbadmin;

        public static string AigioLLink => UrlConstants.GitHub_User_AigioL;

        public static string MossimosLink => UrlConstants.GitHub_User_Mossimos;

        public static string CliencerLink => UrlConstants.BILI_User_Cliencer;

        public static string PrivacyLink => UrlConstants.OfficialWebsite_Privacy;

        public static string AgreementLink => UrlConstants.OfficialWebsite_Agreement;

        public static string OfficialLink => UrlConstants.OfficialWebsite;

        public static string SourceCodeLink => UrlConstants.GitHub_Repository;

        public static string UserSupportLink => UrlConstants.OfficialWebsite_Contact;

        public static string BugReportLink => UrlConstants.GitHub_Issues;

        public static string FAQLink => UrlConstants.OfficialWebsite_Faq;

        public static string ChangeLogLink => UrlConstants.OfficialWebsite_Changelog;

        public static string LicenseLink => UrlConstants.License_GPLv3;

        public static string MicrosoftStoreReviewLink => UrlConstants.MicrosoftStoreReviewLink;

        #endregion
    }
}
