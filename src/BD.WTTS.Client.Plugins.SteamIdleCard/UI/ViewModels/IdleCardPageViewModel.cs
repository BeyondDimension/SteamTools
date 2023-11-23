using BD.SteamClient.Helpers;
using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using BD.SteamClient.Constants;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel : ViewModelBase
{
    readonly ISteamService SteamTool = ISteamService.Instance;
    readonly ISteamIdleCardService IdleCard = ISteamIdleCardService.Instance;

    private SteamLoginState SteamLoginState = new();

    private readonly AsyncLock asyncLock = new AsyncLock();

    public IdleCardPageViewModel()
    {
        SteamIdleSettings.IsAutoNextOn.WhenValueChanged(x => x.Value).Subscribe(RunOrStopAutoNext);

        this.IdleRunStartOrStop = ReactiveCommand.Create(IdleRunStartOrStop_Click);
        this.IdleManualRunNext = ReactiveCommand.Create(ManualRunNext);
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
        base.Activation();

        LoginSteamCommand.Execute(null);
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
                //await SteamConnectService.Current.RefreshGamesListAsync();
                RunState = await ReadyToGoIdle();
                RunOrStopAutoNext(SteamIdleSettings.IsAutoNextOn.Value);
                RunOrStopAutoCardDropCheck(true);
            }
            else
            {
                RunState = false;
                StopIdle();
                RunOrStopAutoNext(false);
                RunOrStopAutoCardDropCheck(false);
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
    public void ManualRunNext()
    {
        RunNextIdle();
    }

    #region PrivateFields

    /// <summary>
    /// 最大并行运行游戏数量
    /// </summary>
    private int MaxIdleCount = 32;

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

    /// <summary>
    /// 加载徽章数据
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadBadges()
    {
        try
        {
            IEnumerable<Badge> badges;

            (UserIdleInfo, badges) = await IdleCard.GetBadgesAsync(SteamLoginState.SteamId.ToString(), true);

            Badges.Clear();
            Badges.Add(badges);
            TotalCardsRemaining = 0;
            TotalCardsAvgPrice = 0;
            IdleTime = default;

            badges = badges.Where(w => w.CardsRemaining != 0);

            foreach (var badge in badges)
            {
                TotalCardsAvgPrice += badge.RegularAvgPrice * badge.CardsRemaining;
                TotalCardsRemaining += badge.CardsRemaining;
            }
            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            IsLogin = false;
            return false;
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

    private async Task<bool> ReadyToGoIdle()
    {
        if (await LoadBadges() && SteamAppsSort())
        {
            StartIdle();
            ChangeRunTxt();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 开始挂卡
    /// </summary>
    private void StartIdle()
    {
        if (SteamLoginState.Success && SteamLoginState.SteamId != (ulong?)SteamConnectService.Current.CurrentSteamUser?.SteamId64)
        {
            Toast.Show(ToastIcon.Error, Strings.SteamIdle_LoginSteamUserError);
            return;
        }

        if (!IdleGameList.Any())
        {
            IdleComplete();
            return;
        }

        DroppedCardsCount = TotalCardsRemaining;

        if (SteamIdleSettings.IdleRule.Value == IdleRule.OnlyOneGame)
        {
            StartSoloIdle(IdleGameList.First());
        }
        else
        {
            if (SteamIdleSettings.IdleRule.Value == IdleRule.OneThenMany)
            {
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed >= SteamIdleSettings.MinRunTime.Value);
                if (multi.Count() >= 1)
                {
                    PauseAutoNext(false);
                    StartSoloIdle(multi.First());
                }
                else
                {
                    PauseAutoNext(true);
                    StartMultipleIdle();
                }
            }
            else
            {
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed < SteamIdleSettings.MinRunTime.Value);
                if (multi.Count() >= 2)
                {
                    PauseAutoNext(true);
                    StartMultipleIdle();
                }
                else
                {
                    PauseAutoNext(false);
                    StartSoloIdle(multi.First());
                }
            }
        }
    }

    private void RunNextIdle()
    {
        if (RunState)
        {
            StopIdle();
            if (CurrentIdle != null)
                IdleGameList.Remove(CurrentIdle);
            StartIdle();
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
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Toast.LogAndShowT(ex);
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
                            AutoCardDropCheck().Wait();
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Toast.LogAndShowT(ex);
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
        if (IdleTime.TotalMinutes != 0 && IdleTime.TotalMinutes % 6 == 0) // 每六分钟刷新徽章信息
        {
            using (await asyncLock.LockAsync())
            {
                if (RunState)
                {
                    await LoadBadges();
                    if (CurrentIdle != null) // 存在单独运行的游戏 检查是否挂卡完成，完成则切换下一个游戏
                    {
                        StopSoloIdle(CurrentIdle.App);
                        if (!Badges.Any(x => x.AppId == CurrentIdle.AppId && x.CardsRemaining > 0))
                            RunNextIdle();
                        else
                            StartSoloIdle(CurrentIdle);

                    }

                    var isMultipleIdle = IdleGameList.Count(x => x.App.Process != null) > 1; // 存在多个挂卡游戏 刷新挂卡列表重新批量挂卡
                    if (isMultipleIdle)
                    {
                        StopIdle();
                        SteamAppsSort();
                        StartIdle();
                    }
                    ChangeRunTxt();
                }
            }
        }
        else
            IdleTime += TimeSpan.FromMilliseconds(100);
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