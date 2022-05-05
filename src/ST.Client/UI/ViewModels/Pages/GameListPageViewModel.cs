using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public partial class GameListPageViewModel
    {
        const string TAG = "GameListPageVM";

        readonly Dictionary<string, string[]> dictPinYinArray = new();

        Func<SteamApp, bool> PredicateName(string? text)
        {
            return s =>
            {
                if (s == null || s.DisplayName == null)
                    return false;
                if (string.IsNullOrEmpty(text))
                    return true;
                if (s.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                       s.AppId.ToString().Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                var pinyinArray = Pinyin.GetPinyin(s.DisplayName, dictPinYinArray);
                if (Pinyin.SearchCompare(text, s.DisplayName, pinyinArray))
                {
                    return true;
                }
                return false;
            };
        }

        Func<SteamApp, bool> PredicateType(IEnumerable<EnumModel<SteamAppType>>? types)
        {
            //var types = AppTypeFiltres.Where(x => x.Enable);
            return (s) =>
            {
                if (types == null)
                    return false;
                if (types.Any())
                {
                    if (types.Any(x => x.Value == s.Type))
                    {
                        return true;
                    }
                }
                return false;
            };
        }

        Func<SteamApp, bool> PredicateInstalled(bool isInstalledFilter)
        {
            return s =>
            {
                if (isInstalledFilter)
                    return s.IsInstalled;
                return true;
            };
        }

        public GameListPageViewModel()
        {
            _IconKey = nameof(GameListPageViewModel);

            if (!IApplication.IsDesktopPlatform)
            {
                return;
            }

            AppTypeFiltres = new ObservableCollection<EnumModel<SteamAppType>>(EnumModel.GetEnums<SteamAppType>());

            foreach (var type in AppTypeFiltres)
            {
                if (GameLibrarySettings.GameTypeFiltres.Value?.Contains(type.Value) == true)
                {
                    type.Enable = true;
                }
            }

            IsInstalledFilter = GameLibrarySettings.GameIsInstalledFilter.Value;

            this.WhenValueChanged(x => x.IsInstalledFilter, false)
                .Subscribe(s => GameLibrarySettings.GameIsInstalledFilter.Value = s);

            var nameFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateName);

            var installFilter = this.WhenAnyValue(x => x.IsInstalledFilter).Select(PredicateInstalled);

            var typeFilter = this.WhenAnyValue(x => x.EnableAppTypeFiltres).Select(PredicateType);

            this.WhenAnyValue(x => x.AppTypeFiltres)
                .Subscribe(type => type?
                      .ToObservableChangeSet()
                      .AutoRefresh(x => x.Enable)
                      .Subscribe(_ =>
                      {
                          EnableAppTypeFiltres = AppTypeFiltres.Where(s => s.Enable).ToList();
                          GameLibrarySettings.GameTypeFiltres.Value = EnableAppTypeFiltres.Select(s => s.Value).ToList();
                          this.RaisePropertyChanged(nameof(TypeFilterString));
                      }));

            SteamConnectService.Current.WhenValueChanged(x => x.IsLoadingGameList, false)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamAppsEmpty)));

            SteamConnectService.Current.SteamApps
                .Connect()
                .Filter(nameFilter)
                .Filter(typeFilter)
                .Filter(installFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.DisplayName).ThenByDescending(s => s.SizeOnDisk))
                .Bind(out _SteamApps)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(IsSteamAppsEmpty));
                    CalcTypeCount();
                });

            HideAppCommand = ReactiveCommand.Create(() =>
            {
                IWindowManager.Instance.Show(CustomWindow.HideApp, resizeMode: ResizeMode.CanResize);
            });
            IdleAppCommand = ReactiveCommand.Create(() =>
            {
                IWindowManager.Instance.Show(CustomWindow.IdleApp, resizeMode: ResizeMode.CanResize);
            });
            SteamShutdownCommand = ReactiveCommand.Create(() =>
            {
                IWindowManager.Instance.Show(CustomWindow.SteamShutdown, resizeMode: ResizeMode.CanResize);
            });
            SaveEditedAppInfoCommand = ReactiveCommand.Create(() =>
            {
                IWindowManager.Instance.Show(CustomWindow.SaveEditedAppInfo, resizeMode: ResizeMode.CanResize);
            });
        }

        //public ReactiveCommand<Unit, Unit> EnableAFKAutoUpdateCommand { get; }

        //public MenuItemViewModel? AFKAutoUpdate { get; }

        public ReactiveCommand<Unit, Unit>? HideAppCommand { get; }

        public ReactiveCommand<Unit, Unit>? IdleAppCommand { get; }

        public ReactiveCommand<Unit, Unit>? SteamShutdownCommand { get; }

        public ReactiveCommand<Unit, Unit>? SaveEditedAppInfoCommand { get; }

        public override void Activation()
        {
            if (IsFirstActivation && !SteamConnectService.Current.SteamApps.Items.Any())
            {
                //SteamConnectService.Current.Initialize();
                Task.Run(SteamConnectService.Current.RefreshGamesList).ForgetAndDispose();

                UISettings.AppGridSize.Subscribe(x =>
                {
                    SteamConnectService.Current.SteamApps.Refresh();
                }).AddTo(this);

            }
            base.Activation();
        }

        public override void Deactivation()
        {
            base.Deactivation();
        }

        private bool _IsOpenFilter;

        public bool IsOpenFilter
        {
            get => _IsOpenFilter;
            set => this.RaiseAndSetIfChanged(ref _IsOpenFilter, value);
        }

        private bool _IsInstalledFilter;

        public bool IsInstalledFilter
        {
            get => _IsInstalledFilter;
            set => this.RaiseAndSetIfChanged(ref _IsInstalledFilter, value);
        }

        private bool _IsAppInfoOpen;

        public bool IsAppInfoOpen
        {
            get => _IsAppInfoOpen;
            set => this.RaiseAndSetIfChanged(ref _IsAppInfoOpen, value);
        }

        private SteamApp? _SelectApp;

        public SteamApp? SelectApp
        {
            get => _SelectApp;
            set => this.RaiseAndSetIfChanged(ref _SelectApp, value);
        }

        private readonly ReadOnlyObservableCollection<SteamApp>? _SteamApps;

        public ReadOnlyObservableCollection<SteamApp>? SteamApps => _SteamApps;

        private string? _SearchText;

        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        public bool IsSteamAppsEmpty => !SteamApps.Any_Nullable() && !SteamConnectService.Current.IsLoadingGameList;

        private ObservableCollection<EnumModel<SteamAppType>>? _AppTypeFiltres;

        public ObservableCollection<EnumModel<SteamAppType>>? AppTypeFiltres
        {
            get => _AppTypeFiltres;
            set => this.RaiseAndSetIfChanged(ref _AppTypeFiltres, value);
        }

        private IReadOnlyCollection<EnumModel<SteamAppType>>? _EnableAppTypeFiltres;

        public IReadOnlyCollection<EnumModel<SteamAppType>>? EnableAppTypeFiltres
        {
            get => _EnableAppTypeFiltres;
            set => this.RaiseAndSetIfChanged(ref _EnableAppTypeFiltres, value);
        }

        public string TypeFilterString
        {
            get => string.Join(',', EnableAppTypeFiltres.Select(s => s.Name_Localiza));
        }

        public void CalcTypeCount()
        {
            if (SteamConnectService.Current.SteamApps.Items.Any() && AppTypeFiltres != null)
                foreach (var item in AppTypeFiltres)
                {
                    item.Count = SteamConnectService.Current.SteamApps.Items.Count(s => s.Type == item.Value);
                }
        }

        public void AppClick(SteamApp app)
        {
            IsAppInfoOpen = true;
            SelectApp = app;
        }

        public static void EditAppInfoClick(SteamApp app)
        {
            if (app == null) return;
            IWindowManager.Instance.Show(CustomWindow.EditAppInfo, new EditAppInfoWindowViewModel(app), string.Empty, ResizeMode.CanResize);
        }

        public static void InstallOrStartApp(SteamApp app)
        {
            string url;
            if (app.IsInstalled)
                url = string.Format(SteamApiUrls.STEAM_RUNGAME_URL, app.AppId);
            else
                url = string.Format(SteamApiUrls.STEAM_INSTALL_URL, app.AppId);
            Process2.Start(url, useShellExecute: true);
        }

        public static void NavAppToSteamView(SteamApp app)
        {
            var url = string.Format(SteamApiUrls.STEAM_NAVGAME_URL, app.AppId);
            Process2.Start(url, useShellExecute: true);
        }

        public static void OpenFolder(SteamApp app)
        {
            if (!string.IsNullOrEmpty(app.InstalledDir))
                IPlatformService.Instance.OpenFolder(app.InstalledDir);
        }

        public static async void OpenAppStoreUrl(SteamApp app)
        {
            await Browser2.OpenAsync(string.Format(SteamApiUrls.STEAMSTORE_APP_URL, app.AppId));
        }

        public static async void OpenSteamDBUrl(SteamApp app)
        {
            await Browser2.OpenAsync(string.Format(SteamApiUrls.STEAMDBINFO_APP_URL, app.AppId));
        }

        public static async void OpenSteamCardUrl(SteamApp app)
        {
            await Browser2.OpenAsync(string.Format(SteamApiUrls.STEAMCARDEXCHANGE_APP_URL, app.AppId));
        }

        public static async void AddAFKAppList(SteamApp app)
        {
            try
            {
                if (GameLibrarySettings.AFKAppList.Value?.Count >= SteamConnectService.SteamAFKMaxCount)
                {
                    await MessageBox.ShowAsync(AppResources.GameList_AddAFKAppsMaxCountTips.Format(SteamConnectService.SteamAFKMaxCount), button: MessageBox.Button.OK);
                }
                else
                {
                    if (GameLibrarySettings.AFKAppList.Value?.Count == SteamConnectService.SteamAFKMaxCount - 2)
                    {
                        var result = await MessageBox.ShowAsync(AppResources.GameList_AddAFKAppsWarningCountTips.Format(SteamConnectService.SteamAFKMaxCount, SteamConnectService.SteamAFKMaxCount), button: MessageBox.Button.OKCancel);
                        if (result.IsOK())
                        {
                            AddAFKAppListFunc(app);
                        }
                    }
                    else
                    {

                        AddAFKAppListFunc(app);
                    }
                }
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
        }

        public static void AddAFKAppListFunc(SteamApp app)
        {
            try
            {
                if (GameLibrarySettings.AFKAppList.Value == null)
                {
                    GameLibrarySettings.AFKAppList.Value = new Dictionary<uint, string?>();
                }
                if (!GameLibrarySettings.AFKAppList.Value.ContainsKey(app.AppId))
                {
                    GameLibrarySettings.AFKAppList.Value.Add(app.AppId, app.DisplayName);
                    GameLibrarySettings.AFKAppList.RaiseValueChanged();
                }
                Toast.Show(AppResources.GameList_AddAFKAppsSuccess);
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
        }

        public static void AddHideAppList(SteamApp app)
        {
            try
            {
                GameLibrarySettings.HideGameList.Value!.Add(app.AppId, app.DisplayName);
                GameLibrarySettings.HideGameList.RaiseValueChanged();

                SteamConnectService.Current.SteamApps.Remove(app);
                Toast.Show(AppResources.GameList_HideAppsSuccess);
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
        }

        public static async void UnlockAchievement_Click(SteamApp app)
        {
            if (!ISteamService.Instance.IsRunningSteamProcess)
            {
                Toast.Show(AppResources.GameList_SteamNotRuning);
                return;
            }
            switch (app.Type)
            {
                case SteamAppType.Application:
                case SteamAppType.Game:
                    var result = await MessageBox.ShowAsync(AppResources.Achievement_RiskWarning, button: MessageBox.Button.OKCancel,
                        rememberChooseKey: MessageBox.DontPromptType.UnLockAchievement);
                    if (result.IsOK())
                    {
                        Toast.Show(AppResources.GameList_RuningWait);
                        app.StartSteamAppProcess(true);
                        SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
                    }
                    break;
                default:
                    Toast.Show(AppResources.GameList_Unsupport);
                    break;
            }
        }

        public static void AppGridReSize()
        {
            UISettings.AppGridSize.Value = UISettings.AppGridSize.Value == 200 ? 150 : 200;
        }
    }
}
