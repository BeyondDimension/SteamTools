using ReactiveUI;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class MainWindowViewModel : WindowViewModel
    {
        #region 更改通知

        bool mTopmost;
        public bool Topmost
        {
            get => mTopmost;
            set => this.RaiseAndSetIfChanged(ref mTopmost, value);
        }

        private ItemViewModel _SelectedItem;
        public ItemViewModel SelectedItem
        {
            get => _SelectedItem;
            set => this.RaiseAndSetIfChanged(ref _SelectedItem, value);
        }

        bool _IsOpenUserMenu;
        public bool IsOpenUserMenu
        {
            get => _IsOpenUserMenu;
            set => this.RaiseAndSetIfChanged(ref _IsOpenUserMenu, value);
        }

        public ReactiveCommand<Unit, Unit>? OpenUserMenu { get; }

        #endregion

        public StartPageViewModel StartPage => GetTabItemVM<StartPageViewModel>();

        public CommunityProxyPageViewModel CommunityProxyPage => GetTabItemVM<CommunityProxyPageViewModel>();

        public ProxyScriptManagePageViewModel ProxyScriptPage => GetTabItemVM<ProxyScriptManagePageViewModel>();

        public SteamAccountPageViewModel SteamAccountPage => GetTabItemVM<SteamAccountPageViewModel>();

        public GameListPageViewModel GameListPage => GetTabItemVM<GameListPageViewModel>();

        public LocalAuthPageViewModel LocalAuthPage => GetTabItemVM<LocalAuthPageViewModel>();

        public SteamIdlePageViewModel SteamIdlePage => GetTabItemVM<SteamIdlePageViewModel>();

        public ArchiSteamFarmPlusPageViewModel ASFPage => GetTabItemVM<ArchiSteamFarmPlusPageViewModel>();

        public GameRelatedPageViewModel GameRelatedPage => GetTabItemVM<GameRelatedPageViewModel>();

        public OtherPlatformPageViewModel OtherPlatformPage => GetTabItemVM<OtherPlatformPageViewModel>();

        protected static readonly IPlatformService platformService = IPlatformService.Instance;
        public MainWindowViewModel()
        {
            if (IApplication.IsDesktopPlatform)
            {
                var adminTag = platformService.IsAdministrator ? (OperatingSystem2.IsWindows ? " (Administrator)" : " (Root)") : string.Empty;
                var title = $"{ThisAssembly.AssemblyTrademark} {RuntimeInformation.ProcessArchitecture.ToString().ToLower()} v{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName}{adminTag}";
#if DEBUG
                title = $"[Debug] {title}";
#endif
                Title = title;

                IUserManager.Instance.OnSignOut += () =>
                {
                    IsOpenUserMenu = false;
                };

                OpenUserMenu = ReactiveCommand.Create(() =>
                {
                    IsOpenUserMenu = UserService.Current.IsAuthenticated;
                    if (!IsOpenUserMenu)
                    {
                        UserService.Current.ShowWindow(CustomWindow.LoginOrRegister);
                    }
                });
            }

            #region InitTabItems


            //AddTabItem<StartPageViewModel>();
            AddTabItem<CommunityProxyPageViewModel>();
            if (IApplication.IsDesktopPlatform)
            {
                AddTabItem<ProxyScriptManagePageViewModel>();
                AddTabItem<SteamAccountPageViewModel>();
                AddTabItem<GameListPageViewModel>();
            }
            AddTabItem<LocalAuthPageViewModel>();
            AddTabItem<ArchiSteamFarmPlusPageViewModel>();

            //AddTabItem<SteamIdlePageViewModel>();
#if !TRAY_INDEPENDENT_PROGRAM
            if (OperatingSystem2.IsWindows)
                AddTabItem<GameRelatedPageViewModel>();
#endif
            //AddTabItem<OtherPlatformPageViewModel>();

#if !TRAY_INDEPENDENT_PROGRAM && DEBUG
            if (IApplication.EnableDevtools && IApplication.IsDesktopPlatform)
            {
                AddTabItem<DebugPageViewModel>();
                //FooterTabItems.Add(new DebugPageViewModel().AddTo(this));

                //if (AppHelper.IsSystemWebViewAvailable)
                //{
                //    AddTabItem<DebugWebViewPageViewModel>();
                //}
            }
#endif

            #endregion

            _SelectedItem = TabItems.First();

            R.Subscribe(() =>
            {
                foreach (var item in CurrentAllTabItems)
                {
                    item.RaisePropertyChanged(nameof(TabItemViewModelBase.Name));
                }
            }).AddTo(this);
        }

        public override void Initialize()
        {
            Task.Run(() =>
            {
                Threading.Thread.CurrentThread.IsBackground = true;
                if (!IsInitialized)
                {
                    Task.Run(async () =>
                    {
                        await ProxyService.Current.Initialize();
                        if (ASFSettings.AutoRunArchiSteamFarm.Value)
                        {
                            if (platformService.UsePlatformForegroundService)
                            {
                                await platformService.StartOrStopForegroundServiceAsync(nameof(ASFService), true);
                            }
                            else
                            {
                                await ASFService.Current.InitASF();
                            }
                        }
                    });

                    if (IApplication.IsDesktopPlatform)
                    {
                        SteamConnectService.Current.Initialize();
                    }

                    Parallel.ForEach(TabItems, item =>
                    {
                        item.Initialize();
                        //Task.Run(item.Initialize).ForgetAndDispose();
                    });
                    IsInitialized = true;
                }
            }).ForgetAndDispose();
        }

        //public async override void Activation()
        //{
        //    if (IsFirstActivation)
        //    {
        //        if (UISettings.DoNotShowMessageBoxs.Value?.Contains(MessageBox.DontPromptType.Donate) == false)
        //        {
        //            //INotificationService.Instance.Notify("如果你觉得Steam++好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", NotificationType.Message);
        //            await MessageBox.ShowAsync("如果你觉得Steam++好用，你可以考虑给我们一些捐助以支持我们继续开发，谢谢！", button: MessageBox.Button.OK,
        //                rememberChooseKey: MessageBox.DontPromptType.Donate);
        //        }
        //    }
        //    base.Activation();
        //}
    }
}