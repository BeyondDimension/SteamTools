using BD.SteamClient.Constants;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameListPageViewModel : TabItemViewModel
{
    readonly Dictionary<string, string[]> dictPinYinArray = new();

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
            .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.DisplayName).ThenByDescending(s => s.SizeOnDisk))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _SteamApps)
            .Subscribe(_ =>
            {
                ////无已安装游戏时，显示提示
                //if (SteamConnectService.Current.SteamApps.Keys.Any() &&
                //    IsInstalledFilter && !_SteamApps.Any_Nullable())
                //{
                //    IsInstalledFilter = false;
                //    Toast.Show(ToastIcon.Info, "没有检测到已安装游戏，自动切换全部游戏");
                //}
                CalcTypeCount();
            });

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
        var vm = new EditAppInfoPageViewModel(ref app);
        var result = await IWindowManager.Instance.ShowTaskDialogAsync(vm, Strings.GameList_EditAppInfo,
            pageContent: new EditAppInfoPage(), okButtonText: Strings.Save, isCancelButton: true, disableScroll: true);

        if (result)
        {
            vm.SaveEditAppInfo();
        }
        else
        {
            vm.CancelEditAppInfo();
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
        app.StartSteamAppProcess(SteamAppRunType.CloudManager);
        SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
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
                    app.StartSteamAppProcess(SteamAppRunType.UnlockAchievement);
                    SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
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
