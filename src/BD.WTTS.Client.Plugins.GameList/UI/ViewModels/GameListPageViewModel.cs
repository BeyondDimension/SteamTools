using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public partial class GameListPageViewModel : TabItemViewModel
{
    readonly Dictionary<string, string[]> dictPinYinArray = new();

    public ReactiveCommand<Unit, Unit>? HideAppCommand { get; }

    public ReactiveCommand<Unit, Unit>? IdleAppCommand { get; }

    public ReactiveCommand<Unit, Unit>? SteamShutdownCommand { get; }

    public ReactiveCommand<Unit, Unit>? SaveEditedAppInfoCommand { get; }

    public GameListPageViewModel()
    {
        if (!IApplication.IsDesktop())
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

        IsInstalledFilter = GameLibrarySettings.GameInstalledFilter.Value;

        //IsCloudArchiveFilter = GameLibrarySettings.GameInstalledFilter.Value;

        this.WhenValueChanged(x => x.IsInstalledFilter, false)
            .Subscribe(s => GameLibrarySettings.GameInstalledFilter.Value = s);

        var nameFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateName);

        var installFilter = this.WhenAnyValue(x => x.IsInstalledFilter).Select(PredicateInstalled);

        var isCloudArchiveFilter = this.WhenAnyValue(x => x.IsCloudArchiveFilter).Select(PredicateCloudArchive);

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

        SteamConnectService.Current.SteamApps
            .Connect()
            .Filter(nameFilter)
            .Filter(typeFilter)
            .Filter(installFilter)
            .Filter(isCloudArchiveFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.DisplayName).ThenByDescending(s => s.SizeOnDisk))
            .Bind(out _SteamApps)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsSteamAppsEmpty));
                CalcTypeCount();
            });

        SteamConnectService.Current.WhenAnyValue(x => x.IsLoadingGameList)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamAppsEmpty)));

        HideAppCommand = ReactiveCommand.Create(() =>
        {
            //IWindowManager.Instance.Show(CustomWindow.HideApp, resizeMode: ResizeMode.CanResize);
        });
        IdleAppCommand = ReactiveCommand.Create(() =>
        {
            //IWindowManager.Instance.Show(CustomWindow.IdleApp, resizeMode: ResizeMode.CanResize);
        });
        SteamShutdownCommand = ReactiveCommand.Create(() =>
        {
            //IWindowManager.Instance.Show(CustomWindow.SteamShutdown, resizeMode: ResizeMode.CanResize);
        });
        SaveEditedAppInfoCommand = ReactiveCommand.Create(() =>
        {
            //IWindowManager.Instance.Show(CustomWindow.SaveEditedAppInfo, resizeMode: ResizeMode.CanResize);
        });
    }

    public override void Activation()
    {
        if (IsFirstActivation && !SteamConnectService.Current.SteamApps.Items.Any())
        {
            //SteamConnectService.Current.Initialize();
            Task2.InBackground(SteamConnectService.Current.RefreshGamesListAsync);

            //UISettings.GameListGridSize.Subscribe(x =>
            //{
            //    SteamConnectService.Current.SteamApps.Refresh();
            //}).AddTo(this);
        }
        base.Activation();
    }

    public override void Deactivation()
    {
        base.Deactivation();
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

    Func<SteamApp, bool> PredicateCloudArchive(bool isCloudArchiveFilter)
    {
        return s =>
        {
            if (isCloudArchiveFilter)
                return s.IsCloudArchive;
            return true;
        };
    }

    public string? TypeFilterString
    {
        get => EnableAppTypeFiltres != null ? string.Join(',', EnableAppTypeFiltres.Select(s => s.LocalizationName)) : null;
    }

    public void CalcTypeCount()
    {
        if (SteamConnectService.Current.SteamApps.Items.Any() && AppTypeFiltres != null)
            foreach (var item in AppTypeFiltres)
            {
                item.Count = SteamConnectService.Current.SteamApps.Items.Count(s => s.Type == item.Value);
            }
    }
}
