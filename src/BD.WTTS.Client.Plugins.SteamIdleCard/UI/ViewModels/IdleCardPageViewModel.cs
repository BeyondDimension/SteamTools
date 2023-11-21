using BD.SteamClient.Helpers;
using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using System;
using System.Linq;
using static SteamKit2.Internal.CChatUsability_ClientUsabilityMetrics_Notification;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel : ViewModelBase
{
    readonly ISteamService SteamTool = ISteamService.Instance;
    readonly ISteamIdleCardService IdleCard = ISteamIdleCardService.Instance;

    private DateTimeOffset _StartIdleTime;
    private SteamLoginState SteamLoginState = new();

    public IdleCardPageViewModel()
    {
        this.WhenPropertyChanged(x => x.IsAutoNextOn)
            .Subscribe(x =>
            {
                RunOrStopAutoNext(x.Value);
            });

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
                        await SteamAppsSort();
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

        if (!IsLoaing)
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
            }
            else
            {
                RunState = false;
                StopIdle();
                RunOrStopAutoNext(false);
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
    private CancellationTokenSource CancellationTokenSource = new();

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

    private async Task<bool> SteamAppsSort()
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
            var apps = IdleSequentital switch
            {
                IdleSequentital.LeastCards => badges.OrderBy(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostcards => badges.OrderByDescending(o => o.CardsRemaining).Select(s => new IdleApp(s)),
                IdleSequentital.Mostvalue => badges.OrderByDescending(o => o.RegularAvgPrice).Select(s => new IdleApp(s)),
                _ => badges.Select(s => new IdleApp(s)),
            };

            foreach (var app in apps)
            {
                if (app.Badge.CardsRemaining != 0)// 过滤可掉落卡片的游戏
                {
                    TotalCardsAvgPrice += app.Badge.RegularAvgPrice * app.Badge.CardsRemaining;
                    TotalCardsRemaining += app.Badge.CardsRemaining;
                }
            }

            IdleGameList = new ObservableCollection<IdleApp>(apps);
            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            IsLogin = false;
            return false;
        }
    }

    private async Task<bool> ReadyToGoIdle()
    {
        if (await SteamAppsSort())
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

        _StartIdleTime = DateTimeOffset.Now;
        DroppedCardsCount = TotalCardsRemaining;

        if (IdleRule == IdleRule.OnlyOneGame)
        {
            CurrentIdle = IdleGameList.First();
            StartSoloIdle(CurrentIdle);
        }
        else
        {
            if (IdleRule == IdleRule.OneThenMany)
            {
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed >= MinRunTime);
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
                var multi = IdleGameList.Where(z => z.Badge.HoursPlayed < MinRunTime);
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
            if (item.Badge.HoursPlayed >= MinRunTime)
                StopSoloIdle(item.App);

            if (item.Badge.HoursPlayed < MinRunTime && IdleGameList.Count(x => x.App.Process != null) < MaxIdleCount)
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

    /// <summary>
    /// 暂停自动切换下一个游戏
    /// </summary>
    /// <param name="b"></param>
    private void PauseAutoNext(bool b)
    {
        if (IsAutoNextOn && b && !IsAutoNextPaused)
        {
            IsAutoNextOn = false;
            IsAutoNextPaused = true;
        }
        else if (!IsAutoNextOn && !b && IsAutoNextPaused)
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
                Task2.InBackground(() =>
                {
                    while (!CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            AutoNextTask().Wait();
                            IdleTime = DateTimeOffset.Now - _StartIdleTime;
                            Task.Delay(TimeSpan.FromSeconds(SwitchTime), CancellationTokenSource.Token).Wait();
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
            Toast.Show(ToastIcon.Info, Strings.Idle_PleaseStartIdle);
        }
        else
        {
            if (IsAutoNextPaused)
                IsAutoNextPaused = false;
        }
    }

    /// <summary>
    /// 定时自动切换任务
    /// </summary>
    private async Task AutoNextTask()
    {
        if (IsAutoNextOn == false || IsAutoNextPaused == true)
        {
            CancellationTokenSource.Cancel();
            return;
        }
        if (IdleGameList.Sum(s => s.Badge.CardsRemaining) == 0)
        {
            CancellationTokenSource.Cancel();
            if (IsReloaded == false)
            {
                IsReloaded = true;
                await ReadyToGoIdle();
            }
            else
            {
                IsAutoNextOn = false;
                IdleComplete();
            }
            return;
        }
        else
        {
            IsReloaded = false;
            RunNextIdle();
        }
    }

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