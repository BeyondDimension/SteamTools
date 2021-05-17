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
        public IdleAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_EditAppInfo;
            //this.WhenAnyValue(x => x.IdleGameList)
            //   .Subscribe(x => x?.ToObservableChangeSet()
            //   .AutoRefresh(x => x.Process));
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

        public ObservableCollection<SteamApp> _IdleGameList = new();
        public ObservableCollection<SteamApp> IdleGameList
        {
            get => _IdleGameList;
            set => this.RaiseAndSetIfChanged(ref _IdleGameList, value);
        }
        public void DeleteAllButton_Click()
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.GameList_DeleteAll, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    if (GameLibrarySettings.AFKAppList.Value != null)
                    {
                        foreach (var item in GameLibrarySettings.AFKAppList.Value)
                        {
                            var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.Key);
                            if (runState != null)
                            {
                                runState.Process?.Kill();
                                SteamConnectService.Current.RuningSteamApps.Remove(runState);
                            }
                        }
                        IdleGameList.Clear();
                        GameLibrarySettings.AFKAppList.Value.Clear();
                        GameLibrarySettings.AFKAppList.RaiseValueChanged();
                        RunState = false;
                        Toast.Show(AppResources.GameList_DeleteSuccess);
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
                            var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == app.AppId);
                            if (runState != null)
                            {
                                runState.Process?.Kill();
                                SteamConnectService.Current.RuningSteamApps.Remove(runState);
                            }
                            MainThreadDesktop.BeginInvokeOnMainThread(() =>
                            {
                                IdleGameList.Remove(app);
                                RunState = IdleGameList.Count(x => x.Process != null) > 0;
                                GameLibrarySettings.AFKAppList.Value.Remove(app.AppId);
                                GameLibrarySettings.AFKAppList.RaiseValueChanged();
                                Toast.Show(AppResources.GameList_DeleteSuccess);
                            });
                        }

                    }
                }
            });

        }

        public void RunOrStopAllButton_Click()
        {
            if (!RunLoaingState)
            {
                RunLoaingState = true;
                if (RunState)
                {

                    foreach (var item in IdleGameList)
                    {
                        var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.AppId);
                        if (runState == null)
                        {

                            item.Process = Process.Start(AppHelper.ProgramPath, "-clt app -silence -id " + item.AppId.ToString(CultureInfo.InvariantCulture));
                            SteamConnectService.Current.RuningSteamApps.Add(item);
                        }
                        else
                        {
                            if (runState.Process == null)
                                runState.Process = Process.Start(AppHelper.ProgramPath, "-clt app -silence -id " + item.AppId.ToString(CultureInfo.InvariantCulture));
                            item.Process = runState.Process;
                        }
                    }
                }
                else
                {
                    foreach (var item in IdleGameList)
                    {
                        var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.AppId);
                        if (runState != null)
                        {
                            runState.Process?.Kill();
                            SteamConnectService.Current.RuningSteamApps.Remove(runState);
                            item.Process = null;
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
        public void RunStopBtn_Click(SteamApp app)
        {
            var runInfoState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == app.AppId);
            if (runInfoState == null)
            {
                RunOrStop(app);
                SteamConnectService.Current.RuningSteamApps.Add(app);
            }
            else
            {
                RunOrStop(runInfoState);
                app.Process = runInfoState.Process;
            }
            RunState = IdleGameList.Count(x => x.Process != null) > 0;
            Toast.Show(AppResources.GameList_OperationSuccess);
        }
        public void RunOrStop(SteamApp app)
        {
            if (app.Process == null)
            {
                app.Process = Process.Start(AppHelper.ProgramPath, "-clt app -silence -id " + app.AppId.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                app.Process.Kill();
                app.Process = null;
            }
        }
        public void Refresh_Click()
        {
            IdleGameList.Clear();
            var list = new ObservableCollection<SteamApp>();
            if (GameLibrarySettings.AFKAppList.Value != null)
                foreach (var item in GameLibrarySettings.AFKAppList.Value)
                {
                    var appInfo = SteamConnectService.Current.SteamApps.Items.FirstOrDefault(x => x.AppId == item.Key);
                    if (appInfo != null)
                    {
                        var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.Key);
                        if (runState != null && runState.Process != null)
                        {
                            RunState = true;
                            appInfo.Process = runState.Process;
                            appInfo.Process.Exited += (object? item, EventArgs e) =>
                            {
                                SteamConnectService.Current.RuningSteamApps.Remove(runState);
                            };
                            appInfo.Process.EnableRaisingEvents = true;
                        }
                        list.Add(appInfo);
                    }
                }
            IdleGameList = list;
            Toast.Show(AppResources.GameList_OperationSuccess);
        }

    }
}