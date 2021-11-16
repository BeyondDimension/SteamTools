using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
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

        private ObservableCollection<RankingResponse>? _DonateList;
        public ObservableCollection<RankingResponse>? DonateList
        {
            get => _DonateList;
            set => this.RaiseAndSetIfChanged(ref _DonateList, value);
        }

        private DateTimeOffset _DonateFliterDate;
        public DateTimeOffset DonateFliterDate
        {
            get => _DonateFliterDate;
            set => this.RaiseAndSetIfChanged(ref _DonateFliterDate, value);
        }

        private const int DonateListPageSize = 50;

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
                var title = PageViewModel.GetTitleByDisplayName(AppResources.DelAccount);
                var r = await MessageBox.ShowAsync(AppResources.DelAccountTips, title, button: MessageBox.OKCancel);
                if (r.IsOK())
                {
                    var phoneNumber = (await IUserManager.Instance.GetCurrentUserAsync())?.PhoneNumber;
                    string verifyString, description, errorMessage;
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        verifyString = phoneNumber;
                        description = AppResources.DelAccount_VerifyDesc_PhoneNumber;
                        errorMessage = AppResources.DelAccount_VerifyError_PhoneNumber;
                    }
                    else
                    {
                        verifyString = UserService.Current.User?.NickName ?? string.Empty;
                        description = AppResources.DelAccount_VerifyDesc_NickName;
                        errorMessage = AppResources.DelAccount_VerifyError_NickName;
                    }
                    while (true)
                    {
                        var inputString = await TextBoxWindowViewModel.ShowDialogAsync(new()
                        {
                            Title = title,
                            Description = description,
                        });
                        if (inputString == null) break;
                        else if (inputString == verifyString)
                        {
                            await UserService.Current.DelAccountAsync();
                            break;
                        }
                        else
                        {
                            Toast.Show(errorMessage);
                        }
                    }
                }
            });

            DonateFliterDate = DateTimeOffset.Now.GetCurrentMonth();

            this.WhenValueChanged(x => x.DonateFliterDate, false)
                .Subscribe(x => LoadDonateRankListData(true));

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

        public override void Activation()
        {
            LoadDonateRankListData(true);
            base.Activation();
        }

        public override void Deactivation()
        {
            DonateList = null;
            base.Deactivation();
        }

        public async void LoadDonateRankListData(bool refresh = false, bool nextPage = false)
        {
            if (refresh)
                DonateList = null;

            DonateList ??= new();

            var pageIndex = 0;

            if (nextPage)
                pageIndex = DonateList.Count;


            var result = await ICloudServiceClient.Instance.DonateRanking.RangeQuery(new PageQueryRequest<RankingRequest>
            {
                Current = pageIndex,
                PageSize = DonateListPageSize,
                Params = new RankingRequest()
                {
                    TimeRange = new[] {
                        DonateFliterDate,DonateFliterDate.GetCurrentMonthLastDay(),
                    }
                }
            });

            if (result?.IsSuccess == true && result.Content != null)
            {
                if (DonateList.Count <= result.Content.Total)
                {
                    DonateList.AddRange(result.Content.DataSource);
                }
            }
            else
            {
                Toast.Show("加载捐助列表数据出错：" + result.Message);
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
