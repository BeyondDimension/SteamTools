using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Security;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public partial class LocalAuthPageViewModel
    {
        public LocalAuthPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(LocalAuthPageViewModel);
#endif

            OpenBrowserCommand = ReactiveCommand.Create<string>(Services.CloudService.Constants.BrowserOpen);
            AddAuthCommand = ReactiveCommand.Create(AddAuthMenu_Click);
            RefreshAuthCommand = ReactiveCommand.CreateFromTask(async () =>
            {
#if __MOBILE__
                IsRefreshing = true;
#endif
                await AuthService.Current.InitializeAsync(true);
#if __MOBILE__
                IsRefreshing = false;
#endif
            });
            EncryptionAuthCommand = ReactiveCommand.Create(ShowEncryptionAuthWindow);
            ExportAuthCommand = ReactiveCommand.Create(ShowExportAuthAuthWindow);
            LockCommand = ReactiveCommand.Create(async () =>
            {
                var result = await AuthService.Current.HasPasswordEncryption();
                if (result)
                {
                    AuthService.Current.Authenticators.Clear();
#if !__MOBILE__
                    MenuItems?.Clear();
#endif
                    Activation();
                }
                else
                {
                    Toast.Show(AppResources.LocalAuth_LockError);
                }
            });

#if !__MOBILE__
            //MenuItems = new ObservableCollection<MenuItemViewModel>()
            //{
            //    //new MenuItemViewModel(nameof(AppResources.LocalAuth_EditAuth))
            //    //{
            //    //    Items = new[]
            //    //    {
            //            new MenuItemViewModel(nameof(AppResources.Add)) { IconKey="AddDrawing",
            //                Command = AddAuthCommand },
            //            new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="ShieldLockDrawing",
            //                Command = EncryptionAuthCommand },
            //            //new MenuItemViewModel(nameof(AppResources.Edit)) { IconKey="EditDrawing" },
            //            new MenuItemViewModel(nameof(AppResources.Export)) { IconKey="ExportDrawing",
            //                Command = ExportAuthCommand  },
            //            new MenuItemSeparator(),
            //            new MenuItemViewModel(nameof(AppResources.Lock)) {IconKey="LockDrawing",
            //                Command = LockCommand },
            //            new MenuItemViewModel(nameof(AppResources.Refresh)) {IconKey="RefreshDrawing",
            //                Command = RefreshAuthCommand },
            //            //new MenuItemViewModel(),
            //            //new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="LockDrawing" },
            //            //new MenuItemViewModel(nameof(AppResources.CloudSync)) {IconKey="CloudDrawing" },
            //    //    }
            //    //},
            //};
#endif

            AuthService.Current.Authenticators
                .Connect()
                //.Filter(scriptFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<MyAuthenticator>.Ascending(x => x.Index).ThenBy(x => x.Name))
                .Bind(out _Authenticators)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAuthenticatorsEmpty)));
        }

        private readonly ReadOnlyObservableCollection<MyAuthenticator> _Authenticators;
        public ReadOnlyObservableCollection<MyAuthenticator> Authenticators => _Authenticators;

        public bool IsAuthenticatorsEmpty => !AuthService.Current.Authenticators.Items.Any_Nullable();

        [Obsolete("use IsAuthenticatorsEmptyButHasSourceAuths")] // AndHasPassword ???
        public bool IsAuthenticatorsEmptyAndHasPassword => SourceAuthCount > 0 && IsAuthenticatorsEmpty;

        public bool IsAuthenticatorsEmptyButHasSourceAuths => SourceAuthCount > 0 && IsAuthenticatorsEmpty;

        public int SourceAuthCount { get; set; }

        public ReactiveCommand<Unit, Unit> AddAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> EncryptionAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> ExportAuthCommand { get; }

        public ReactiveCommand<Unit, Task> LockCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshAuthCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

#if __MOBILE__
        private bool _IsFirstLoadedAuthenticatorsEmpty;
        /// <summary>
        /// 是否第一次加载完成时令牌集合数据为空
        /// </summary>
        public bool IsFirstLoadedAuthenticatorsEmpty
        {
            get => _IsFirstLoadedAuthenticatorsEmpty;
            set => this.RaiseAndSetIfChanged(ref _IsFirstLoadedAuthenticatorsEmpty, value);
        }

        private bool _IsRefreshing;
        /// <summary>
        /// 是否正在刷新中
        /// </summary>
        public bool IsRefreshing
        {
            get => _IsRefreshing;
            set => this.RaiseAndSetIfChanged(ref _IsRefreshing, value);
        }
