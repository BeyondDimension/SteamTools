using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;
using BD.WTTS.UI.Views.Pages;
using System;
using System.Linq;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel : ViewModelBase
{
    readonly ISteamService SteamTool = ISteamService.Instance;
    readonly ISteamIdleCardService IdleCard = ISteamIdleCardService.Instance;

    private SteamLoginState SteamLoginState = new();

    public IdleCardPageViewModel()
    {
        this.WhenPropertyChanged(x => x.IsAutoNextOn)
            .Subscribe(x =>
            {
                RunOrStopAutoNext(x.Value);
                this.IsAutoNextOnTxt = x.Value ? Strings.Idle_StopAutoNext : Strings.Idle_OpenAutoNext;
            });

        this.IdleRunStartOrStop = ReactiveCommand.Create(IdleRunStartOrStop_Click);
        this.IdleManualRunNext = ReactiveCommand.Create(ManualRunNext);
        this.LoginSteamCommand = ReactiveCommand.Create(async () =>
        {
            if (!IsLoaing)
            {
                IsLoaing = true;
                await LoginSteam();
                await SteamAppsSort();
                IsLoaing = false;
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
    public async void IdleRunStartOrStop_Click()
    {
        if (!SteamTool.IsRunningSteamProcess)
        {
            await MessageBox.ShowAsync(Strings.Idle_SteamNotRuning, button: MessageBox.Button.OK);
            return;
        }

        if (!SteamConnectService.Current.IsConnectToSteam) // 是否登录 Steam 客户端
        {
            await MessageBox.ShowAsync(Strings.Idle_NeedLoginSteam, button: MessageBox.Button.OK);
            return;
        }

        if (!await LoginSteam()) // 登录 Steam Web
            return;

        if (!RunLoaingState)
        {
            RunLoaingState = true;
            RunState = !RunState;

            if (RunState)
            {
                await SteamConnectService.Current.RefreshGamesListAsync();
                await ReadyToGoIdle();
            }
            else
            {
                StopIdle();
                RunOrStopAutoNext(false);
            }
            RunLoaingState = false;
            Toast.Show(ToastIcon.Success, Strings.Idle_OperationSuccess);
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
        var seesion = await Ioc.Get<ISteamSessionService>()
            .LoadSession(Path.Combine(Plugin.Instance.CacheDirectory));

        if (seesion != null && ulong.TryParse(seesion.SteamId, out var steamid))
        {
            SteamLoginState.SteamId = steamid;
            SteamLoginState.AccessToken = seesion.AccessToken;
            SteamLoginState.RefreshToken = seesion.RefreshToken;
            SteamLoginState.Cookies = seesion.CookieContainer.GetAllCookies();

            //var success = await Ioc.Get<ISteamAccountService>().CheckAccessTokenValidation(SteamLoginState.AccessToken);
            SteamLoginState.Success = true;
            //SteamLoginState.SteamId = success ? steamid : 0;
        }

        if (!SteamLoginState.Success)
        {
            var vm = new IdleSteamLoginPageViewModel(ref SteamLoginState);
            await IWindowManager.Instance.ShowTaskDialogAsync(vm, Strings.Steam_Login,
                pageContent: new IdleSteamLoginPage(), okButtonText: Strings.Confirm, isOkButton: false);

            return SteamLoginState.Success;
        }
        return true;
    }

    private void ChangeRunTxt()
    {
        var count = IdleGameList.Count(x => x.App.Process != null);
        RuningCountTxt = Strings.Idle_RuningCount.Format(count, IdleGameList.Count);
        RunState = count > 0;
    }

    private async Task SteamAppsSort()
    {
        try
        {
            IEnumerable<Badge> badges;
            //if (IdleSequentital == IdleSequentital.Mostvalue)
            //{
            (UserIdleInfo, badges) = await IdleCard.GetBadgesAsync(SteamConnectService.Current.CurrentSteamUser!.SteamId64.ToString(), true);
            //}
            //else
            //{
            //    (UserIdleInfo, badges) = await IdleCard.GetBadgesAsync(SteamConnectService.Current.CurrentSteamUser!.SteamId64.ToString());
            //}

            Badges.Clear();
            TotalCardsRemaining = 0;
            TotalCardsAvgPrice = 0;
            foreach (var b in badges)
            {
                if (b.CardsRemaining != 0)// 过滤可掉落卡片的游戏
                {
                    TotalCardsAvgPrice += b.RegularAvgPrice * b.CardsRemaining;
                    TotalCardsRemaining += b.CardsRemaining;
                    Badges.Add(b);
                }
            }
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
            return;
        }

        var apps = Enumerable.Empty<IdleApp>();
        apps = IdleSequentital switch
        {
            IdleSequentital.LeastCards => Badges.OrderBy(o => o.CardsRemaining).Select(s => new IdleApp(s)),
            IdleSequentital.Mostcards => Badges.OrderByDescending(o => o.CardsRemaining).Select(s => new IdleApp(s)),
            IdleSequentital.Mostvalue => Badges.OrderByDescending(o => o.RegularAvgPrice).Select(s => new IdleApp(s)),
            _ => Badges.Select(s => new IdleApp(s)),
        };

        //不应该使用 SteamConnectService 的 apps
        //var apps = SteamConnectService.Current.SteamApps.Items
        //    .Where(x => appid_sorts.Contains((int)x.AppId))
        //    .OrderBy(o => appid_sorts.ToList().FindIndex(x => x == o.AppId))
        //    .ToList();
        IdleGameList.Add(apps);
    }

    private async Task ReadyToGoIdle()
    {
        await SteamAppsSort();
        StartIdle();
        ChangeRunTxt();
    }

    /// <summary>
    /// 开始挂卡
    /// </summary>
    private void StartIdle()
    {
        if (!IdleGameList.Any())
        {
            IdleComplete();
            return;
        }

        if (IdleRule == IdleRule.OnlyOneGame)
        {
            CurrentIdle = IdleGameList.First();
            StartSoloIdle(CurrentIdle);
        }
        else
        {
            if (IdleRule == IdleRule.OneThenMany)
            {
                var canIdles = Badges.Where(z => z.HoursPlayed >= MinRunTime).Select(s => s.AppId);
                var multi = IdleGameList.Where(x => canIdles.Contains(x.AppId));
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
                var canIdles = Badges.Where(z => z.HoursPlayed < MinRunTime).Select(s => s.AppId);
                var multi = IdleGameList.Where(x => canIdles.Contains(x.AppId));
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
            var badge = Badges.FirstOrDefault(x => x.AppId == item.AppId);

            if (badge == null)
                continue;

            if (badge.HoursPlayed >= MinRunTime)
                StopSoloIdle(item.App);

            if (badge.HoursPlayed < MinRunTime && IdleGameList.Count(x => x.App.Process != null) < MaxIdleCount)
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
                Task.Run(async () =>
                {
                    while (!CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await AutoNextTask();
                            await Task.Delay(TimeSpan.FromSeconds(SwitchTime), CancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Toast.LogAndShowT(ex);
                        }
                    }
                });
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
        if (Badges.Where(x => IdleGameList.Select(s => s.AppId).Contains(x.AppId)).Sum(s => s.CardsRemaining) == 0)
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
        Toast.Show(ToastIcon.Success, Strings.Idle_Complete);
    }
    #endregion

}