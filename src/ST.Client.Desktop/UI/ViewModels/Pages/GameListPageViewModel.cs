using ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Application.Services.CloudService.Constants;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Properties;
using System.Application.Models.Settings;

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }

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

                return false;
            };
        }

        Func<SteamApp, bool> PredicateType(IEnumerable<EnumModel<SteamAppType>> types)
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
            _IconKey = nameof(GameListPageViewModel).Replace("ViewModel", "Svg");
            AppTypeFiltres = new ObservableCollection<EnumModel<SteamAppType>>(EnumModel.GetEnumModels<SteamAppType>());
            AppTypeFiltres[1].Enable = true;
            AppTypeFiltres[2].Enable = true;

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
                          this.RaisePropertyChanged(nameof(TypeFilterString));
                      }));

            SteamConnectService.Current.SteamApps
                .Connect()
                .Filter(nameFilter)
                .Filter(typeFilter)
                .Filter(installFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.DisplayName))
                .Bind(out _SteamApps)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(IsSteamAppsEmpty));
                    this.CalcTypeCount();
                });


            HideAppCommand = ReactiveCommand.Create(OpenHideAppWindow);
            IdleAppCommand = ReactiveCommand.Create(OpenIdleAppWindow);

            EnableAFKAutoUpdateCommand = ReactiveCommand.Create(() =>
            {
                AFKAutoUpdate?.CheckmarkChange(SteamConnectService.Current.IsAutoAFKApps = !SteamConnectService.Current.IsAutoAFKApps);
            });

            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                  (AFKAutoUpdate=new MenuItemViewModel (nameof(AppResources.GameList_AutoAFK))
                   {Command=EnableAFKAutoUpdateCommand }),
                  new MenuItemViewModel (),
                  new MenuItemViewModel(nameof(AppResources.GameList_HideGameManger)){
                      IconKey ="EyeHideDrawing", Command = HideAppCommand },
                  new MenuItemViewModel (nameof(AppResources.GameList_IdleGamesManger)){
                      IconKey ="TopSpeedDrawing", Command = IdleAppCommand },
            };

            AFKAutoUpdate?.CheckmarkChange(SteamConnectService.Current.IsAutoAFKApps);
        }
        public ReactiveCommand<Unit, Unit> EnableAFKAutoUpdateCommand { get; }

        public MenuItemViewModel? AFKAutoUpdate { get; }
        public void OpenHideAppWindow()
        {
            IShowWindowService.Instance.Show(CustomWindow.HideApp, new HideAppWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
        }

        public void OpenIdleAppWindow()
        {
            IShowWindowService.Instance.Show(CustomWindow.IdleApp, new IdleAppWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
        }

        public ReactiveCommand<Unit, Unit> HideAppCommand { get; }
        public ReactiveCommand<Unit, Unit> IdleAppCommand { get; }

        public override void Activation()
        {
            if (IsFirstActivation)
            {
                //SteamConnectService.Current.Initialize();
                Task.Run(SteamConnectService.Current.RefreshGamesList).ForgetAndDispose();
            }
            base.Activation();
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

        private readonly ReadOnlyObservableCollection<SteamApp> _SteamApps;
        public ReadOnlyObservableCollection<SteamApp> SteamApps => _SteamApps;

        private string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        public bool IsSteamAppsEmpty => !SteamApps.Any_Nullable() && !SteamConnectService.Current.IsLoadingGameList;

        private ObservableCollection<EnumModel<SteamAppType>> _AppTypeFiltres = new();
        public ObservableCollection<EnumModel<SteamAppType>> AppTypeFiltres
        {
            get => _AppTypeFiltres;
            set => this.RaiseAndSetIfChanged(ref _AppTypeFiltres, value);
        }

        private IReadOnlyCollection<EnumModel<SteamAppType>> _EnableAppTypeFiltres = new List<EnumModel<SteamAppType>>();
        public IReadOnlyCollection<EnumModel<SteamAppType>> EnableAppTypeFiltres
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
            if (SteamConnectService.Current.SteamApps.Items.Any())
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

        public void InstallOrStartApp(SteamApp app)
        {
            string url;
            if (app.IsInstalled)
                url = string.Format(SteamApiUrls.STEAM_RUNGAME_URL, app.AppId);
            else
                url = string.Format(SteamApiUrls.STEAM_INSTALL_URL, app.AppId);
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        public void OpenFolder(SteamApp app)
        {
            if (!string.IsNullOrEmpty(app.InstalledDir))
                IDesktopPlatformService.Instance.OpenFolder(app.InstalledDir);
        }

        public void OpenAppStoreUrl(SteamApp app)
        {
            BrowserOpen(string.Format(SteamApiUrls.STEAMSTORE_APP_URL, app.AppId));
        }

        public void OpenSteamDBUrl(SteamApp app)
        {
            BrowserOpen(string.Format(SteamApiUrls.STEAMDBINFO_APP_URL, app.AppId));
        }

        public void OpenSteamCardUrl(SteamApp app)
        {
            BrowserOpen(string.Format(SteamApiUrls.STEAMCARDEXCHANGE_APP_URL, app.AppId));
        }

        public void AddAFKAppList(SteamApp app)
        {
            try
            {
                if (GameLibrarySettings.AFKAppList.Value?.Count >= SteamConnectService.Current.SteamAFKMaxCount)
                {
                    var result = MessageBoxCompat.ShowAsync(AppResources.GameList_AddAFKAppsMaxCountTips.Format(SteamConnectService.Current.SteamAFKMaxCount), ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OK);
                }
                else
                {
                    if (GameLibrarySettings.AFKAppList.Value?.Count == SteamConnectService.Current.SteamAFKMaxCount - 2)
                    {
                        var result = MessageBoxCompat.ShowAsync(AppResources.GameList_AddAFKAppsWarningCountTips.Format(SteamConnectService.Current.SteamAFKMaxCount, SteamConnectService.Current.SteamAFKMaxCount), ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                        {
                            if (s.Result == MessageBoxResultCompat.OK)
                            {
                                AddAFKAppListFunc(app);
                            }
                        });
                    }
                    else
                    {

                        AddAFKAppListFunc(app);
                    }
                }
            }
            catch (Exception e)
            {
                Toast.Show(e.ToString());
            }
        }
        public void AddAFKAppListFunc(SteamApp app)
        {
            try
            {
                if (GameLibrarySettings.AFKAppList.Value != null && !GameLibrarySettings.AFKAppList.Value.ContainsKey(app.AppId))
                {
                    GameLibrarySettings.AFKAppList.Value!.Add(app.AppId, app.DisplayName);
                    GameLibrarySettings.AFKAppList.RaiseValueChanged();
                }
                Toast.Show(AppResources.GameList_AddAFKAppsSuccess);
            }
            catch (Exception e)
            {
                Toast.Show(e.ToString());
            }
        }
        public void AddHideAppList(SteamApp app)
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
                Toast.Show(e.ToString());
            }
        }
        public void UnlockAchievement_Click(SteamApp app)
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
                    var result = MessageBoxCompat.ShowAsync(AppResources.Achievement_RiskWarning, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                    {
                        if (s.Result == MessageBoxResultCompat.OK)
                        {
                            Toast.Show(AppResources.GameList_RuningWait);
                            app.Process = Process.Start(AppHelper.ProgramPath, "-clt app -id " + app.AppId.ToString(CultureInfo.InvariantCulture));
                            SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
                        }
                    });

                    break;
                default:
                    Toast.Show(AppResources.GameList_Unsupport);
                    break;
            }
        }

        public void AppGridReSize()
        {
            //var back = SteamConnectService.Current.SteamApps.Items;
            //SteamConnectService.Current.SteamApps.Clear();
            UISettings.AppGridSize.Value = UISettings.AppGridSize.Value == 200 ? 150 : 200;
            SteamConnectService.Current.SteamApps.Refresh();
            //SteamConnectService.Current.RefreshGamesList();
        }
    }
}