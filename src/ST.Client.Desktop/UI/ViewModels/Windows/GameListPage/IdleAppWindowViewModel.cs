using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            Refresh_Click();
        }

        private bool _RunState;
        public bool RunState
        {
            get => _RunState;
            set => this.RaiseAndSetIfChanged(ref _RunState, value);
        }

        public ObservableCollection<SteamApp> _IdleGameList = new();
        public ObservableCollection<SteamApp> IdleGameList
        {
            get => _IdleGameList;
            set => this.RaiseAndSetIfChanged(ref _IdleGameList, value);
        }
        public void StopOrRunItem(SteamApp item)
        {
            SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.AppId);


        }
        public void DeleteAllButton_Click()
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.ScriptShop_NoLogin, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
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
                        GameLibrarySettings.AFKAppList.Value = new Dictionary<uint, string?>();
                        GameLibrarySettings.AFKAppList.RaiseValueChanged();

                    } 
                }
            });
        }
        public void DeleteButton_Click(SteamApp app)
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.ScriptShop_NoLogin, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    if (GameLibrarySettings.AFKAppList.Value != null)
                    {
                        var item = GameLibrarySettings.AFKAppList.Value.ContainsKey(app.AppId);
                        if (item)
                        {
                            GameLibrarySettings.AFKAppList.Value.Remove(app.AppId);
                            GameLibrarySettings.AFKAppList.RaiseValueChanged();
                            var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == app.AppId);
                            if (runState != null)
                            {
                                runState.Process?.Kill();
                                SteamConnectService.Current.RuningSteamApps.Remove(runState);
                            }

                        }

                    }
                }
            });

        }
        public void RunStopBtn_Click(SteamApp app)
        {


        }
        public void Refresh_Click()
        {
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

                            appInfo.Process = runState.Process;
                        }
                        list.Add(appInfo);
                    }
                }
            IdleGameList = list;
        }

    }
}