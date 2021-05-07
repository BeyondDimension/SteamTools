using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Properties;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Reactive.Linq;
using DynamicData.Binding;
using DynamicData;

namespace System.Application.UI.ViewModels
{
    public class LocalAuthPageViewModel : TabItemViewModel
    {
        public LocalAuthPageViewModel()
        {
            IconKey = nameof(LocalAuthPageViewModel).Replace("ViewModel", "Svg");

            AddAuthCommand = ReactiveCommand.Create(AddAuthMenu_Click);
            RefreshAuthCommand = ReactiveCommand.Create(() => AuthService.Current.Initialize(true));
            EncryptionAuthCommand = ReactiveCommand.Create(ShowEncryptionAuthWindow);

            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                //new MenuItemViewModel(nameof(AppResources.LocalAuth_EditAuth))
                //{
                //    Items = new[]
                //    {
                        new MenuItemViewModel(nameof(AppResources.Add)) { IconKey="AddDrawing",
                            Command= AddAuthCommand },
                        new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="LockDrawing",
                            Command =EncryptionAuthCommand },
                        //new MenuItemViewModel(nameof(AppResources.Edit)) { IconKey="EditDrawing" },
                        //new MenuItemViewModel(nameof(AppResources.Export)) { IconKey="ExportDrawing",
                        //    Command= RefreshAuthCommand  },
                        new MenuItemViewModel(),
                        new MenuItemViewModel(nameof(AppResources.Refresh)) {IconKey="RefreshDrawing",
                            Command= RefreshAuthCommand },
                        //new MenuItemViewModel(),
                        //new MenuItemViewModel(nameof(AppResources.Encrypt)) {IconKey="LockDrawing" },
                        //new MenuItemViewModel(nameof(AppResources.CloudSync)) {IconKey="CloudDrawing" },
                //    }
                //},
            };


            AuthService.Current.Authenticators
                .Connect()
                //.Filter(scriptFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<MyAuthenticator>.Ascending(x => x.Index).ThenBy(x => x.Name))
                .Bind(out _Authenticators)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAuthenticatorsEmpty)));
        }

        public override string Name
        {
            get => AppResources.LocalAuth;
            protected set { throw new NotImplementedException(); }
        }

        private readonly ReadOnlyObservableCollection<MyAuthenticator> _Authenticators;
        public ReadOnlyObservableCollection<MyAuthenticator> Authenticators => _Authenticators;

        public bool IsAuthenticatorsEmpty => !AuthService.Current.Authenticators.Items.Any_Nullable();

        public ReactiveCommand<Unit, Unit> AddAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> EncryptionAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshAuthCommand { get; }

        //internal async override Task Initialize()
        //{
        //    await Task.CompletedTask;
        //}

        void AddAuthMenu_Click()
        {
            if (!AppSettings.IsOfficialChannelPackage) return;
            IShowWindowService.Instance.Show<AddAuthWindowViewModel>(CustomWindow.AddAuth);
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

        public void CopyCodeCilp(MyAuthenticator auth)
        {
            DI.Get<IDesktopAppService>().SetClipboardText(auth.CurrentCode);
            ToastService.Current.Notify(AppResources.LocalAuth_CopyAuthTip + auth.Name);
        }

        public void DeleteAuth(MyAuthenticator auth)
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.LocalAuth_DeleteAuthTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    AuthService.DeleteSaveAuthenticators(auth);
                }
            });
        }

        public void ShowSteamAuthData(MyAuthenticator auth)
        {
            if (auth.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator)
            {
                IShowWindowService.Instance.Show(CustomWindow.ShowAuth, new ShowAuthWindowViewModel(auth), string.Empty, ResizeModeCompat.CanResize);
            }
        }

        public void ShowSteamAuthTrade(MyAuthenticator auth)
        {
            if (auth.AuthenticatorData.Value is GAPAuthenticatorValueDTO.SteamAuthenticator)
            {
                IShowWindowService.Instance.Show(CustomWindow.AuthTrade, new AuthTradeWindowViewModel(auth), string.Empty, ResizeModeCompat.CanResize);
            }
        }

        public void ShowEncryptionAuthWindow()
        {
            IShowWindowService.Instance.Show(CustomWindow.EncryptionAuth, new EncryptionAuthWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
        }

    }
}
