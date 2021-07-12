using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
#if __MOBILE__
using XEClipboard = Xamarin.Essentials.Clipboard;
#endif

namespace System.Application.UI.ViewModels
{
    public partial class LocalAuthPageViewModel
    {
        public static string DisplayName => AppResources.LocalAuth;

        public LocalAuthPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(LocalAuthPageViewModel).Replace("ViewModel", "Svg");
#endif

            AddAuthCommand = ReactiveCommand.Create(AddAuthMenu_Click);
            RefreshAuthCommand = ReactiveCommand.Create(() => AuthService.Current.Initialize(true));
            EncryptionAuthCommand = ReactiveCommand.Create(ShowEncryptionAuthWindow);
            ExportAuthCommand = ReactiveCommand.Create(ShowExportAuthAuthWindow);
            LockCommand = ReactiveCommand.Create(async () =>
            {
                var result = await AuthService.Current.HasPasswordEncryption();
                if (result)
                {
                    AuthService.Current.Authenticators.Clear();
                    Activation();
                }
                else
                {
                    Toast.Show(AppResources.LocalAuth_LockError);
                }
            });

#if !__MOBILE__
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                //new MenuItemViewModel(nameof(AppResources.LocalAuth_EditAuth))
                //{
                //    Items = new[]
                //    {
                        new MenuItemViewModel(nameof(AppResources.Add)) { IconKey="AddDrawing",
                            Command = AddAuthCommand },
                        new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="ShieldLockDrawing",
                            Command = EncryptionAuthCommand },
                        //new MenuItemViewModel(nameof(AppResources.Edit)) { IconKey="EditDrawing" },
                        new MenuItemViewModel(nameof(AppResources.Export)) { IconKey="ExportDrawing",
                            Command = ExportAuthCommand  },
                        new MenuItemViewModel(),
                        new MenuItemViewModel(nameof(AppResources.Lock)) {IconKey="LockDrawing",
                            Command = LockCommand },
                        new MenuItemViewModel(nameof(AppResources.Refresh)) {IconKey="RefreshDrawing",
                            Command = RefreshAuthCommand },
                        //new MenuItemViewModel(),
                        //new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="LockDrawing" },
                        //new MenuItemViewModel(nameof(AppResources.CloudSync)) {IconKey="CloudDrawing" },
                //    }
                //},
            };
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

        public bool IsAuthenticatorsEmptyAndHasPassword => SourceAuthCount > 0 && IsAuthenticatorsEmpty == true;

        public int SourceAuthCount { get; set; }

        public ReactiveCommand<Unit, Unit> AddAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> EncryptionAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> ExportAuthCommand { get; }

        public ReactiveCommand<Unit, Task> LockCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshAuthCommand { get; }

        public async override void Activation()
        {
            SourceAuthCount = await AuthService.Current.GetRealAuthenticatorCount();
            //if (IsFirstActivation)
            if (IsAuthenticatorsEmptyAndHasPassword)
                AuthService.Current.Initialize();
            base.Activation();
        }

        public void Refreshing()
        {
            AuthService.Current.Initialize();
        }

        void AddAuthMenu_Click()
        {
            if (DI.DeviceIdiom == DeviceIdiom.Desktop && !AppSettings.IsOfficialChannelPackage) return;
            IShowWindowService.Instance.Show<AddAuthWindowViewModel>(CustomWindow.AddAuth,resizeMode:ResizeModeCompat.CanResize);
        }

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
                        auth.CurrentCode = string.Empty;
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

#if __MOBILE__
        async void SetClipboardText(string s) => await XEClipboard.SetTextAsync(s);
#endif

        public void CopyCodeCilp(MyAuthenticator auth)
        {
            var s = auth.CurrentCode;
#if __MOBILE__
            SetClipboardText(s);
#else
            DI.Get<IDesktopAppService>().SetClipboardText(s);
#endif
            Toast.Show(AppResources.LocalAuth_CopyAuthTip + auth.Name);
        }

        public async void DeleteAuth(MyAuthenticator auth)
        {
            var (success, _) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success) return;
            var r = await MessageBoxCompat.ShowAsync(@AppResources.LocalAuth_DeleteAuthTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
            if (r == MessageBoxResultCompat.OK)
            {
                AuthService.DeleteSaveAuthenticators(auth);
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
            var row = await DI.Get<IGameAccountPlatformAuthenticatorRepository>().MoveOrderByIdAsync(AuthService.Current.Authenticators.Items.Select(x => x.AuthenticatorData).ToList(), auth.Id, upOrDown);
        }
    }
}