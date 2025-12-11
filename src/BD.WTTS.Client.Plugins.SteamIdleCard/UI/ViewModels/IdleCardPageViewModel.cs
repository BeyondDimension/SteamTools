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

    public IdleCardPageViewModel()
    {
        SteamIdleSettings.IdleRule.Subscribe(async _ => { await IdleRuleChange(); }, false);
        SteamIdleSettings.IdleSequentital.Subscribe(async _ => { await IdleSequentitalChance(); }, false);

        this.WhenAnyValue(x => x.TotalCardsRemaining, y => y.DroppedCardsCount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(DropCardsCount));
            });

        this.PriorityRunIdle = ReactiveCommand.CreateFromTask<IdleApp>(PriorityRunIdleGame);
        this.IdleRunStartOrStop = ReactiveCommand.Create(IdleRunStartOrStop_Click);
        this.IdleManualRunNext = ReactiveCommand.CreateFromTask(ManualRunNext);
        this.LoginSteamCommand = ReactiveCommand.Create(async () =>
        {
            if (!IsLogin)
            {
                if (!IsLoaing)
                {
                    IsLoaing = true;
                    if (await LoginSteam())
                    {
                        await LoadBadges();
                        SteamAppsSort();
                    }
                    else
                    {
                        Toast.Show(ToastIcon.Warning, Strings.Idle_NeedLoginSteam);
                    }
                    IsLoaing = false;
                }
            }
            else //注销登录
            {

                await ISecureStorage.Instance.RemoveAsync(ISteamSessionService.CurrentSteamUserKey);
                steamSession.RemoveSession(SteamLoginState.SteamId.ToString());
                SteamLoginState = new();
                IsLogin = false;
            }
        });

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
            LoginSteamCommand.Execute(null);
        base.Activation();
    }

    /// <summary>
    /// 启动或停止挂卡
    /// </summary>
    public async Task IdleRunStartOrStop_Click()
    {
        if (!SteamTool.IsRunningSteamProcess)
        {
            Toast.Show(ToastIcon.Warning, Strings.SteamNotRuning);
            //await MessageBox.ShowAsync(Strings.SteamNotRuning, button: MessageBox.Button.OK);
            return;
        }

        if (!SteamConnectService.Current.IsConnectToSteam) // 是否登录 Steam 客户端
        {
            Toast.Show(ToastIcon.Warning, Strings.Idle_NeedLoginSteam);
            //await MessageBox.ShowAsync(Strings.Idle_NeedLoginSteam, button: MessageBox.Button.OK);
            return;
        }

        if (!IsLogin)
        {
            IsLoaing = true;
            if (!await LoginSteam()) // 登录 Steam Web
            {
                IsLoaing = false;
                Toast.Show(ToastIcon.Warning, Strings.Idle_NeedLoginSteam);
                return;
            }
            else
                IsLoaing = false;
        }

        if (!RunLoaingState)
        {
            RunLoaingState = true;

            if (!RunState)
            {
                TracepointHelper.TrackEvent("IdleCardRun");

                if (SteamLoginState.Success && SteamLoginState.SteamId != (ulong?)SteamConnectService.Current.CurrentSteamUser?.SteamId64)
                {
                    Toast.Show(ToastIcon.Warning, Strings.SteamIdle_LoginSteamUserError);
                    //RunState = false;
                    //RunLoaingState = false;
                    //return;
                }

                if (RunState = await ReadyToGoIdle(true))
                {
                    RunOrStopAutoNext(true);
                    RunOrStopAutoCardDropCheck(true);
                }
                else
                    Toast.Show(ToastIcon.Error, Strings.Idle_StartError);
            }
            else
            {
                StopIdleRun();
            }

            RunLoaingState = false;
            //Toast.Show(ToastIcon.Success, Strings.Idle_OperationSuccess);
        }
        else
        {
            Toast.Show(ToastIcon.Warning, Strings.Idle_LoaingTips);
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
        using (await asyncLock.LockAsync())
        {
            StopIdle();
            PauseAutoNext(true);
            StartSoloIdle(idleApp);
            CurrentIdleIndex = IdleGameList.IndexOf(idleApp);
            ChangeRunTxt();
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

    #endregion

    #region Private Method

    private async Task<bool> LoginSteam()
    {
        var seesion = await Ioc.Get<ISteamSessionService>().LoadSession();

        if (seesion != null && ulong.TryParse(seesion.SteamId, out var steamid))
        {
            SteamLoginState.SteamId = steamid;
            SteamLoginState.AccessToken = seesion.AccessToken;
            SteamLoginState.RefreshToken = seesion.RefreshToken;
            SteamLoginState.Cookies = seesion.CookieContainer.GetAllCookies();

            //var success = await Ioc.Get<ISteamAccountService>().CheckAccessTokenValidation(SteamLoginState.AccessToken);
            IsLogin = SteamLoginState.Success = true;
            //SteamLoginState.SteamId = success ? steamid : 0;
        }

        if (!SteamLoginState.Success)
        {
            LoginViewModel = new IdleSteamLoginPageViewModel(ref SteamLoginState);
            LoginViewModel.Close += async _ =>
            {
                IsLogin = SteamLoginState.Success;
                ViewState = 1;
                await LoadBadges();
                SteamAppsSort();
            };
            ViewState = 0;
            //var vm = new IdleSteamLoginPageViewModel(ref SteamLoginState);
            //await IWindowManager.Instance.ShowTaskDialogAsync(vm, Strings.Steam_Login,
            //    pageContent: new IdleSteamLoginPage(), okButtonText: Strings.Confirm, isOkButton: false);
            //IsLogin = SteamLoginState.Success;
        }

        var sid = SteamConnectService.Current.CurrentSteamUser?.SteamId64;
        if (SteamLoginState.Success && sid.HasValue && SteamLoginState.SteamId != (ulong)sid.Value)
        {
            Toast.Show(ToastIcon.Error, Strings.SteamIdle_LoginSteamUserError);
        }

        return IsLogin;
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
            var isTokenAccess = Ioc.Get<ISteamAccountService>().IsAccessTokenValid(SteamLoginState.AccessToken!);
            if (isTokenAccess)
            {
                await RunLoadBadges();
            }
            else
            {
                var new_accessToken = await Ioc.Get<ISteamAccountService>().RefreshAccessToken(SteamLoginState.SteamId, SteamLoginState.RefreshToken!);
                if (new_accessToken is not null) // 刷新Token
                {
                    await RefreshAccessTokenAsync(new_accessToken);
                    await RunLoadBadges();
                }
                else // 重新登录
                {
                    await ISecureStorage.Instance.RemoveAsync(ISteamSessionService.CurrentSteamUserKey);
                    SteamLoginState = new();
                    await LoginSteam();
                    await RunLoadBadges();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            return false;
        }
        async Task RunLoadBadges()
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
                throw new ArgumentNullException(Strings.Idle_PIN_NotBeNull);
            }
            else if (status != HttpStatusCode.OK)
                throw new HttpRequestException($"{Strings.Idle_GetBadgesError} status code: {status}");
            goto HandleBadges;

        HandleBadges:
            Badges.Clear();
            Badges.Add(badges!);
            TotalCardsRemaining = 0;
            TotalCardsAvgPrice = 0;

            badges = badges!.Where(w => w.CardsRemaining != 0);

            foreach (var badge in badges)
            {
                TotalCardsAvgPrice += badge.RegularAvgPrice * badge.CardsRemaining;
                TotalCardsRemaining += badge.CardsRemaining;
            }
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
            }).ToImmutableList();
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

        if (!IdleGameList.Any())
        {
            IdleComplete();
            return;
        }

        if (SteamIdleSettings.IdleRule.Value == IdleRule.FastMode)
        {
            var multi = IdleGameList.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value).ToList();

            var isLastSingle = multi.Count == 1 && IdleGameList.Count == 1; // 是否只剩最后一个游戏
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
                idleApp = VerifyIsNext(IdleGameList, isNext);
                StartSoloIdle(idleApp);
            }
            else
            {
                if (SteamIdleSettings.IdleRule.Value == IdleRule.OneThenMany)
                {
                    var multi_Idles = IdleGameList.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value).ToList();
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
                    var multi_AFKs = IdleGameList.Where(z => z.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value).ToList();
                    if (multi_AFKs.Count >= 2)
                    {
                        StartMultipleIdle();
                    }
                    else
                    {
                        idleApp = VerifyIsNext(multi_AFKs.Count <= 0 ? IdleGameList : multi_AFKs, isNext);
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
        foreach (var item in IdleGameList)
        {
            if (item.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value)
                StopSoloIdle(item.App);

            if (item.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value && IdleGameList.Count(x => x.App.Process != null) < SteamIdleSettings.MaxIdleCount)
                StartSoloIdle(item);
        }
        ResetCurrentIdle();

        if (!IdleGameList.Any(x => x.App.Process != null))
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
                item.App.Process = null;
            }
            else
            {
                item.App.Process = null;
                SteamConnectService.Current.RuningSteamApps.TryAdd(item.AppId, item.App);
            }
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
                if (IdleGameList.Sum(s => s.Badge.CardsRemaining) == 0) // 如果挂卡列表可挂卡数量为0，重新获取徽章数据挂卡
                {
                    if (IsReloaded == false)
                    {
                        IsReloaded = true;
                        await ReadyToGoIdle();
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
                    currentIdleAppId = CurrentIdle.AppId;
                    nextIdleAppId = (IdleGameList.Count - 1) >= (++CurrentIdleIndex) ? IdleGameList[CurrentIdleIndex].AppId : null;
                }

                StopIdle();
                if (await LoadBadges())
                    SteamAppsSort();
                ResetCurrentIdle();

                IdleApp? idleApp = null;
                if (currentIdleAppId.HasValue)
                {

                    if (!Badges.Any(x => x.AppId == currentIdleAppId && x.CardsRemaining > 0)) // 当前游戏挂卡完成
                    {
                        if (nextIdleAppId.HasValue) // 挂卡下一个游戏
                            idleApp = IdleGameList.Where(x => x.AppId == nextIdleAppId).FirstOrDefault();
                    }
                    else // 继续挂卡当前游戏
                        idleApp = IdleGameList.Where(x => x.AppId == currentIdleAppId).First();
                }

                var isMultipleIdle = IdleGameList.Count(x => x.App.Process != null) > 1;
                if (isMultipleIdle || idleApp == null) // 重头开始挂卡
                    StartIdle();
                else
                {
                    CurrentIdleIndex = IdleGameList.IndexOf(idleApp);
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

        if (LoginViewModel is null || LoginViewModel.RemenberLogin)
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