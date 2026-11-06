using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.SteamClient.Constants;
using Avalonia.Threading;
using System.Runtime.Devices;
using BD.WTTS.Helpers;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel
{
    readonly ISteamService SteamTool = ISteamService.Instance;
    readonly ISteamIdleCardService IdleCard = ISteamIdleCardService.Instance;
    readonly ISteamSessionService steamSession = ISteamSessionService.Instance;

    private SteamLoginState SteamLoginState = new();

    private readonly AsyncLock asyncLock = new AsyncLock();

    private bool loginOperationInProgress;

    public IdleCardPageViewModel()
    {
        SteamIdleSettings.IdleRule.Subscribe(async _ => { await IdleRuleChange(); }, false);
        SteamIdleSettings.IdleSequentital.Subscribe(async _ => { await IdleSequentitalChance(); }, false);

        this.WhenAnyValue(x => x.TotalCardsRemaining, y => y.DroppedCardsCount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(DropCardsCount));
            });

        this.WhenAnyValue(x => x.IsCheckingLogin, x => x.IsLoadingBadges, x => x.ViewState)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsPageLoading));
                this.RaisePropertyChanged(nameof(CanStartIdle));
                this.RaisePropertyChanged(nameof(PageLoadingText));
            }).AddTo(this);

        this.PriorityRunIdle = ReactiveCommand.CreateFromTask<IdleApp>(PriorityRunIdleGame);
        this.ToggleBlacklistIdle = ReactiveCommand.CreateFromTask<IdleApp>(ToggleBlacklistIdleGame);
        this.IdleRunStartOrStop = ReactiveCommand.Create(IdleRunStartOrStop_Click);
        this.IdleManualRunNext = ReactiveCommand.CreateFromTask(ManualRunNext);
        this.LoginSteamCommand = ReactiveCommand.CreateFromTask(LoginOrLogoutAsync);

        NavAppToSteamViewCommand = ReactiveCommand.Create<uint>((appid) =>
        {
            var url = string.Format(SteamApiUrls.STEAM_NAVGAME_URL, appid);
            Process2.Start(url, useShellExecute: true);
        });
        OpenLinkUrlCommand = ReactiveCommand.Create<string>(async url => await Browser2.OpenAsync(url));
    }

    public override void Activation()
    {
        if (IsFirstActivation)
            _ = CheckLoginAndLoadAsync();
        base.Activation();
    }

    /// <summary>
    /// 启动或停止挂卡
    /// </summary>
    public async Task IdleRunStartOrStop_Click()
    {
        if (RunLoaingState || loginOperationInProgress || IsPageLoading || ViewState == 0)
            return;

        RunLoaingState = true;
        try
        {
            if (!SteamTool.IsRunningSteamProcess)
            {
                Toast.Show(ToastIcon.Warning, Strings.SteamNotRuning);
                return;
            }

            if (!SteamConnectService.Current.IsConnectToSteam)
            {
                Toast.Show(ToastIcon.Warning, Strings.Idle_NeedLoginSteam);
                return;
            }
            var alreadyLoggedIn = IsLogin;

            if (!IsLogin && !await CheckLoginAsync())
                return;

            if (!RunState)
            {
                TracepointHelper.TrackEvent("IdleCardRun");

                if (alreadyLoggedIn && SteamLoginState.Success && SteamLoginState.SteamId != (ulong?)SteamConnectService.Current.CurrentSteamUser?.SteamId64)
                    Toast.Show(ToastIcon.Warning, Strings.SteamIdle_LoginSteamUserError);

                IsLoadingBadges = true;
                try
                {
                    if (RunState = await ReadyToGoIdle(true))
                    {
                        RunOrStopAutoNext(true);
                        RunOrStopAutoCardDropCheck(true);
                    }
                }
                finally
                {
                    IsLoadingBadges = false;
                }
            }
            else
            {
                StopIdleRun();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }
        finally
        {
            RunLoaingState = false;
        }
    }

    private void StopIdleRun()
    {
        RunState = false;
        StopIdle();
        RunOrStopAutoNext(false);
        RunOrStopAutoCardDropCheck(false);
        IdleTime = default;
        ResetCurrentIdle();
        ChangeRunTxt();
    }

    /// <summary>
    /// 手动切换下一个游戏
    /// </summary>
    public async Task ManualRunNext()
    {
        using (await asyncLock.LockAsync())
        {
            RunNextIdle();
        }
    }

    /// <summary>
    /// 优先运行当前游戏
    /// </summary>
    /// <param name="idleApp"></param>
    /// <returns></returns>
    public async Task PriorityRunIdleGame(IdleApp idleApp)
    {
        if (idleApp.IsExcludedFromIdle)
        {
            Toast.Show(ToastIcon.Warning, "该游戏已被排除，不参与挂卡。请先取消拉黑或移除私密限制。");
            return;
        }

        using (await asyncLock.LockAsync())
        {
            StopIdle();
            PauseAutoNext(true);
            StartSoloIdle(idleApp);
            CurrentIdleIndex = GetRunnableIdleGames().IndexOf(idleApp);
            ChangeRunTxt();
        }
    }

    public async Task ToggleBlacklistIdleGame(IdleApp idleApp)
    {
        using (await asyncLock.LockAsync())
        {
            var blackList = SteamIdleSettings.BlacklistAppList.Value ?? [];
            if (blackList.ContainsKey(idleApp.AppId))
                blackList.Remove(idleApp.AppId);
            else
                blackList[idleApp.AppId] = idleApp.AppName;

            SteamIdleSettings.BlacklistAppList.Value = blackList;

            var wasRun = RunState;
            if (wasRun)
            {
                StopIdle();
                PauseAutoNext(true);
                ResetCurrentIdle();
            }

            SteamAppsSort();

            if (wasRun)
            {
                StartIdle();
                ChangeRunTxt();
            }
        }
    }

    #region PrivateFields

    /// <summary>
    /// 当前运行游戏索引
    /// </summary>
    private int CurrentIdleIndex = 0;

    /// <summary>
    /// 暂停游戏自动切换
    /// </summary>
    private bool IsAutoNextPaused = false;

    /// <summary>
    /// 定时自动切换 CancelToken
    /// </summary>
    private CancellationTokenSource AutoNextCancellationTokenSource = new();

    /// <summary>
    /// 定时刷新检查徽章卡片掉落 CancelToken
    /// </summary>
    private CancellationTokenSource DropCardCancellationTokenSource = new();

    /// <summary>
    /// 重新加载
    /// </summary>
    private bool IsReloaded;

    private HashSet<uint> PrivateGameAppIds = [];

    #endregion

    #region Private Method

    private async Task LoginOrLogoutAsync()
    {
        if (loginOperationInProgress || IsPageLoading || RunLoaingState || ViewState == 0)
            return;

        if (!IsLogin)
        {
            await CheckLoginAndLoadAsync();
            return;
        }

        loginOperationInProgress = true;
        try
        {
            using (await asyncLock.LockAsync())
            {
                await ISecureStorage.Instance.RemoveAsync(ISteamSessionService.CurrentSteamUserKey);
                if (RunState)
                    StopIdleRun();
                steamSession.RemoveSession(SteamLoginState.SteamId.ToString());
                SteamLoginState = new();
                UserIdleInfo = null;
                Badges.Clear();
                IdleGameList.Clear();
                PrivateGameAppIds.Clear();
                DroppedCardsCount = TotalCardsRemaining = 0;
                TotalCardsAvgPrice = 0;
                HasPrivateGameExcluded = false;
                ExcludedPrivateGameCount = ExcludedBlacklistGameCount = 0;
                PrivateGameExcludeTip = string.Empty;
                IsReloaded = false;
                ShowLoginForm();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }
        finally
        {
            loginOperationInProgress = false;
        }
    }

    private async Task CheckLoginAndLoadAsync()
    {
        if (await CheckLoginAsync())
            await LoadBadgesForPageAsync();
    }

    private async Task<bool> CheckLoginAsync()
    {
        if (loginOperationInProgress || ViewState == 0)
            return false;

        loginOperationInProgress = true;
        IsCheckingLogin = true;
        try
        {
            var session = await steamSession.LoadSession();
            if (session != null && ulong.TryParse(session.SteamId, out var steamId))
            {
                SteamLoginState.SteamId = steamId;
                SteamLoginState.AccessToken = session.AccessToken;
                SteamLoginState.RefreshToken = session.RefreshToken;
                SteamLoginState.Cookies = session.CookieContainer.GetAllCookies();
                IsLogin = SteamLoginState.Success = true;
            }

            if (!SteamLoginState.Success)
            {
                ShowLoginForm();
                return false;
            }

            var sid = SteamConnectService.Current.CurrentSteamUser?.SteamId64;
            if (sid.HasValue && SteamLoginState.SteamId != (ulong)sid.Value)
                Toast.Show(ToastIcon.Error, Strings.SteamIdle_LoginSteamUserError);
            return true;
        }
        catch (OperationCanceledException)
        {
            ShowLoginForm();
            return false;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            ShowLoginForm();
            return false;
        }
        finally
        {
            IsCheckingLogin = false;
            loginOperationInProgress = false;
        }
    }

    private void ShowLoginForm()
    {
        if (ViewState == 0 && LoginViewModel != null)
            return;

        IsLogin = false;
        LoginViewModel?.Dispose();
        LoginViewModel = new IdleSteamLoginPageViewModel(ref SteamLoginState);
        LoginViewModel.Close += async _ =>
        {
            if (!SteamLoginState.Success || IsLoadingBadges)
                return;

            IsLogin = true;
            IsLoadingBadges = true;
            ViewState = 1;
            try
            {
                if (await LoadBadges())
                    SteamAppsSort();
            }
            finally
            {
                IsLoadingBadges = false;
            }
        };
        ViewState = 0;
    }

    private async Task LoadBadgesForPageAsync()
    {
        if (IsLoadingBadges)
            return;

        IsLoadingBadges = true;
        try
        {
            if (await LoadBadges())
                SteamAppsSort();
        }
        finally
        {
            IsLoadingBadges = false;
        }
    }

    private void ChangeRunTxt()
    {
        //var count = IdleGameList.Count(x => x.App.Process != null);
        //RuningCountTxt = Strings.Idle_RuningCount.Format(count, IdleGameList.Count);
        RuningCount = IdleGameList.Count(x => x.App.Process != null);
        RunState = RuningCount > 0;
    }

    private void ResetCurrentIdle() { CurrentIdle = null; CurrentIdleIndex = 0; }

    /// <summary>
    /// 挂卡规则变动
    /// </summary>
    /// <returns></returns>
    private async Task IdleRuleChange()
    {
        if (SteamIdleSettings.IdleRule.Value == IdleRule.FastMode)
            ShowSettingsOpenAutoNextWarning();
        if (RunState)
        {
            using (await asyncLock.LockAsync())
            {
                StopIdle();
                PauseAutoNext(true);
                ResetCurrentIdle();
                StartIdle();
                ChangeRunTxt();
            }
        }
    }

    /// <summary>
    /// 挂卡顺序变动
    /// </summary>
    /// <returns></returns>
    private async Task IdleSequentitalChance()
    {
        if (RunState)
        {
            using (await asyncLock.LockAsync())
            {
                StopIdle();
                ResetCurrentIdle();
                SteamAppsSort();
                StartIdle();
                ChangeRunTxt();
            }
        }

    }

    /// <summary>
    /// 加载徽章数据
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadBadges()
    {
        try
        {
            if (!Ioc.Get<ISteamAccountService>().IsAccessTokenValid(SteamLoginState.AccessToken!))
            {
                var newAccessToken = string.IsNullOrWhiteSpace(SteamLoginState.RefreshToken)
                    ? null
                    : await Ioc.Get<ISteamAccountService>().RefreshAccessToken(SteamLoginState.SteamId, SteamLoginState.RefreshToken);
                if (string.IsNullOrWhiteSpace(newAccessToken))
                {
                    await ISecureStorage.Instance.RemoveAsync(ISteamSessionService.CurrentSteamUserKey);
                    steamSession.RemoveSession(SteamLoginState.SteamId.ToString());
                    SteamLoginState = new();
                    await Dispatcher.UIThread.InvokeAsync(ShowLoginForm);
                    return false;
                }
                await RefreshAccessTokenAsync(newAccessToken);
            }

            return await RunLoadBadges();
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            return false;
        }
        async Task<bool> RunLoadBadges()
        {
            IEnumerable<Badge>? badges;
            HttpStatusCode status;

        GetBadges:
            var steam_id = SteamLoginState.SteamId.ToString();
            (UserIdleInfo, badges, status) = await IdleCard.GetBadgesAsync(steam_id, true);
            if (status == HttpStatusCode.Forbidden)
            {
                var result = await TextBoxWindowViewModel.ShowDialogAsync(new TextBoxWindowViewModel
                {
                    Title = Strings.Idle_NeedParentalPIN,
                    Placeholder = "PIN CODE",
                    InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
                });
                if (!string.IsNullOrEmpty(result))
                {
                    await steamSession.UnlockParental(steam_id, result);
                    goto GetBadges;
                }
                return false;
            }
            else if (status != HttpStatusCode.OK)
                throw new HttpRequestException($"{Strings.Idle_GetBadgesError} status code: {status}");
            goto HandleBadges;

        HandleBadges:
            Badges.Clear();
            Badges.Add(badges!);
            //await RefreshPrivateGameAppIds(steam_id);
            TotalCardsRemaining = 0;
            TotalCardsAvgPrice = 0;

            badges = badges!.Where(w => w.CardsRemaining != 0);

            foreach (var badge in badges)
            {
                if (IsExcludedApp(badge.AppId))
                    continue;

                TotalCardsAvgPrice += badge.RegularAvgPrice * badge.CardsRemaining;
                TotalCardsRemaining += badge.CardsRemaining;
            }
            return true;
        }
    }

    /// <summary>
    /// 排序并加载挂卡列表
    /// </summary>
    /// <returns></returns>
    private bool SteamAppsSort()
    {
        try
        {
            var badges = Badges.Where(w => w.CardsRemaining != 0);
            var apps = (SteamIdleSettings.IdleSequentital.Value switch
            {
                IdleSequentital.LeastCards => badges.OrderBy(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostcards => badges.OrderByDescending(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostvalue => badges.OrderByDescending(o => o.RegularAvgPrice).Select(s => new IdleApp(s)),
                _ => badges.Select(s => new IdleApp(s)),
            }).ToList();

            foreach (var app in apps)
            {
                app.IsPrivateGame = PrivateGameAppIds.Contains(app.AppId);
                app.IsBlacklisted = IsBlacklistedApp(app.AppId);
            }

            ExcludedPrivateGameCount = apps.Count(x => x.IsPrivateGame);
            ExcludedBlacklistGameCount = apps.Count(x => x.IsBlacklisted);
            HasPrivateGameExcluded = ExcludedPrivateGameCount > 0;
            PrivateGameExcludeTip = HasPrivateGameExcluded
                ? $"检测到 {ExcludedPrivateGameCount} 个私密游戏，这些游戏将自动排除，不参与挂卡。"
                : string.Empty;

            Dispatcher.UIThread.Invoke(() =>
            {
                IdleGameList.Clear();
                EnumerableExtensions.AddRange(IdleGameList, apps);
            });
            return true;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return false;
        }
    }

    private bool IsBlacklistedApp(uint appId)
    {
        var blackList = SteamIdleSettings.BlacklistAppList.Value;
        return blackList != null && blackList.ContainsKey(appId);
    }

    private bool IsExcludedApp(uint appId)
    {
        return PrivateGameAppIds.Contains(appId) || IsBlacklistedApp(appId);
    }

    private async Task RefreshPrivateGameAppIds(string steamId)
    {
        try
        {
            var (appIds, status) = await IdleCard.GetPrivateGameAppIdsAsync(steamId);
            if (status == HttpStatusCode.OK)
            {
                PrivateGameAppIds = appIds.Count > 0 ? appIds.ToHashSet() : [];
            }
            else
            {
                PrivateGameAppIds.Clear();
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            PrivateGameAppIds.Clear();
        }
    }

    private List<IdleApp> GetRunnableIdleGames()
    {
        return IdleGameList.Where(x => !x.IsExcludedFromIdle).ToList();
    }

    private int GetRunnableCardsRemaining()
    {
        return GetRunnableIdleGames().Sum(s => s.Badge.CardsRemaining);
    }

    private async Task<bool> ReadyToGoIdle(bool isFirstStart = false)
    {
        ResetCurrentIdle();
        if (await LoadBadges() && SteamAppsSort())
        {
            if (isFirstStart)
                DroppedCardsCount = TotalCardsRemaining;
            StartIdle();
            ChangeRunTxt();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 开始挂卡
    /// </summary>
    private void StartIdle(bool isNext = false)
    {
        IdleApp idleApp;
        var runnableGames = GetRunnableIdleGames();

        if (!runnableGames.Any())
        {
            IdleComplete();
            return;
        }

        if (SteamIdleSettings.IdleRule.Value == IdleRule.FastMode)
        {
            var multi = runnableGames.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value).ToList();

            var isLastSingle = multi.Count == 1 && runnableGames.Count == 1; // 是否只剩最后一个游戏
            if (multi.Count > 1 || isLastSingle)
            {
                idleApp = VerifyIsNext(multi, isNext);
                PauseAutoNext(isLastSingle);
                StartSoloIdle(idleApp);
            }
            else
            {
                PauseAutoNext(true);
                StartMultipleIdle();
            }
        }
        else
        {

            PauseAutoNext(true);

            if (SteamIdleSettings.IdleRule.Value == IdleRule.OnlyOneGame)
            {
                idleApp = VerifyIsNext(runnableGames, isNext);
                StartSoloIdle(idleApp);
            }
            else
            {
                if (SteamIdleSettings.IdleRule.Value == IdleRule.OneThenMany)
                {
                    var multi_Idles = runnableGames.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value).ToList();
                    if (multi_Idles.Count >= 1)
                    {
                        idleApp = VerifyIsNext(multi_Idles, isNext);
                        StartSoloIdle(idleApp);
                    }
                    else
                    {
                        StartMultipleIdle();
                    }
                }
                else
                {
                    var multi_AFKs = runnableGames.Where(z => z.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value).ToList();
                    if (multi_AFKs.Count >= 2)
                    {
                        StartMultipleIdle();
                    }
                    else
                    {
                        idleApp = VerifyIsNext(multi_AFKs.Count <= 0 ? runnableGames : multi_AFKs, isNext);
                        StartSoloIdle(idleApp);
                    }
                }
            }
        }

        IdleApp VerifyIsNext(IList<IdleApp> idles, bool isNext)
        {
            if (isNext && (idles.Count - 1) >= (CurrentIdleIndex + 1))
                return idles[++CurrentIdleIndex];
            else
            {
                ResetCurrentIdle();
                return idles.First();
            }
        }
    }

    /// <summary>
    /// 运行下一个挂卡
    /// </summary>
    private void RunNextIdle()
    {
        if (RunState)
        {
            StopIdle();
            StartIdle(true);
        }
    }

    /// <summary>
    /// 单独运行游戏
    /// </summary>
    /// <param name="item"></param>
    private void StartSoloIdle(IdleApp item)
    {
        if (item.IsExcludedFromIdle)
            return;

        CurrentIdle = item;
        SteamConnectService.Current.RuningSteamApps.TryGetValue(item.AppId, out var runState);
        if (runState == null)
        {
            item.App.StartSteamAppProcess();
            SteamConnectService.Current.RuningSteamApps.TryAdd(item.AppId, item.App);
        }
        else
        {
            if (runState.Process == null || !runState.Process.HasExited)
            {
                runState.StartSteamAppProcess();
            }
            else
            {
                item.App.Process = runState.Process;
            }
        }
    }

    private void StartMultipleIdle()
    {
        var runnableGames = GetRunnableIdleGames();
        foreach (var item in runnableGames)
        {
            if (item.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value)
                StopSoloIdle(item.App);

            if (item.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value && runnableGames.Count(x => x.App.Process != null) < SteamIdleSettings.MaxIdleCount)
                StartSoloIdle(item);
        }
        ResetCurrentIdle();

        if (!runnableGames.Any(x => x.App.Process != null))
            StartIdle();

    }

    /// <summary>
    /// 停止所有挂卡游戏
    /// </summary>
    private void StopIdle()
    {
        foreach (var item in IdleGameList)
        {
            SteamConnectService.Current.RuningSteamApps.TryGetValue(item.AppId, out var runState);
            if (runState != null)
            {
                runState.Process?.KillEntireProcessTree();
                SteamConnectService.Current.RuningSteamApps.TryRemove(item.AppId, out var remove);
            }
            item.App.Process = null;
        }
    }

    private void StopSoloIdle(SteamApp item)
    {
        SteamConnectService.Current.RuningSteamApps.TryGetValue(item.AppId, out var runState);
        if (runState != null)
        {
            runState.Process?.KillEntireProcessTree();
            SteamConnectService.Current.RuningSteamApps.TryRemove(item.AppId, out var remove);
            item.Process = null;
        }
        else
        {
            item.Process = null;
            SteamConnectService.Current.RuningSteamApps.TryAdd(item.AppId, item);
        }
    }

    #region AutoNext

    /// <summary>
    /// 暂停自动切换下一个游戏
    /// </summary>
    /// <param name="b"></param>
    private void PauseAutoNext(bool b)
    {
        if (b && !IsAutoNextPaused)
        {
            IsAutoNextPaused = true;
        }
        else if (!b && IsAutoNextPaused)
        {
            IsAutoNextPaused = false;
        }
    }

    /// <summary>
    /// 自动切换游戏开启或停止
    /// </summary>
    /// <param name="b"></param>
    private void RunOrStopAutoNext(bool b)
    {
        if (b)
        {
            if (RunState)
            {
                if (AutoNextCancellationTokenSource.Token.IsCancellationRequested)
                    AutoNextCancellationTokenSource = new();

                Task2.InBackground(() =>
                {
                    while (!AutoNextCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            Task.Delay(TimeSpan.FromMilliseconds(100), AutoNextCancellationTokenSource.Token).Wait();
                            IdleTime += TimeSpan.FromMilliseconds(100);
                            AutoNextTask().Wait();
                        }
                        catch (AggregateException ae)
                        {
                            if (!ae.InnerExceptions.Any(x => x is TaskCanceledException || x is InvalidOperationException))
                                ae.LogAndShowT();
                        }
                    }
                }, true);
            }
            else
                Toast.Show(ToastIcon.Info, Strings.Idle_PleaseStartIdle);
        }
        else
        {
            if (IsAutoNextPaused)
                IsAutoNextPaused = false;

            AutoNextCancellationTokenSource.Cancel();
        }
    }

    /// <summary>
    /// 定时自动切换任务
    /// </summary>
    private async Task AutoNextTask()
    {
        if (IdleTime.TotalMilliseconds != 0 && IdleTime.TotalMilliseconds % SteamIdleSettings.SwitchTime.Value == 0)
        {
            using (await asyncLock.LockAsync())
            {
                if (IsAutoNextPaused)
                {
                    return;
                }
                if (GetRunnableCardsRemaining() == 0) // 如果挂卡列表可挂卡数量为0，重新获取徽章数据挂卡
                {
                    if (IsReloaded == false)
                    {
                        IsReloaded = true;
                        if (!await ReadyToGoIdle() && !IsLogin)
                            StopIdleRun();
                    }
                    else
                    {
                        AutoNextCancellationTokenSource.Cancel();
                        IdleComplete();
                    }
                    return;
                }
                else // 开始下一个挂卡
                {
                    IsReloaded = false;
                    RunNextIdle();
                    ChangeRunTxt();
                }
            }
        }
    }
    #endregion

    #region AutoCardDropCheck
    private void RunOrStopAutoCardDropCheck(bool b)
    {
        if (b)
        {
            if (RunState)
            {
                if (DropCardCancellationTokenSource.Token.IsCancellationRequested)
                    DropCardCancellationTokenSource = new();

                Task2.InBackground(() =>
                {
                    while (!DropCardCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            Task.Delay(TimeSpan.FromMinutes(SteamIdleSettings.RefreshBadgesTime.Value), DropCardCancellationTokenSource.Token).Wait();
                            AutoCardDropCheck().Wait();
                        }
                        catch (AggregateException ae)
                        {
                            if (!ae.InnerExceptions.Any(x => x is TaskCanceledException || x is InvalidOperationException))
                                ae.LogAndShowT();
                        }
                    }
                }, true);
            }
            else
                Toast.Show(ToastIcon.Info, Strings.Idle_PleaseStartIdle);
        }
        else
        {
            DropCardCancellationTokenSource.Cancel();
        }
    }

    private async Task AutoCardDropCheck()
    {
        using (await asyncLock.LockAsync())
        {
            if (RunState)
            {
                uint? currentIdleAppId = null;
                uint? nextIdleAppId = null;
                if (CurrentIdle != null)
                {
                    var runnableBeforeRefresh = GetRunnableIdleGames();
                    currentIdleAppId = CurrentIdle.AppId;
                    var currentIdleIndex = runnableBeforeRefresh.FindIndex(x => x.AppId == currentIdleAppId.Value);
                    nextIdleAppId = (currentIdleIndex >= 0 && (runnableBeforeRefresh.Count - 1) >= (currentIdleIndex + 1))
                        ? runnableBeforeRefresh[currentIdleIndex + 1].AppId
                        : null;
                }

                StopIdle();
                if (await LoadBadges())
                    SteamAppsSort();
                else if (!IsLogin)
                {
                    StopIdleRun();
                    return;
                }
                ResetCurrentIdle();

                IdleApp? idleApp = null;
                var runnableGames = GetRunnableIdleGames();
                if (currentIdleAppId.HasValue)
                {

                    if (!Badges.Any(x => x.AppId == currentIdleAppId && x.CardsRemaining > 0)) // 当前游戏挂卡完成
                    {
                        if (nextIdleAppId.HasValue) // 挂卡下一个游戏
                            idleApp = runnableGames.Where(x => x.AppId == nextIdleAppId).FirstOrDefault();
                    }
                    else // 继续挂卡当前游戏
                        idleApp = runnableGames.Where(x => x.AppId == currentIdleAppId).FirstOrDefault();
                }

                var isMultipleIdle = runnableGames.Count(x => x.App.Process != null) > 1;
                if (isMultipleIdle || idleApp == null) // 重头开始挂卡
                    StartIdle();
                else
                {
                    CurrentIdleIndex = runnableGames.IndexOf(idleApp);
                    StartSoloIdle(idleApp);
                }
                ChangeRunTxt();
            }
        }
    }
    #endregion

    /// <summary>
    /// 挂卡完毕，没有需要挂卡的游戏
    /// </summary>
    private void IdleComplete()
    {
        RunState = false;
        var message = Strings.Idle_Complete.Format(DropCardsCount, IdleTime.TotalHours.ToInt32());
        Toast.Show(ToastIcon.Success, message);
        INotificationService.Instance.Notify(message, NotificationType.Message);
    }

    private async Task RefreshAccessTokenAsync(string new_accessToken)
    {
        SteamLoginState.AccessToken = new_accessToken;
        SteamSession session = new SteamSession()
        {
            SteamId = SteamLoginState.SteamId.ToString(),
            AccessToken = SteamLoginState.AccessToken,
            RefreshToken = SteamLoginState.RefreshToken
        };
        session.GenerateSetCookie();
        var sessionService = Ioc.Get<ISteamSessionService>();
        sessionService.AddOrSetSeesion(session);

        if (LoginViewModel is null || LoginViewModel.Login.RememberLogin)
            await sessionService.SaveSession(session);
    }
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShowSettingsOpenAutoNextWarning()
    {
        if (Ioc.Get_Nullable<IToastIntercept>() is StartupToastIntercept intercept
            && !intercept.IsStartuped)
        {
            return;
        }
        Toast.Show(ToastIcon.Info, Strings.SteamIdle_OpenAutoNextWarning);
    }
}
