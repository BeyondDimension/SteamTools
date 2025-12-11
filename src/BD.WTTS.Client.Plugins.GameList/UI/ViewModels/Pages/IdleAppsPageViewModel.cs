using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed class IdleAppsPageViewModel : ViewModelBase
{
    readonly ISteamService SteamTool = ISteamService.Instance;

    public static string DisplayName => Strings.GameList_IdleGamesManger;

    public ICommand RunStopBtnCommand { get; }

    public ICommand DeleteButtonCommand { get; }

    public IdleAppsPageViewModel()
    {
        CompositeDisposable.Add(GameLibrarySettings.AFKAppList.Subscribe(_ => Refresh_Click(), false));

        RunStopBtnCommand = ReactiveCommand.Create<SteamApp>(RunStopBtn_Click);
        DeleteButtonCommand = ReactiveCommand.Create<SteamApp>(DeleteButton_Click);

        Refresh_Click();
    }

    [Reactive]
    public bool RunLoaingState { get; set; }

    [Reactive]

    public bool RunState { get; set; }

    [Reactive]
    public string? RuningCountTxt { get; set; }

    [Reactive]
    public ObservableCollection<SteamApp> IdleGameList { get; set; } = new();

    public void DeleteAllButton_Click()
    {
        var result = MessageBox.ShowAsync(Strings.GameList_DeleteAll, button: MessageBox.Button.OKCancel).ContinueWith((s) =>
        {
            if (s.Result.IsOK())
            {
                if (GameLibrarySettings.AFKAppList.Value != null)
                {
                    try
                    {
                        foreach (var item in GameLibrarySettings.AFKAppList.Value)
                        {
                            SteamConnectService.Current.RuningSteamApps.TryGetValue(item.Key, out var runState);
                            if (runState != null)
                            {
                                runState.Process?.KillEntireProcessTree();
                                SteamConnectService.Current.RuningSteamApps.TryRemove(item.Key, out runState);
                            }
                        }
                    }
                    catch
                    {
                    }
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        IdleGameList.Clear();
                        RunState = false;
                        GameLibrarySettings.AFKAppList.Value.Clear();
                        GameLibrarySettings.AFKAppList.RaiseValueChanged();
                        Toast.Show(ToastIcon.Success, Strings.GameList_DeleteSuccess);
                    });
                }
            }
        });
    }

    public void DeleteButton_Click(SteamApp app)
    {
        var result = MessageBox.ShowAsync(Strings.GameList_DeleteItem, button: MessageBox.Button.OKCancel).ContinueWith((s) =>
        {
            if (s.Result.IsOK())
            {
                if (GameLibrarySettings.AFKAppList.Value != null)
                {
                    var item = GameLibrarySettings.AFKAppList.Value.ContainsKey(app.AppId);
                    if (item)
                    {
                        SteamConnectService.Current.RuningSteamApps.TryGetValue(app.AppId, out var runState);
                        if (runState != null)
                        {
                            runState.Process?.KillEntireProcessTree();
                            SteamConnectService.Current.RuningSteamApps.TryRemove(app.AppId, out runState);
                        }
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            IdleGameList.Remove(app);
                            ChangeRunTxt();
                            GameLibrarySettings.AFKAppList.Value.Remove(app.AppId);
                            GameLibrarySettings.AFKAppList.RaiseValueChanged();
                            Toast.Show(ToastIcon.Success, Strings.GameList_DeleteSuccess);
                        });
                    }
                }
            }
        });
    }

    public void ChangeRunTxt(bool title = false)
    {
        var count = IdleGameList.Count(x => x.Process != null);
        RuningCountTxt = Strings.GameList_RuningCount.Format(count, IdleGameList.Count);
        RunState = count > 0;
        if (title)
        {
            var titleStr = Strings.GameList_IdleGamesManger + Strings.GameList_ListCount.Format(IdleGameList.Count, SteamConnectService.SteamAFKMaxCount);
        }
    }

    public async void RunOrStopAllButton_Click()
    {
        if (SteamTool.IsRunningSteamProcess)
        {
            if (!RunLoaingState)
            {
                RunLoaingState = true;
                RunState = !RunState;
                if (RunState)
                {
                    foreach (var item in IdleGameList)
                    {
                        SteamConnectService.Current.RuningSteamApps.TryGetValue(item.AppId, out var runState);
                        if (runState == null)
                        {
                            item.StartSteamAppProcess();
                            SteamConnectService.Current.RuningSteamApps.TryAdd(item.AppId, item);
                        }
                        else
                        {
                            if (runState.Process == null || !runState.Process.HasExited)
                            {
                                runState.StartSteamAppProcess();
                            }
                            else
                            {
                                item.Process = runState.Process;
                            }
                        }
                    }
                    ChangeRunTxt();
                }
                else
                {
                    foreach (var item in IdleGameList)
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
                }
                RunLoaingState = false;
                Toast.Show(ToastIcon.Success, Strings.GameList_OperationSuccess);
            }
            else
            {
                Toast.Show(ToastIcon.Warning, Strings.GameList_LoaingTips);
            }
        }
        else
        {
            await MessageBox.ShowAsync(Strings.SteamNotRuning, button: MessageBox.Button.OK);
        }
    }

    public async void RunStopBtn_Click(SteamApp app)
    {
        if (SteamTool.IsRunningSteamProcess)
        {
            SteamConnectService.Current.RuningSteamApps.TryGetValue(app.AppId, out var runInfoState);
            if (runInfoState == null)
            {
                app.RunOrStopSteamAppProcess();
                SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
            }
            else
            {
                runInfoState.RunOrStopSteamAppProcess();
                app.Process = runInfoState.Process;
            }

            ChangeRunTxt();
            Toast.Show(ToastIcon.Success, Strings.GameList_OperationSuccess);
        }
        else
        {
            await MessageBox.ShowAsync(Strings.SteamNotRuning, button: MessageBox.Button.OK);
        }
    }

    public async void Refresh_Click()
    {
        var list = new ObservableCollection<SteamApp>();
        if (GameLibrarySettings.AFKAppList.Any_Nullable())
        {
            int runOtherAppCount = 0;

            while (SteamConnectService.Current.IsLoadingGameList)
            {
                await Task.Delay(500);
            }

            foreach (var item in GameLibrarySettings.AFKAppList.Value!)
            {
                var appInfo = SteamConnectService.Current.SteamApps.Items.FirstOrDefault(x => x.AppId == item.Key);
                if (appInfo != null)
                {
                    SteamConnectService.Current.RuningSteamApps.TryGetValue(appInfo.AppId, out var runState);
                    if (runState != null && runState.Process != null)
                    {
                        RunState = true;
                        if (!runState.Process.HasExited)
                        {
                            appInfo.Process = runState.Process;
                            appInfo.Process.Exited += (_, _) =>
                            {
                                SteamConnectService.Current.RuningSteamApps.TryRemove(appInfo.AppId, out runState);
                            };
                            appInfo.Process.EnableRaisingEvents = true;
                        }
                        else
                            appInfo.Process = null;
                    }
                    else
                    {
                        appInfo.Process = null;
                    }
                    list.Add(appInfo);
                }
                else
                {
                    runOtherAppCount++;
                    SteamConnectService.Current.RuningSteamApps.TryGetValue(item.Key, out var runState);
                    runState?.Process?.KillEntireProcessTree();
                    list.Add(new SteamApp
                    {
                        AppId = item.Key,
                        Name = item.Value
                    });
                }
            }

            if (runOtherAppCount > 0)
            {
                Toast.Show(ToastIcon.Warning, Strings.GameList_RunOtherAppCount_.Format(runOtherAppCount));
            }
        }
        IdleGameList = list;
        ChangeRunTxt(true);
    }
}