#endif

        public async override void Activation()
        {
            var auths = await AuthService.Current.Repository.GetAllSourceAsync();
            SourceAuthCount = auths.Length;
            //if (IsFirstActivation)
            if (IsAuthenticatorsEmptyButHasSourceAuths)
            {
                await AuthService.Current.InitializeAsync(auths);
#if !__MOBILE__
                if (Authenticators.Any())
                {
                    MenuItems = new ObservableCollection<MenuItemViewModel>();
                    foreach (var auth in Authenticators)
                    {
                        MenuItems.Add(new MenuItemCustomName(auth.Name, AppResources.LocalAuth_Copy)
                        {
                            Command = ReactiveCommand.Create(() =>
                            {
                                auth.CopyCodeCilp();
                                INotificationService.Instance.Notify(AppResources.LocalAuth_CopyAuthTip + auth.Name, NotificationType.Message);
                            }),
                        });
                    }
                    this.RaisePropertyChanged(nameof(IsTaskBarSubMenu));
                }
#endif
            }
#if __MOBILE__
            else
            {
                IsFirstLoadedAuthenticatorsEmpty = true;
            }
#endif
            base.Activation();
        }

        //#if !__MOBILE__
        //        public async void Refreshing()
        //        {
        //            await AuthService.Current.InitializeAsync();
        //        }
        //#endif

        void AddAuthMenu_Click()
        {
            if (!IsNotOfficialChannelPackageDetectionHelper.Check()) return;
            IShowWindowService.Instance.Show<AddAuthWindowViewModel>(CustomWindow.AddAuth, resizeMode: ResizeModeCompat.CanResize);
        }

#if !__MOBILE__
        public void ShowAuthCode(MyAuthenticator auth)
        {
            if (auth.IsShowCode == false)
            {
                auth.IsShowCode = true;
                var max = 100;
                auth.CodeCountdown = max;
                Task.Run(async () =>
                {
                    while (auth.IsShowCode)
                    {
                        //auth.CurrentCode = string.Empty;
                        auth.RefreshCode();
                        auth.CodeCountdown -= 5;
                        if (auth.CodeCountdown == 0)
                        {
                            auth.CodeCountdown = max;
                            auth.IsShowCode = false;
                        }
                        await Task.Delay(100);
                    }
                }).ContinueWith(s => s.Dispose());
            }
        }
#endif

        public void CopyCodeCilp(MyAuthenticator auth) => auth.CopyCodeCilp();

#if !__MOBILE__
        public void DeleteAuth(MyAuthenticator auth) => DeleteAuthCore(auth);
#endif

        public async void DeleteAuthCore(MyAuthenticator auth, Action? okAction = null)
        {
            var (success, _) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success) return;
            var r = await MessageBoxCompat.ShowAsync(@AppResources.LocalAuth_DeleteAuthTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
            if (r == MessageBoxResultCompat.OK)
            {
                AuthService.DeleteSaveAuthenticators(auth);
                okAction?.Invoke();
            }
        }

        public async void ShowSteamAuthData(MyAuthenticator auth)
        {
            var (success, _) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success) return;
            switch (auth.AuthenticatorData.Platform)
            {
                case GamePlatform.Steam:
                    await IShowWindowService.Instance.Show(CustomWindow.ShowAuth,
#if __MOBILE__
                        new MyAuthenticatorWrapper(auth),
#else
                        new ShowAuthWindowViewModel(auth),
#endif
                        string.Empty, ResizeModeCompat.CanResize);
                    break;
            }
        }

        public void ShowSteamAuthTrade(MyAuthenticator auth)
        {
            switch (auth.AuthenticatorData.Platform)
            {
                case GamePlatform.Steam:
                    IShowWindowService.Instance.Show(CustomWindow.AuthTrade,
#if __MOBILE__
                        new MyAuthenticatorWrapper(auth),
#else
                        new AuthTradeWindowViewModel(auth),
#endif
                        string.Empty, ResizeModeCompat.CanResize);
                    break;
            }
        }

        async void ShowEncryptionAuthWindow()
        {
            //if (await AuthService.Current.HasPasswordEncryptionShowPassWordWindow())
            //{
            await IShowWindowService.Instance.Show(CustomWindow.EncryptionAuth,
#if __MOBILE__
                (PageViewModel?)null,
#else
                new EncryptionAuthWindowViewModel(),
#endif
                string.Empty, ResizeModeCompat.CanResize);
            //}
        }

        async void ShowExportAuthAuthWindow()
        {
            //if (await AuthService.Current.HasPasswordEncryptionShowPassWordWindow())
            //{
            await IShowWindowService.Instance.Show(CustomWindow.ExportAuth,
#if __MOBILE__
                (PageViewModel?)null,
#else
                new ExportAuthWindowViewModel(),
#endif
                string.Empty, ResizeModeCompat.CanResize);
            //}
        }

        public void UpMoveOrderById(MyAuthenticator auth)
        {
            MoveOrderById(auth, true);
        }

        public void DownMoveOrderById(MyAuthenticator auth)
        {
            MoveOrderById(auth, false);
        }

        async void MoveOrderById(MyAuthenticator auth, bool upOrDown)
        {
            var repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();
            var row = await repository.MoveOrderByItemAsync(x => x.AuthenticatorData, _Authenticators, auth, upOrDown);
            await AuthService.Current.InitializeAsync(false);
        }
    }
}