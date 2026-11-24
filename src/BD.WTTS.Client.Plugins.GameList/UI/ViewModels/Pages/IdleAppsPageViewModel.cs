using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleAppsPageViewModel : ViewModelBase
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
    public partial bool RunLoaingState { get; set; }

    [Reactive]

    public partial bool RunState { get; set; }

    [Reactive]
    public partial string? RuningCountTxt { get; set; }

    [Reactive]
    public partial ObservableCollection<SteamApp> IdleGameList { get; set; } = new();

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
                            SteamConnectService.Current.StopIdleApp(item.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.LogAndShowT(nameof(IdleAppsPageViewModel));
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
                        SteamConnectService.Current.StopIdleApp(app.AppId);
                        app.Process = null;
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
        if (!SteamTool.IsRunningSteamProcess)
        {
            await MessageBox.ShowAsync(Strings.SteamNotRuning, button: MessageBox.Button.OK);
            return;
        }
        if (RunLoaingState)
        {
            Toast.Show(ToastIcon.Warning, Strings.GameList_LoaingTips);
            return;
        }

        RunLoaingState = true;
        try
        {
            var start = !RunState;
            for (var index = 0; index < IdleGameList.Count; index++)
            {
                var app = IdleGameList[index];
                if (start)
                    IdleGameList[index] = SteamConnectService.Current.StartIdleApp(app);
                else
                {
                    SteamConnectService.Current.StopIdleApp(app.AppId);
                    app.Process = null;
                }
            }
            Toast.Show(ToastIcon.Success, Strings.GameList_OperationSuccess);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(IdleAppsPageViewModel));
        }
        finally
        {
            RunLoaingState = false;
            ChangeRunTxt();
        }
    }

    public async void RunStopBtn_Click(SteamApp app)
    {
        if (SteamTool.IsRunningSteamProcess)
        {
            var current = SteamConnectService.Current.ToggleIdleApp(app);
            var index = IdleGameList.IndexOf(app);
            if (index >= 0 && !ReferenceEquals(app, current)) IdleGameList[index] = current;

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
            while (SteamConnectService.Current.IsLoadingGameList)
                await Task.Delay(500);

            foreach (var item in GameLibrarySettings.AFKAppList.Value!)
            {
                if (SteamConnectService.Current.TryGetRunningIdleApp(item.Key, out var running))
                {
                    list.Add(running!);
                    continue;
                }
                var cached = SteamConnectService.Current.SteamApps.Lookup(item.Key);
                var app = cached.HasValue ? cached.Value : new SteamApp(item.Key) { Name = item.Value };
                app.Process = null;
                list.Add(app);
            }
        }
        IdleGameList = list;
        ChangeRunTxt(true);
    }
}
