using BD.SteamClient.Helpers;
using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using BD.SteamClient.Constants;
using AngleSharp.Common;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel : ViewModelBase
{
    readonly ISteamService SteamTool = ISteamService.Instance;
    readonly ISteamIdleCardService IdleCard = ISteamIdleCardService.Instance;

    private SteamLoginState SteamLoginState = new();

    private readonly AsyncLock asyncLock = new AsyncLock();

    public IdleCardPageViewModel()
    {
        SteamIdleSettings.IsAutoNextOn.Subscribe(RunOrStopAutoNext, false);
        SteamIdleSettings.IdleRule.Subscribe(async _ => { await IdleRuleChange(); }, false);
        SteamIdleSettings.IdleSequentital.Subscribe(async _ => { await IdleSequentitalChance(); }, false);

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

        if (OperatingSystem2.IsMacOS())
        {
            if (!ISteamService.Instance.IsRunningSteamProcess)
            {
                Toast.Show(ToastIcon.Warning, Strings.Idle_NeedLoginSteam);
                return;
            }
        }
        else if (!SteamConnectService.Current.IsConnectToSteam) // 是否登录 Steam 客户端
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
                //MAC无法获取到当前steam客户端登录用户所以忽略判断
                if (!OperatingSystem2.IsMacOS())
                {
                    if (SteamLoginState.Success && SteamLoginState.SteamId != (ulong?)SteamConnectService.Current.CurrentSteamUser?.SteamId64)
                    {
                        Toast.Show(ToastIcon.Error, Strings.SteamIdle_LoginSteamUserError);
                        RunState = false;
                        return;
                    }
                }

                //await SteamConnectService.Current.RefreshGamesListAsync();
                RunState = await ReadyToGoIdle(true);
                RunOrStopAutoNext(SteamIdleSettings.IsAutoNextOn.Value);
                RunOrStopAutoCardDropCheck(true);
            }
            else
            {
                RunState = false;
                StopIdle();
                RunOrStopAutoNext(false);
                RunOrStopAutoCardDropCheck(false);
                IdleTime = default;
                ResetCurrentIdle();
                ChangeRunTxt();
            }

            RunLoaingState = false;
            //Toast.Show(ToastIcon.Success, Strings.Idle_OperationSuccess);
        }
        else
        {
            Toast.Show(ToastIcon.Warning, Strings.Idle_LoaingTips);
        }
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
    /// 最大并行运行游戏数量
    /// </summary>
    private int MaxIdleCount = 32;

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
            var vm = new IdleSteamLoginPageViewModel(ref SteamLoginState);
            await IWindowManager.Instance.ShowTaskDialogAsync(vm, Strings.Steam_Login,
                pageContent: new IdleSteamLoginPage(), okButtonText: Strings.Confirm, isOkButton: false);

            IsLogin = SteamLoginState.Success;
        }

        //if (SteamLoginState.Success && SteamLoginState.SteamId != (ulong?)SteamConnectService.Current.CurrentSteamUser?.SteamId64)
        //{
        //    Toast.Show(ToastIcon.Error, Strings.SteamIdle_LoginSteamUserError);
        //}

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
        if (RunState)
        {
            using (await asyncLock.LockAsync())
            {
                StopIdle();
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
            var isTokenAccess = await Ioc.Get<ISteamAccountService>().CheckAccessTokenValidation(SteamLoginState.AccessToken!);
            if (isTokenAccess)
            {
                await RunLoadBadges();
            }
            else
            {
                await ISecureStorage.Instance.RemoveAsync(ISteamSessionService.CurrentSteamUserKey);
                SteamLoginState = new();
                await LoginSteam();
                await RunLoadBadges();
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
            IEnumerable<Badge> badges;

            (UserIdleInfo, badges) = await IdleCard.GetBadgesAsync(SteamLoginState.SteamId.ToString(), true);

            Badges.Clear();
            Badges.Add(badges);
            TotalCardsRemaining = 0;
            TotalCardsAvgPrice = 0;

            badges = badges.Where(w => w.CardsRemaining != 0);

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
            var apps = SteamIdleSettings.IdleSequentital.Value switch
            {
                IdleSequentital.LeastCards => badges.OrderBy(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostcards => badges.OrderByDescending(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostvalue => badges.OrderByDescending(o => o.RegularAvgPrice).Select(s => new IdleApp(s)),
                _ => badges.Select(s => new IdleApp(s)),
            };

            IdleGameList = new ObservableCollection<IdleApp>(apps);

            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
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

        if (SteamIdleSettings.IdleRule.Value == IdleRule.OnlyOneGame)
        {
            PauseAutoNext(false);
            idleApp = VerifyIsNext(IdleGameList, isNext);
            StartSoloIdle(idleApp);
        }
        else
        {
            if (SteamIdleSettings.IdleRule.Value == IdleRule.OneThenMany)
            {
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value).ToList();

                if (multi.Count >= 1)
                {
                    idleApp = VerifyIsNext(multi, isNext);
                    PauseAutoNext(false);
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
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value).ToList();
                if (multi.Count >= 2)
                {
                    PauseAutoNext(true);
                    StartMultipleIdle();
                }
                else
                {
                    idleApp = VerifyIsNext(multi, isNext);
                    PauseAutoNext(false);
                    StartSoloIdle(idleApp);
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

            if (item.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value && IdleGameList.Count(x => x.App.Process != null) < MaxIdleCount)
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
                            Task.Delay(TimeSpan.FromMilliseconds(SteamIdleSettings.SwitchTime.Value), AutoNextCancellationTokenSource.Token).Wait();
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
        using (await asyncLock.LockAsync())
        {
            if (!SteamIdleSettings.IsAutoNextOn.Value || IsAutoNextPaused)
            {
                if (!SteamIdleSettings.IsAutoNextOn.Value)
                    AutoNextCancellationTokenSource.Cancel();
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
                    SteamIdleSettings.IsAutoNextOn.Value = false;
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
                            Task.Delay(TimeSpan.FromMilliseconds(100), DropCardCancellationTokenSource.Token).Wait();
                            IdleTime += TimeSpan.FromMilliseconds(100);
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
        if (IdleTime.TotalMinutes != 0 && IdleTime.TotalMinutes % SteamIdleSettings.RefreshBadgesTime == 0) // 每六分钟刷新徽章信息
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
                        nextIdleAppId = (IdleGameList.Count - 1) > (++CurrentIdleIndex) ? IdleGameList[CurrentIdleIndex].AppId : null;
                    }

                    StopIdle();
                    await LoadBadges();
                    SteamAppsSort();
                    ResetCurrentIdle();

                    IdleApp? idleApp = null;
                    if (currentIdleAppId.HasValue) // 存在单独运行的游戏 检查是否挂卡完成，完成则切换下一个游戏
                    {

                        if (!Badges.Any(x => x.AppId == currentIdleAppId && x.CardsRemaining > 0))
                        {
                            if (nextIdleAppId.HasValue)
                                idleApp = IdleGameList.Where(x => x.AppId == nextIdleAppId).First();
                        }
                        else
                            idleApp = IdleGameList.Where(x => x.AppId == currentIdleAppId).First();
                    }

                    var isMultipleIdle = IdleGameList.Count(x => x.App.Process != null) > 1; // 存在多个挂卡游戏 刷新挂卡列表重新批量挂卡
                    if (isMultipleIdle || idleApp == null)
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
    }
    #endregion

    /// <summary>
    /// 挂卡完毕，没有需要挂卡的游戏
    /// </summary>
    private void IdleComplete()
    {
        RunState = false;
        Toast.Show(ToastIcon.Success, Strings.Idle_Complete);
        INotificationService.Instance.Notify(Strings.Idle_Complete, NotificationType.Message);
    }
    #endregion

}