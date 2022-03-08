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
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AboutPageViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public ReactiveCommand<Unit, Unit> DelAccountCommand { get; }

        public ICommand UIDCommand { get; }

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

        public static readonly DateTimeOffset StartYear = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset ThisYear = new DateTimeOffset(DateTimeOffset.Now.Year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(-1);

        const int DonateListPageSize = 500;

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

            UIDCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var uid = UserService.Current.User?.Id;
                if (uid.HasValue)
                {
                    await IApplication.CopyToClipboardAsync(uid.Value.ToString());
                }
                else
                {
                    Toast.Show(AppResources.YouNeedSignInToGetUID);
                }
            });

            if (!IApplication.IsDesktopPlatform)
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

            DonateFliterDate = DateTimeOffset.Now.GetCurrentMonth();

            this.WhenValueChanged(x => x.DonateFliterDate, false)
                .Subscribe(x => LoadDonateRankListData(true));
        }

        public override void Activation()
        {
            if (IApplication.IsDesktopPlatform) // Android 上手动调用加载
            {
                LoadDonateRankListData(true);
            }
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
            {
                if (IApplication.IsDesktopPlatform)
                {
                    DonateList = null;
                }
                else
                {
                    // Android 上 List 关联 Adapter 在初始化时，这里不能执行 null 赋值
                    DonateList?.Clear();
                }
            }

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
                        DonateFliterDate, DonateFliterDate.GetCurrentMonthLastDay(),
                    },
                }
            });

            if (result.TryGetContent(out var content))
            {
                if (DonateList.Count <= content.Total)
                {
                    if (content.DataSource.Any())
                    {
                        DonateList.AddRange(content.DataSource);
                    }
#if DEBUG
                    else
                    {
                        if (DonateFliterDate.Year == 2022 && DonateFliterDate.Month == 1)
                        {
                            var mockDatas = new[]
                            {
                                new RankingResponse
                                {
                                    Name = "Name1",
                                    Amount = 648,
                                    CurrencyCode = CurrencyCode.USD,
                                    Avatar = "https://cn.bing.com/th?id=OHR.WhalesDolphins_EN-CN1280683758_1920x1080.jpg&rf=LaDigue_1920x1080.jpg",
                                },
                                new RankingResponse
                                {
                                    Name = "Name2",
                                    Amount = 99,
                                    CurrencyCode = CurrencyCode.CNY,
                                    Avatar = "https://cn.bing.com/th?id=OHR.WhalesDolphins_EN-CN1280683758_1920x1080.jpg&rf=LaDigue_1920x1080.jpg",
                                },
                                new RankingResponse
                                {
                                    Name = "Name3",
                                    Amount = 1500,
                                    CurrencyCode = CurrencyCode.CNY,
                                    Avatar = "https://cn.bing.com/th?id=OHR.WhalesDolphins_EN-CN1280683758_1920x1080.jpg&rf=LaDigue_1920x1080.jpg",
                                },
                                new RankingResponse
                                {
                                    Name = "Name4",
                                    Amount = 666,
                                    CurrencyCode = CurrencyCode.USD,
                                    Avatar = "https://cn.bing.com/th?id=OHR.WhalesDolphins_EN-CN1280683758_1920x1080.jpg&rf=LaDigue_1920x1080.jpg",
                                },
                                new RankingResponse
                                {
                                    Name = "Name5",
                                    Amount = 999,
                                    CurrencyCode = CurrencyCode.USD,
                                    Avatar = "https://cn.bing.com/th?id=OHR.WhalesDolphins_EN-CN1280683758_1920x1080.jpg&rf=LaDigue_1920x1080.jpg",
                                },
                            };
                            DonateList.AddRange(mockDatas);
                        }
                    }
#endif
                }
            }
            else
            {
                Toast.Show(AppResources.About_DonateRecord_Error + result.Message);
            }
        }

        public string VersionDisplay => $"{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";

        public string LabelVersionDisplay => ThisAssembly.IsAlphaRelease ? "Alpha Version:" : (ThisAssembly.IsBetaRelease ? "Beta Version:" : "Current Version:");

        public static string Copyright
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
