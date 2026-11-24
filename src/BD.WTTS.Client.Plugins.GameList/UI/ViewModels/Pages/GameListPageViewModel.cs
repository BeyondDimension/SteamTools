using BD.SteamClient.Constants;
using BD.WTTS.UI.Views.Pages;
using System.Runtime.CompilerServices;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameListPageViewModel : TabItemViewModel
{
    // Search metadata follows the lifetime of the model's name, including rename/refresh.
    readonly ConditionalWeakTable<string, string[]> dictPinYinArray = new();
    // Above the 128-game read batch; larger filter changes can still reset efficiently.
    const int ListResetThreshold = 256;

    public GameListPageViewModel()
    {
        if (!IApplication.IsDesktop())
        {
            throw new NotSupportedException();
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
        IsCloudArchiveFilter = GameLibrarySettings.GameCloudArchiveFilter.Value;

        this.ObserveValue(nameof(IsInstalledFilter), static x => x.IsInstalledFilter).Skip(1)
            .Subscribe(s => GameLibrarySettings.GameInstalledFilter.Value = s)
            .AddTo(this);

        var searchText = this.ObserveValue(nameof(SearchText), static x => x.SearchText);
        var nameFilter = searchText.Take(1)
            .Concat(searchText.Skip(1).Throttle(TimeSpan.FromMilliseconds(250), RxSchedulers.TaskpoolScheduler))
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Select(PredicateName);

        var installFilter = this.ObserveValue(nameof(IsInstalledFilter), static x => x.IsInstalledFilter)
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Select(PredicateInstalled);

        var isCloudArchiveFilter = this.ObserveValue(nameof(IsCloudArchiveFilter), static x => x.IsCloudArchiveFilter)
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Select(PredicateCloudArchive);

        var typeFilter = this.ObserveValue(nameof(EnableAppTypeFiltres), static x => x.EnableAppTypeFiltres)
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Select(PredicateType);

        var filter = Observable.CombineLatest(nameFilter, typeFilter, installFilter, isCloudArchiveFilter,
            (name, type, installed, cloud) => new Func<SteamApp, bool>(app =>
                type(app) && installed(app) && cloud(app) && name(app)));

        var appTypeChanges = this.ObserveValue(nameof(AppTypeFiltres), static x => x.AppTypeFiltres)
            .Select(type => type == null
                ? Observable.Empty<IChangeSet<EnumModel<SteamAppType>>>()
                : type.ToObservableChangeSet().AutoRefreshOnObservable(static item =>
                    item.ObserveValue(nameof(item.Enable), static x => x.Enable).Skip(1)));
        Observable.Switch(appTypeChanges)
            .Subscribe(_ =>
            {
                EnableAppTypeFiltres = AppTypeFiltres?.Where(s => s.Enable).ToArray();
                GameLibrarySettings.GameTypeFiltres.Value = EnableAppTypeFiltres?.Select(s => s.Value).ToList();
                this.RaisePropertyChanged(nameof(TypeFilterString));
            })
            .AddTo(this);

        SteamConnectService.Current.SteamApps
            .Connect()
            .Throttle(TimeSpan.FromMilliseconds(250), RxSchedulers.TaskpoolScheduler)
            .Subscribe(_ => CalcTypeCount())
            .AddTo(this);

        SteamConnectService.Current.SteamApps
            .Connect()
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Filter(filter)
            // Keep realized covers when a normal inventory batch arrives.
            .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.DisplayName).ThenByDescending(s => s.SizeOnDisk), resetThreshold: ListResetThreshold)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out _SteamApps, resetThreshold: ListResetThreshold)
            .Subscribe()
            .AddTo(this);

        ShowHideAppCommand = ReactiveCommand.Create(() =>
        {
            IWindowManager.Instance.ShowTaskDialogAsync(new HideAppsPageViewModel(), Strings.GameList_HideGameManger,
                pageContent: new HideAppsPage(), isOkButton: false);
        });
        RefreshAppCommand = ReactiveCommand.CreateFromTask(SteamConnectService.Current.RefreshGamesListAsync);

        AddHideAppListCommand = ReactiveCommand.Create<SteamApp>(AddHideAppList);
        AddAFKAppListCommand = ReactiveCommand.Create<SteamApp>(AddAFKAppList);
        InstallOrStartAppCommand = ReactiveCommand.Create<SteamApp>(InstallOrStartApp);
        EditAppInfoClickCommand = ReactiveCommand.Create<SteamApp>(EditAppInfoClick);
        ManageCloudArchive_ClickCommand = ReactiveCommand.Create<SteamApp>(ManageCloudArchive_Click);
        UnlockAchievement_ClickCommand = ReactiveCommand.Create<SteamApp>(UnlockAchievement_Click);
        NavAppToSteamViewCommand = ReactiveCommand.Create<SteamApp>(NavAppToSteamView);
        NavAppScreenshotToSteamViewCommand = ReactiveCommand.Create<SteamApp>(NavAppScreenshotToSteamView);
        OpenFolderCommand = ReactiveCommand.Create<SteamApp>(OpenFolder);
        OpenLinkUrlCommand = ReactiveCommand.Create<string>(async url => await Browser2.OpenAsync(url));
    }

    public override void Activation()
    {
        if (IsFirstActivation && !SteamConnectService.Current.SteamApps.Items.Any())
        {
            Task2.InBackground(SteamConnectService.Current.RefreshGamesListAsync);
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
            if (s == null)
                return false;
            if (string.IsNullOrEmpty(text))
                return true;
            var name = s.DisplayName;
            if (name.Contains(text, StringComparison.OrdinalIgnoreCase))
                return true;

            // App IDs have at most ten decimal digits; avoid a temporary string for
            // every game on every search/filter evaluation.
            Span<char> appId = stackalloc char[10];
            if (s.AppId.TryFormat(appId, out var length) &&
                ((ReadOnlySpan<char>)appId[..length]).Contains(text.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;

            var pinyinArray = dictPinYinArray.GetValue(name, static value => Pinyin.GetPinyinArray(value));
            return Pinyin.SearchCompare(text, name, pinyinArray);
        };
    }

    Func<SteamApp, bool> PredicateType(IEnumerable<EnumModel<SteamAppType>>? types)
    {
        var enabledTypes = types?.Select(x => x.Value).ToHashSet();
        return s => enabledTypes?.Contains(s.Type) == true;
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
        var counts = new Dictionary<SteamAppType, int>();
        foreach (var app in SteamConnectService.Current.SteamApps.Items)
        {
            counts.TryGetValue(app.Type, out var count);
            counts[app.Type] = count + 1;
        }

        MainThread2.BeginInvokeOnMainThread(() =>
        {
            if (Disposed || AppTypeFiltres == null)
                return;

            foreach (var item in AppTypeFiltres)
                item.Count = counts.GetValueOrDefault(item.Value);
        });
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

    public static async void EditAppInfoClick(SteamApp app)
    {
        if (app == null) return;
        var vm = new EditAppInfoPageViewModel(app);
        try
        {
            if (await IWindowManager.Instance.ShowTaskDialogAsync(vm, Strings.GameList_EditAppInfo,
                pageContent: new EditAppInfoPage(), okButtonText: Strings.L10nGames_SaveLocalEdit,
                isCancelButton: true, cancelCloseAction: () => vm.IsChangingImage, disableScroll: true))
                await vm.SaveEditAppInfoAsync();
        }
        finally
        {
            vm.DisposeEditor();
        }
    }

    public static void ManageCloudArchive_Click(SteamApp app)
    {
        if (!ISteamService.Instance.IsRunningSteamProcess)
        {
            Toast.Show(ToastIcon.Warning, Strings.SteamNotRuning);
            return;
        }

        Toast.Show(ToastIcon.Info, Strings.GameList_RuningWait);
        using var process = app.StartSteamAppProcess(SteamAppRunType.CloudManager);
    }

    public static async void UnlockAchievement_Click(SteamApp app)
    {
        if (!ISteamService.Instance.IsRunningSteamProcess)
        {
            Toast.Show(ToastIcon.Warning, Strings.SteamNotRuning);
            return;
        }
        switch (app.Type)
        {
            case SteamAppType.Application:
            case SteamAppType.Game:
                var result = await MessageBox.ShowAsync(Strings.Achievement_RiskWarning, button: MessageBox.Button.OKCancel,
                    rememberChooseKey: MessageBox.DontPromptType.UnLockAchievement);
                if (result.IsOK())
                {
                    Toast.Show(ToastIcon.Info, Strings.GameList_RuningWait);
                    NavAppToSteamView(app);
                    using var process = app.StartSteamAppProcess(SteamAppRunType.UnlockAchievement);
                }
                break;
            default:
                Toast.Show(ToastIcon.Warning, Strings.GameList_Unsupport);
                break;
        }
    }

    public static async void AddAFKAppList(SteamApp app)
    {
        try
        {
            if (GameLibrarySettings.AFKAppList.Value?.Count >= SteamConnectService.SteamAFKMaxCount)
            {
                await MessageBox.ShowAsync(Strings.GameList_AddAFKAppsMaxCountTips.Format(SteamConnectService.SteamAFKMaxCount), button: MessageBox.Button.OK);
            }
            else
            {
                if (GameLibrarySettings.AFKAppList.Value?.Count == SteamConnectService.SteamAFKMaxCount - 2)
                {
                    var result = await MessageBox.ShowAsync(Strings.GameList_AddAFKAppsWarningCountTips.Format(SteamConnectService.SteamAFKMaxCount, SteamConnectService.SteamAFKMaxCount), button: MessageBox.Button.OKCancel);
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
            e.LogAndShowT(nameof(GameListPageViewModel));
        }
    }

    public static void AddAFKAppListFunc(SteamApp app)
    {
        try
        {
            if (!GameLibrarySettings.AFKAppList.ContainsKey(app.AppId))
            {
                GameLibrarySettings.AFKAppList.Add(app.AppId, app.DisplayName);
            }
            Toast.Show(ToastIcon.Success, Strings.GameList_AddAFKAppsSuccess);
        }
        catch (Exception e)
        {
            e.LogAndShowT(nameof(GameListPageViewModel));
        }
    }

    public static void AddHideAppList(SteamApp app)
    {
        try
        {
            GameLibrarySettings.HideGameList.Add(app.AppId, app.DisplayName);
            SteamConnectService.Current.SteamApps.RemoveKey(app.AppId);

            Toast.Show(ToastIcon.Success, Strings.GameList_HideAppsSuccess);
        }
        catch (Exception e)
        {
            e.LogAndShowT(nameof(GameListPageViewModel));
        }
    }

    public static void NavAppToSteamView(SteamApp app)
    {
        var url = string.Format(SteamApiUrls.STEAM_NAVGAME_URL, app.AppId);
        Process2.Start(url, useShellExecute: true);
    }

    public static void NavAppScreenshotToSteamView(SteamApp app)
    {
        var url = string.Format(SteamApiUrls.STEAM_NAVGAMESCREENSHOTS_URL, app.AppId);
        Process2.Start(url, useShellExecute: true);
    }

    public static void OpenFolder(SteamApp app)
    {
        if (!string.IsNullOrEmpty(app.InstalledDir))
            IPlatformService.Instance.OpenFolder(app.InstalledDir);
    }
}
