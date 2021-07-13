using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Properties;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class IdleAppWindowViewModel : WindowViewModel
    {
        private bool init = false;
        private readonly ISteamService SteamTool = ISteamService.Instance;
        public IdleAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_IdleGamesManger;

            GameLibrarySettings.AFKAppList.Subscribe(_ => Refresh_Click());

            Refresh_Click();
        }
        private string? _RunStateTxt;
        public string? RunStateTxt
        {
            get => _RunStateTxt;
            set => this.RaiseAndSetIfChanged(ref _RunStateTxt, value);
        }
        private bool _RunLoaingState;
        public bool RunLoaingState
        {
            get => _RunLoaingState;
            set => this.RaiseAndSetIfChanged(ref _RunLoaingState, value);
        }
        private bool _RunState;
        public bool RunState
        {
            get => _RunState;
            set
            {
                RunStateTxt = value ? AppResources.GameList_StopBtn : AppResources.GameList_RuningBtn;
                this.RaiseAndSetIfChanged(ref _RunState, value);
            }
        }
        public bool _IsIdleAppEmpty;
        public bool IsIdleAppEmpty
        {
            get => _IsIdleAppEmpty;
            set => this.RaiseAndSetIfChanged(ref _IsIdleAppEmpty, value);
        }
        public string _RuningCountTxt;
        public string RuningCountTxt
        {
            get => _RuningCountTxt;
            set
            {
                this.RaiseAndSetIfChanged(ref _RuningCountTxt, value);
            }
        }

        public ObservableCollection<SteamApp> _IdleGameList = new();
        public ObservableCollection<SteamApp> IdleGameList
        {
            get => _IdleGameList;
            set
            {
                this.RaiseAndSetIfChanged(ref _IdleGameList, value);
            }
        }
        public void DeleteAllButton_Click()
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.GameList_DeleteAll, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
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
                                    runState.Process?.Kill();
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
                            IsIdleAppEmpty = true;
                            GameLibrarySettings.AFKAppList.Value.Clear();
                            GameLibrarySettings.AFKAppList.RaiseValueChanged();
                            Toast.Show(AppResources.GameList_DeleteSuccess);
                        });

                    }
                }
            });
        }
        public void DeleteButton_Click(SteamApp app)
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.GameList_DeleteItem, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    if (GameLibrarySettings.AFKAppList.Value != null)
                    {
                        var item = GameLibrarySettings.AFKAppList.Value.ContainsKey(app.AppId);
                        if (item)
                        {
                            SteamConnectService.Current.RuningSteamApps.TryGetValue(app.AppId, out var runState);
                            if (runState != null)
                            {
                                runState.Process?.Kill();
                                SteamConnectService.Current.RuningSteamApps.TryRemove(app.AppId, out runState);
                            }
                            MainThread2.BeginInvokeOnMainThread(() =>
                            {
                                IdleGameList.Remove(app);
                                ChangeRunTxt();
                                GameLibrarySettings.AFKAppList.Value.Remove(app.AppId);
                                GameLibrarySettings.AFKAppList.RaiseValueChanged();
                                Toast.Show(AppResources.GameList_DeleteSuccess);
                            });
                        }

                    }
                }
            });

        }

        public void ChangeRunTxt(bool title = false)
        {
            var count = IdleGameList.Count(x => x.Process != null);
            RuningCountTxt = AppResources.GameList_RuningCount.Format(count, IdleGameList.Count);
            RunState = count > 0;
            if (title)
                Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_IdleGamesManger + AppResources.GameList_ListCount.Format(IdleGameList.Count, SteamConnectService.Current.SteamAFKMaxCount);
        }
        public void RunOrStopAllButton_Click()
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
                                runState.Process?.Kill();
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
                    Toast.Show(AppResources.GameList_OperationSuccess);
                }
                else
                {
                    Toast.Show(AppResources.GameList_LoaingTips);
                }
            }
            else
            {
                var result = MessageBoxCompat.ShowAsync(@AppResources.GameList_SteamNotRuning, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OK);
            }
        }
        public void RunStopBtn_Click(SteamApp app)
        {
            if (SteamTool.IsRunningSteamProcess)
            {
                SteamConnectService.Current.RuningSteamApps.TryGetValue(app.AppId, out var runInfoState);
                if (runInfoState == null)
                {
                    RunOrStop(app);
                    SteamConnectService.Current.RuningSteamApps.TryAdd(app.AppId, app);
                }
                else
                {
                    RunOrStop(runInfoState);
                    app.Process = runInfoState.Process;
                }

                ChangeRunTxt();
                Toast.Show(AppResources.GameList_OperationSuccess);
            }
            else
            {
                var result = MessageBoxCompat.ShowAsync(@AppResources.GameList_SteamNotRuning, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OK);
            }
        }
        public void RunOrStop(SteamApp app)
        {
            if (app.Process == null)
            {
                app.StartSteamAppProcess();
            }
            else
            {
                app.Process.Kill();
                app.Process = null;
            }
        }
        public void Refresh_Click()
        {
            //IdleGameList.Clear();
            var list = new ObservableCollection<SteamApp>();
            if (GameLibrarySettings.AFKAppList.Value != null)
                foreach (var item in GameLibrarySettings.AFKAppList.Value)
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
                                appInfo.Process.Exited += (object? _, EventArgs _) =>
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
                        SteamConnectService.Current.RuningSteamApps.TryGetValue(item.Key, out var runState);
                        runState?.Process?.Kill();
                        list.Add(new SteamApp
                        {
                            AppId = item.Key,
                            Name = item.Value
                        });
                    }
                }
            IdleGameList = list;
            var allCount = IdleGameList.Count;
            if (allCount == 0)
                IsIdleAppEmpty = true;
            else
                IsIdleAppEmpty = false;


            ChangeRunTxt(true);
            if (init)
                Toast.Show(AppResources.GameList_OperationSuccess);
            else
                init = true;
        }

    }
}