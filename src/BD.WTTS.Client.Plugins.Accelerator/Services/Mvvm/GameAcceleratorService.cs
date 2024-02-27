// ReSharper disable once CheckNamespace
using Avalonia.Threading;
using BD.Common.Models;
using BD.WTTS.UI.Views.Pages;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.Services;

public sealed partial class GameAcceleratorService
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    : ReactiveObject
#endif
{
    static GameAcceleratorService? mCurrent;

    public static GameAcceleratorService Current => mCurrent ??= new();

    public SourceCache<XunYouGameViewModel, int> Games { get; }

    [Reactive]
    public IReadOnlyCollection<XunYouGame>? AllGames { get; set; }

    [Reactive]
    public bool IsLoadingGames { get; set; }

    [Reactive]
    public string? SearchText { get; set; }

    [Reactive]
    public XunYouGameViewModel? CurrentAcceleratorGame { get; set; }

    [Reactive]
    public XunYouAccelStateModel? XYAccelState { get; set; }

    public ICommand DeleteMyGameCommand { get; }

    public ICommand GameAcceleratorCommand { get; }

    public ICommand InstallAcceleratorCommand { get; }

    public ICommand UninstallAcceleratorCommand { get; }

    public ICommand AcceleratorChangeAreaCommand { get; }

    GameAcceleratorService()
    {
        mCurrent = this;

        Games = new(t => t.Id);

        this.WhenPropertyChanged(x => x.XYAccelState, false)
            .Subscribe(x =>
            {
                if (x.Value == null)
                    return;

                switch (x.Value.State)
                {
                    case XunYouState.加速中:
                        if (x.Value.GameId != 0 && CurrentAcceleratorGame == null)
                        {
                            var game2 = Games.Items.FirstOrDefault(s => s.Id == x.Value.GameId);
                            if (game2 != null)
                            {
                                game2.IsAccelerated = false;
                                game2.IsAccelerating = true;
                                game2.LastAccelerateTime = DateTimeOffset.Now;
                                CurrentAcceleratorGame = game2;
                            }
                            else
                            {
                                var game = AllGames?.FirstOrDefault(s => s.Id == x.Value.GameId);
                                if (game != null)
                                {
                                    var cApp = new XunYouGameViewModel(game)
                                    {
                                        IsAccelerated = false,
                                        LastAccelerateTime = DateTimeOffset.Now,
                                        IsAccelerating = true,
                                    };
                                    CurrentAcceleratorGame = cApp;
                                }
                            }
                        }
                        break;
                    case XunYouState.加速已完成:
                        if (x.Value.GameId != 0)
                        {
                            var game2 = Games.Items.FirstOrDefault(s => s.Id == x.Value.GameId);
                            if (game2 != null)
                            {
                                SetGameStatus(game2, x.Value.AreaId, x.Value.ServerId);
                            }
                            else
                            {
                                var game = AllGames?.FirstOrDefault(s => s.Id == x.Value.GameId);
                                if (game != null)
                                {
                                    SetGameStatus(game, x.Value.AreaId, x.Value.ServerId);
                                }
                                else
                                {
                                    RefreshAllGames();
                                }
                            }
                        }
                        break;
                    case XunYouState.未加速:
                    case XunYouState.停止加速中:
                    case XunYouState.加速已断开:
                        //先停止测速
                        XunYouSDK.StopTestSpeded();

                        Games.Items.Where(s => s.IsAccelerating || s.IsAccelerated).ForEach(s =>
                        {
                            RestoreGameStatus(s);
                            Games.Refresh(s);
                        });
                        if (CurrentAcceleratorGame != null)
                        {
                            RestoreGameStatus(CurrentAcceleratorGame);
                            CurrentAcceleratorGame = null;
                        }
                        break;
                    case XunYouState.登录中:
                    case XunYouState.登录失败:
                    case XunYouState.登录成功:
                    case XunYouState.启动中:
                    case XunYouState.启动成功:
                        break;
                    default:
                        Games.Items.Where(s => s.IsAccelerating || s.IsAccelerated).ForEach(s =>
                        {
                            RestoreGameStatus(s);
                            Games.Refresh(s);
                        });
                        if (CurrentAcceleratorGame != null)
                        {
                            RestoreGameStatus(CurrentAcceleratorGame);
                            CurrentAcceleratorGame = null;
                        }
                        Toast.Show(ToastIcon.Warning, x.Value.State.ToString());
                        break;
                }
            });

        Ioc.Get<IAcceleratorService>().InitStateSubscribe(); // 仅旧项目（WattToolkit）上监听

        RefreshAllGames();
        LoadGames();
        DeleteMyGameCommand = ReactiveCommand.Create<XunYouGameViewModel>(DeleteMyGame);
        GameAcceleratorCommand = ReactiveCommand.CreateFromTask<XunYouGameViewModel>(GameAccelerator);
        InstallAcceleratorCommand = ReactiveCommand.Create(InstallAccelerator);
        UninstallAcceleratorCommand = ReactiveCommand.Create(UninstallAccelerator);
        AcceleratorChangeAreaCommand = ReactiveCommand.Create<XunYouGameViewModel>(AcceleratorChangeArea);
    }

    /// <summary>
    /// 恢复加速游戏状态
    /// </summary>
    private static void RestoreGameStatus(XunYouGameViewModel app)
    {
        app.SetPingValue(0);
        app.PingSpeedLoss = 0;
        app.IsAccelerating = false;
        app.IsAccelerated = false;
        app.IsStopAccelerating = false;
    }

    /// <summary>
    /// 设置加速游戏状态
    /// </summary>
    void SetGameStatus(XunYouGameViewModel game, int areaId = 0, int serverId = 0)
    {
        if (CurrentAcceleratorGame != null && CurrentAcceleratorGame.Id == game.Id)
        {
            CurrentAcceleratorGame.IsAccelerating = false;
            CurrentAcceleratorGame.IsAccelerated = true;
            CurrentAcceleratorGame.LastAccelerateTime = DateTimeOffset.Now;

            if (CurrentAcceleratorGame.SelectedArea?.Id != areaId)
            {
                var gameInfo = XunYouSDK.GetGameInfo(game.Id);
                CurrentAcceleratorGame.SelectedArea = gameInfo?.Areas?.FirstOrDefault(s => s.Id == areaId);
            }
            if (CurrentAcceleratorGame.SelectedServer?.Id != serverId)
            {
                CurrentAcceleratorGame.SelectedServer = CurrentAcceleratorGame.SelectedArea?.Servers?.FirstOrDefault(s => s.Id == serverId);
            }
        }
        else
        {
            var cApp = new XunYouGameViewModel(game)
            {
                IsAccelerated = true,
                LastAccelerateTime = DateTimeOffset.Now,
                IsAccelerating = false,
            };

            var gameInfo = XunYouSDK.GetGameInfo(game.Id);
            cApp.SelectedArea = gameInfo?.Areas?.FirstOrDefault(s => s.Id == areaId);
            cApp.SelectedServer = cApp.SelectedArea?.Servers?.FirstOrDefault(s => s.Id == serverId);

            CurrentAcceleratorGame = cApp;
        }

        if (GameAcceleratorSettings.MyGames.ContainsKey(CurrentAcceleratorGame.Id))
        {
            GameAcceleratorSettings.MyGames.Remove(CurrentAcceleratorGame.Id);
        }
        GameAcceleratorSettings.MyGames.TryAdd(CurrentAcceleratorGame.Id, CurrentAcceleratorGame);
        Games.AddOrUpdate(CurrentAcceleratorGame);

        //加速后
        Toast.Show(ToastIcon.Success, "加速成功");
        int testSpeedCallback(SpeedCallbackWrapper w)
        {
            if (CurrentAcceleratorGame != null)
            {
                CurrentAcceleratorGame.SetPingValue(w.Struct.PingSpeed);
                CurrentAcceleratorGame.PingSpeedLoss = w.Struct.PingSpeedLoss;
            }
#if DEBUG
            Console.WriteLine($"测速通知状态：{w.State},SpeedCallbackInfo: ErrorDesc/{w.ErrorDesc}, ErrorCode/{w.Struct.ErrorCode}, PingSpeed/{w.Struct.PingSpeed}, PingLocal/{w.Struct.PingLocal}, PingSpeedLoss/{w.Struct.PingSpeedLoss}, PingLocalLoss/{w.Struct.PingLocalLoss}");
#endif
            return 0;
        }
        var speedCode = XunYouSDK.TestSpeed(CurrentAcceleratorGame.Id, CurrentAcceleratorGame.SelectedArea!.Id, testSpeedCallback);
#if DEBUG
        if (speedCode == XunYouTestSpeedCode.成功)
        {
            Toast.Show(ToastIcon.Info, "发送测速指令");
        }
#endif
    }

    public async Task GameAccelerator(XunYouGameViewModel app)
    {
        if (app.IsAccelerating)
            return;

        app.IsAccelerating = true;
        if (!app.IsAccelerated)
        {
            if (UserService.Current.User?.WattOpenId == null)
            {
                Toast.Show(ToastIcon.Warning, "需要登录账号才能使用游戏加速功能!");
                app.IsAccelerating = false;
                return;
            }
            var xunYouIsInstall = await Ioc.Get<IAcceleratorService>().XY_IsInstall();
            if (xunYouIsInstall.HandleUI(out var isInstall))
            {
                if (!isInstall)
                {
                    if (!await IWindowManager.Instance.ShowTaskDialogAsync(
                                   new MessageBoxWindowViewModel() { Content = "需要下载 Watt 加速器插件才可使用，确定要下载吗?" }, title: "未安装加速插件", isCancelButton: true))
                    {
                        app.IsAccelerating = false;
                        return;
                    }

                    await InstallAccelerator();
                }

                #region 通过 XY_GetAccelStateEx 判断是否已启动迅游

                //var accStateR = await IAcceleratorService.Instance.XY_GetAccelStateEx();
                //bool isStartXY = false;
                //if (accStateR.HandleUI(out var accStateResponse))
                //{
                //    isStartXY = accStateResponse.AccelState != XunYouAccelStateEx.获取失败;
                //    if (isStartXY && accStateResponse.State != XunYouState.未加速)
                //    {
                //        Toast.Show(ToastIcon.Warning, accStateResponse.State.ToString() ?? "失败");
                //        return;
                //    }
                //}

                #endregion

                #region 通过 XY_IsRunning 判断是否已启动迅游

                var apiRspIsRunning = await Ioc.Get<IAcceleratorService>().XY_IsRunning();
                if (!apiRspIsRunning.HandleUI(out var isRunningCode))
                {
                    // 调用后端失败，显示错误并中止逻辑
                    app.IsAccelerating = false;
                    return;
                }

                bool isStartXY = isRunningCode == XunYouIsRunningCode.加速器已启动;

                #endregion

                //加速中

                var gameInfo = XunYouSDK.GetGameInfo(app.Id);
                if (gameInfo == null)
                {
                    Toast.Show(ToastIcon.Warning, "获取游戏信息失败");
                    app.IsAccelerating = false;
                    return;
                }
                app.GameInfo = gameInfo;
                var vm = new GameInfoPageViewModel(app);
                var result = await IWindowManager.Instance.ShowTaskDialogAsync(vm, $"{app.Name} - 区服选择", pageContent: new GameInfoPage(), isOkButton: false, disableScroll: true);
                if (!result || app.SelectedArea == null)
                {
                    app.IsAccelerating = false;
                    return;
                }

                if (GameAcceleratorSettings.MyGames.ContainsKey(app.Id))
                {
                    GameAcceleratorSettings.MyGames.Remove(app.Id);
                }
                GameAcceleratorSettings.MyGames.TryAdd(app.Id, app);
                Current.Games.AddOrUpdate(app);

                var start = isStartXY ? await Ioc.Get<IAcceleratorService>().XY_StartAccel(app.Id,
                    app.SelectedArea.Id,
                    app.SelectedServer?.Id ?? 0,
                    app.SelectedArea.Name,
                    app.SelectedServer?.Name)
                    : await Ioc.Get<IAcceleratorService>().XY_StartEx2(
                        UserService.Current.User.WattOpenId,
                        UserService.Current.User.NickName,
                        app.Id,
                        app.SelectedArea.Id,
                        app.SelectedServer?.Id ?? 0,
                        app.SelectedArea.Name,
                        app.SelectedServer?.Name);
                if (start.HandleUI(out var startCode))
                {
                    if (startCode == 101)
                    {
                        app.AcceleratingProgress = 0;
                        app.LastAccelerateTime = DateTimeOffset.Now;
                        CurrentAcceleratorGame = app;
                        Toast.Show(ToastIcon.Info, "正在加速中...");
                    }
                    else
                    {
                        Toast.Show(ToastIcon.Error, "加速启动失败");
                        app.IsAccelerating = false;
                    }
                }
                else
                {
                    app.IsAccelerating = false;
                }
            }
        }
        else
        {
            var stopRequest = await Ioc.Get<IAcceleratorService>().XY_StopAccel();
            if (stopRequest.HandleUI(out var code))
            {
                // 停止加速
                if (code == XunYouSendResultCode.发送成功)
                {
                    Toast.Show(ToastIcon.Info, "正在停止加速中...");
                }
                else
                {
                    app.IsAccelerating = false;
                    Toast.Show(ToastIcon.Error, "停止加速失败，请尝试去加速器客户端停止加速");
                }
            }
        }
    }

    public static void DeleteMyGame(XunYouGameViewModel app)
    {
        //if (await WindowManagerService.Current.ShowTaskDialogAsync(
        //    new MessageBoxWindowViewModel() { Content = Strings.Script_ReplaceTips }, isCancelButton: true, isDialog: false))
        //{
        //}
        Current.Games.RemoveKey(app.Id);
        GameAcceleratorSettings.MyGames.Remove(app.Id);
        Toast.Show(ToastIcon.Success, "已移除");
    }

    public static void AdddMyGame(XunYouGameViewModel app)
    {
        app.LastAccelerateTime = DateTimeOffset.Now;
        GameAcceleratorSettings.MyGames.Remove(app.Id);
        if (GameAcceleratorSettings.MyGames.TryAdd(app.Id, app))
        {
            Current.Games.AddOrUpdate(app);
            Toast.Show(ToastIcon.Success, "已添加到游戏列表");
        }
    }

    /// <summary>
    /// 加载迅游游戏数据
    /// </summary>
    public void LoadGames()
    {
        if (!XunYouSDK.IsSupported)
            return;

        Task2.InBackground(() =>
        {
            if (!IsLoadingGames)
            {
                IsLoadingGames = true;
                if (GameAcceleratorSettings.MyGames.Any_Nullable())
                {
                    Games.Clear();
                    Games.AddOrUpdate(GameAcceleratorSettings.MyGames.Value!.Values);
                }
                else
                {
                    var games = XunYouSDK.GetHotGames();
                    if (games != null)
                    {
                        Games.Clear();
                        GameAcceleratorSettings.MyGames.AddRange(games.Select(s => new KeyValuePair<int, XunYouGameViewModel>(s.Id, new XunYouGameViewModel(s))));
                        Games.AddOrUpdate(GameAcceleratorSettings.MyGames.Value!.Values);
                    }
                }

                IsLoadingGames = false;
            }
        });
    }

    public void RefreshAllGames()
    {
        Task2.InBackground(async () =>
        {
            var games = XunYouSDK.GetAllGames();
            if (games != null)
            {
                AllGames = new ReadOnlyCollection<XunYouGame>(games);
            }

            //判断是否已经在加速
            var accState = await Ioc.Get<IAcceleratorService>().XY_GetAccelStateEx();
            if (accState.HandleUI() && accState.Content != null)
            {
                if (accState.Content.GameId > 0 && accState.Content.AccelState is
                    XunYouAccelStateEx.启动加速中 or
                    XunYouAccelStateEx.已加速)
                {
                    //var game = Games.Lookup(accState.Content.GameId);
                    //if (game.HasValue)
                    //{
                    //    game.Value.IsAccelerated = true;
                    //    game.Value.LastAccelerateTime = DateTime.Now;
                    //    var gameinfo = XunYouSDK.GetGameInfo(game.Value.Id);
                    //    game.Value.SelectedArea = gameinfo?.Areas?.FirstOrDefault(s => s.Id == accState.Content.AreaId);
                    //}

                    var game = games?.FirstOrDefault(s => s.Id == accState.Content.GameId);
                    if (game != null)
                    {
                        SetGameStatus(game, accState.Content.AreaId, accState.Content.ServerId);
                    }
                }
            }
        });
    }

    public static async Task InstallAccelerator()
    {
        var xunYouIsInstall = await Ioc.Get<IAcceleratorService>().XY_IsInstall();
        if (xunYouIsInstall.HandleUI(out var isInstall))
        {
            if (isInstall)
            {
                Toast.Show(ToastIcon.Info, "已安装Watt加速器");
                return;
            }
            var td = new TaskDialog
            {
                Title = "下载插件",
                ShowProgressBar = true,
                IconSource = new SymbolIconSource { Symbol = FluentAvalonia.UI.Controls.Symbol.Download },
                SubHeader = "下载 Watt 加速器 插件",
                Content = "正在初始化，请稍候",
                XamlRoot = AvaloniaWindowManagerImpl.GetWindowTopLevel(),
                //Buttons =
                //{
                //    new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Cancel)
                //}
            };
            var install = Ioc.Get<IAcceleratorService>().XY_Install(GameAcceleratorSettings.WattAcceleratorDirPath.Value!);

            td.Opened += async (s, e) =>
            {
                await foreach (var item in install)
                {
                    if (item.HandleUI(out var content))
                    {
                        switch (content)
                        {
                            case < 100:
                                Dispatcher.UIThread.Post(() => { td.Content = $"正在下载 {item.Content}%"; });
                                td.SetProgressBarState(item.Content, TaskDialogProgressState.Normal);
                                break;
                            case 100:
                                td.SetProgressBarState(item.Content, TaskDialogProgressState.Indeterminate);
                                Dispatcher.UIThread.Post(() => { td.Content = $"下载完成，正在安装..."; });
                                break;
                            case (int)XunYouDownLoadCode.安装成功:
                                //处理成功
                                //Dispatcher.UIThread.Post(() => { td.Content = $"安装完成"; });
                                Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.OK); });
                                td.Hide();
                                break;
                            case int n when n > 101 && n < (int)XunYouDownLoadCode.启动安装程序失败:
                                //处理失败
                                break;
                            // Code 和进度重叠 递进 1000 XunYouInstallOrStartCode.默认 XunYouInstallOrStartCode.已安装
                            case 1000:
                                Dispatcher.UIThread.Post(() => { td.Content = $"默认"; });
                                // XunYouInstallOrStartCode.默认
                                break;
                            case 1001:
                                Dispatcher.UIThread.Post(() => { td.Content = $"已安装"; });
                                // XunYouInstallOrStartCode.已安装
                                Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.OK); });
                                break;
                        }
                    }
                }
            };

            //_ = Task.Run(() => { XunYouSDK.InstallAsync(progress, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WattAccelerator")); });

            var result = await td.ShowAsync(true);
        }
    }

    public static async void UninstallAccelerator()
    {
        var xunYouIsInstall = await Ioc.Get<IAcceleratorService>().XY_IsInstall();
        if (xunYouIsInstall.HandleUI(out var isInstall))
        {
            if (!isInstall)
            {
                Toast.Show(ToastIcon.Info, "未安装，不需要卸载");
                return;
            }
            var uninstall = await Ioc.Get<IAcceleratorService>().XY_Uninstall();
            if (uninstall.HandleUI(out var code))
            {
                if (code == 0)
                {
                    Toast.Show(ToastIcon.Success, "卸载成功");
                }
                else
                {
                    Toast.Show(ToastIcon.Error, "卸载失败");
                }
            }
        }
    }

    public async void AcceleratorChangeArea(XunYouGameViewModel app)
    {
        var gameInfo = XunYouSDK.GetGameInfo(app.Id);
        if (gameInfo == null)
        {
            Toast.Show(ToastIcon.Warning, "获取游戏信息失败");
            return;
        }
        app.GameInfo = gameInfo;

        var vm = new GameInfoPageViewModel(new XunYouGameViewModel { Name = app.Name, GameInfo = gameInfo, SelectedArea = app.SelectedArea, SelectedServer = app.SelectedServer });
        var result = await IWindowManager.Instance.ShowTaskDialogAsync(vm, $"{app.Name} - 区服选择", pageContent: new GameInfoPage(), isOkButton: false, disableScroll: true);
        if (!result || vm.XunYouGame.SelectedArea == null)
        {
            return;
        }

        if (app.SelectedArea?.Id == vm.XunYouGame.SelectedArea?.Id && app.SelectedServer?.Id == vm.XunYouGame.SelectedServer?.Id)
        {
            // 切换区服选择不变时不重新加速
            return;
        }

        app.SelectedArea = vm.XunYouGame.SelectedArea;
        app.SelectedServer = vm.XunYouGame.SelectedServer;

        if (app.IsAccelerated)
        {
            //重新加速
            var start = await Ioc.Get<IAcceleratorService>().XY_StartAccel(app.Id, app.SelectedArea?.Id ?? 0, app.SelectedServer?.Id ?? 0, app.SelectedArea?.Name);
            if (start.HandleUI(out var startCode))
            {
                if (startCode == 101)
                {
                    app.IsAccelerating = true;
                    app.IsAccelerated = false;
                    app.AcceleratingProgress = 0;
                    CurrentAcceleratorGame = app;
                    Toast.Show(ToastIcon.Info, "正在加速中...");
                }
                else
                {
                    Toast.Show(ToastIcon.Error, "加速启动失败");
                    app.IsAccelerating = false;
                }
            }
            else
            {
                app.IsAccelerating = false;
            }
        }
        else
        {
            await GameAccelerator(app);
        }
    }

    public async Task RefreshXYAccelState()
    {
        var result = await Ioc.Get<IAcceleratorService>().XY_GetAccelStateEx();
        if (result.HandleUI(out var content))
        {
            XYAccelState = content;
        }
    }
}