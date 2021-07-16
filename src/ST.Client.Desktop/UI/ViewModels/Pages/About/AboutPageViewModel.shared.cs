using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
//#if __MOBILE__
//using Xamarin.Essentials;
//#endif
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public partial class AboutPageViewModel
    {
        static string TitleName => AppResources.About;

        public AboutPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(AboutPageViewModel).Replace("ViewModel", "Svg");
#endif

            OpenBrowserCommand = ReactiveCommand.Create<string>(BrowserOpen);

            //#if !__MOBILE__
            //            CopyLinkCommand =
            //#if __MOBILE__
            //                ReactiveCommand.CreateFromTask<string>(Clipboard.SetTextAsync);
            //#else
            //                ReactiveCommand.Create<string>(IDesktopAppService.Instance.SetClipboardText);
            //#endif
            //#endif

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
                    await UserService.Current.DelAccountAsync();
                }
            });

#if __MOBILE__
            preferenceButtons = new()
            {
                PreferenceButtonViewModel.Create(PreferenceButton.检查更新, this),
                PreferenceButtonViewModel.Create(PreferenceButton.更新日志, this),
                PreferenceButtonViewModel.Create(PreferenceButton.常见问题疑难解答, this),
                PreferenceButtonViewModel.Create(PreferenceButton.开放源代码许可, this),
                PreferenceButtonViewModel.Create(PreferenceButton.源码仓库, this),
                PreferenceButtonViewModel.Create(PreferenceButton.产品官网, this),
                PreferenceButtonViewModel.Create(PreferenceButton.联系我们, this),
                PreferenceButtonViewModel.Create(PreferenceButton.Bug反馈, this),
            };

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
#endif
        }

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        //#if !__MOBILE__
        //        public ReactiveCommand<string, Unit> CopyLinkCommand { get; }
        //#endif

        public ReactiveCommand<Unit, Unit> DelAccountCommand { get; }

        static string GetOS() => DI.Platform switch
        {
            Platform.Windows => "Windows",
            Platform.Linux => "Linux",
            Platform.Android => "Android",
            Platform.Apple => DI.DeviceIdiom switch
            {
                DeviceIdiom.Phone => "iOS",
                DeviceIdiom.Tablet => "iPadOS",
                DeviceIdiom.Desktop => "macOS",
                DeviceIdiom.TV => "tvOS",
                DeviceIdiom.Watch => "watchOS",
                _ => string.Empty,
            },
            Platform.UWP => "UWP",
            _ => string.Empty,
        };

        public string VersionDisplay => $"{ThisAssembly.VersionDisplay} for {GetOS()} ({RuntimeInformation.ProcessArchitecture})";

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

        public ICommand ContributorsCommand { get; } = ReactiveCommand.CreateFromTask<string?>(async (p, _) =>
        {
            switch (p)
            {
                case "沙中金":
                    await Email2.ComposeAsync(new() { To = new() { "sanextraction@gmail.com" } });
                    break;
            }
        });
    }
}