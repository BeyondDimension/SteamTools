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

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class LocalAuthPageViewModel
    {
        readonly Dictionary<string, string[]> dictPinYinArray = new();
        Func<MyAuthenticator, bool> PredicateName(string? serachText)
        {
            return s =>
            {
                if (s == null)
                    return false;
                if (string.IsNullOrEmpty(serachText))
                    return true;
                if (s.Name.Contains(serachText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                var pinyinArray = Pinyin.GetPinyin(s.Name, dictPinYinArray);
                if (Pinyin.SearchCompare(serachText, s.Name, pinyinArray))
                {
                    return true;
                }

                return false;
            };
        }

        public LocalAuthPageViewModel()
        {
            IconKey = nameof(LocalAuthPageViewModel);

            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(Browser2.OpenAsync);
            AddAuthCommand = ReactiveCommand.Create(AddAuthMenu_Click);
            RefreshAuthCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                IsRefreshing = true;
                await AuthService.Current.InitializeAsync(true);
                IsRefreshing = false;
            });
            EncryptionAuthCommand = ReactiveCommand.Create(ShowEncryptionAuthWindow);
            ExportAuthCommand = ReactiveCommand.Create(ShowExportAuthAuthWindow);
            LockCommand = ReactiveCommand.Create(async () =>
            {
                var result = await AuthService.Current.HasPasswordEncryption();
                if (result)
                {
                    AuthService.Current.Authenticators.Clear();
                    MenuItems?.Clear();
                    Activation();
                }
                else
                {
                    Toast.Show(AppResources.LocalAuth_LockError);
                }
            });

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

            var textFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateName);
            AuthService.Current.Authenticators
                .Connect()
                .Filter(textFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<MyAuthenticator>.Ascending(x => x.Index).ThenBy(x => x.Name))
                .Bind(out _Authenticators)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAuthenticatorsEmpty)));
        }

        public override bool IsTaskBarSubMenu => MenuItems.Any_Nullable();

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

        private string _SearchText = "";
        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

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

        public async override void Activation()
        {
            var auths = await AuthService.Current.Repository.GetAllSourceAsync();
            SourceAuthCount = auths.Length;
            //if (IsFirstActivation)
            if (IsAuthenticatorsEmptyButHasSourceAuths)
            {
                await AuthService.Current.InitializeAsync(auths);
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
            }
            else
            {
                IsFirstLoadedAuthenticatorsEmpty = true;
            }
            base.Activation();
        }

        void AddAuthMenu_Click()
        {
            if (!IsNotOfficialChannelPackageDetectionHelper.Check()) return;
            IWindowManager.Instance.Show(CustomWindow.AddAuth, resizeMode: ResizeMode.CanResize);
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

        public void CopyCodeCilp(MyAuthenticator auth) => auth.CopyCodeCilp();

        public void DeleteAuth(MyAuthenticator auth) => DeleteAuthCore(auth);

        public async void DeleteAuthCore(MyAuthenticator auth, Action? okAction = null)
        {
            var (success, _) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success) return;
            var r = await MessageBox.ShowAsync(AppResources.LocalAuth_DeleteAuthTip, button: MessageBox.Button.OKCancel);
            if (r == MessageBox.Result.OK)
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
                    await IWindowManager.Instance.Show(
                        CustomWindow.ShowAuth,
                        new ShowAuthWindowViewModel(auth),
                        resizeMode: ResizeMode.CanResize);
                    break;
            }
        }

        public void ShowSteamAuthTrade(MyAuthenticator auth)
        {
            switch (auth.AuthenticatorData.Platform)
            {
                case GamePlatform.Steam:
                    IWindowManager.Instance.Show(
                        CustomWindow.AuthTrade,
                        new AuthTradeWindowViewModel(auth),
                        resizeMode: ResizeMode.CanResize);
                    break;
            }
        }

        async void ShowEncryptionAuthWindow()
        {
            //if (await AuthService.Current.HasPasswordEncryptionShowPassWordWindow())
            //{
            await IWindowManager.Instance.Show(
                CustomWindow.EncryptionAuth,
                resizeMode: ResizeMode.CanResize);
            //}
        }

        async void ShowExportAuthAuthWindow()
        {
            //if (await AuthService.Current.HasPasswordEncryptionShowPassWordWindow())
            //{
            await IWindowManager.Instance.Show(
                CustomWindow.ExportAuth,
                resizeMode: ResizeMode.CanResize);
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
