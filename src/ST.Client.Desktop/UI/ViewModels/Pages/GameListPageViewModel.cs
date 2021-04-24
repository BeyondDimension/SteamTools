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

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }

        public GameListPageViewModel()
        {
            _IconKey = nameof(GameListPageViewModel).Replace("ViewModel", "Svg");
            _AppTypeFiltres = new ObservableCollection<EnumModel<SteamAppType>>(EnumModel.GetEnumModels<SteamAppType>());
            _AppTypeFiltres[1].Enable = true;
            _AppTypeFiltres[2].Enable = true;
        }

        private readonly Subject<Unit> updateSource = new();
        public bool IsReloading { get; set; }

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

        private IReadOnlyCollection<SteamApp>? _SteamApps;
        public IReadOnlyCollection<SteamApp>? SteamApps
        {
            get => _SteamApps;
            set
            {
                if (_SteamApps != value)
                {
                    _SteamApps = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsSteamAppsEmpty));
                }
            }
        }

        private string? _SerachText;
        public string? SerachText
        {
            get => _SerachText;
            set => this.RaiseAndSetIfChanged(ref _SerachText, value);
        }

        public bool IsSteamAppsEmpty => !SteamApps.Any_Nullable();

        private ObservableCollection<EnumModel<SteamAppType>> _AppTypeFiltres;
        public ObservableCollection<EnumModel<SteamAppType>> AppTypeFiltres
        {
            get => _AppTypeFiltres;
            set => this.RaiseAndSetIfChanged(ref _AppTypeFiltres, value);
        }

        public string TypeFilterString
        {
            get => string.Join(',', AppTypeFiltres.Where(x => x.Enable).Select(s => s.Name_Localiza));
        }

        internal override void Initialize()
        {
            this.updateSource
                .Do(_ => this.IsReloading = true)
                .SelectMany(x => this.UpdateAsync())
                .Do(_ => this.IsReloading = false)
                .Subscribe()
                .AddTo(this);

            SteamConnectService.Current
              .WhenAnyValue(x => x.SteamApps)
              .Subscribe(_ => Update());

            this.WhenAnyValue(x => x.SerachText)
                  .Subscribe(_ =>
                  {
                      Update();
                  });

            this.WhenAnyValue(x => x.IsInstalledFilter)
                  .Subscribe(_ =>
                  {
                      Update();
                  });

            this.WhenAnyValue(x => x.AppTypeFiltres)
                  .Subscribe(type => type?
                        .ToObservableChangeSet()
                        .AutoRefresh(x => x.Enable)
                        .WhenValueChanged(x => x.Enable, false)
                        .Subscribe(_ =>
                        {
                            this.RaisePropertyChanged(nameof(TypeFilterString));
                            Update();
                        }));
        }

        private IObservable<Unit> UpdateAsync()
        {
            var types = AppTypeFiltres.Where(x => x.Enable);
            bool predicateName(SteamApp s)
            {
                if (!string.IsNullOrEmpty(SerachText))
                {
                    if (s.DisplayName?.Contains(SerachText, StringComparison.OrdinalIgnoreCase) == true ||
                        s.AppId.ToString().Contains(SerachText, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                return false;
            }
            bool predicateType(SteamApp s)
            {
                if (types.Any())
                {
                    if (types.Any(x => x.Value == s.Type))
                    {
                        return true;
                    }
                }
                return false;
            }
            bool predicateInstalled(SteamApp s)
            {
                if (IsInstalledFilter)
                    return s.IsInstalled;
                return true;
            }

            return Observable.Start(() =>
            {
                var list = SteamConnectService.Current.SteamApps?
                .Where(x => predicateType(x))
                .Where(x => predicateName(x))
                .Where(x => predicateInstalled(x))
                .OrderBy(x => x.DisplayName).ToList();
                if (list.Any_Nullable())
                    this.SteamApps = list;
                else
                    this.SteamApps = null;
                this.CalcTypeCount();
            });
        }

        public void Update()
        {
            this.updateSource.OnNext(Unit.Default);
        }

        public void CalcTypeCount()
        {
            if (SteamConnectService.Current.SteamApps.Any_Nullable())
                foreach (var item in AppTypeFiltres)
                {
                    item.Count = SteamConnectService.Current.SteamApps.Count(s => s.Type == item.Value);
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

        public void UnlockAchievement_Click(SteamApp app)
        {
            switch (app.Type)
            {
                case SteamAppType.Application:
                case SteamAppType.Game:
                    //if (WindowService.Current.MainWindow.Dialog("【风险提示】解锁成就可能会被游戏开发者视为作弊，并且会被成就统计网站封锁。若决定继续使用，请自行承担解锁成就带来的风险和后果。"))
                    //{
                    app.Process = Process.Start(Process.GetCurrentProcess().MainModule.FileName, "-clt app -id " + app.AppId.ToString(CultureInfo.InvariantCulture));
                    SteamConnectService.Current.RuningSteamApps.Add(app);
                    //}
                    break;
                default:
                    ToastService.Current.Notify("不支持的操作");
                    break;
            }
        }
    }
}