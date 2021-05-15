using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Threading.Tasks;

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
        public void onExited(object? sender, EventArgs e) { 
        
        }
        public void Refresh_Click()
        {
            var list = new ObservableCollection<SteamApp>();
            if (GameLibrarySettings.AFKAppList.Value != null)
                foreach (var item in GameLibrarySettings.AFKAppList.Value)
                {
                    var appInfo =SteamConnectService.Current.SteamApps.Items.FirstOrDefault(x => x.AppId == item.Key);
                    if (appInfo != null) {
                        var runState = SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.Key);
                        if (runState != null && runState.Process != null) {

                            appInfo.Process = runState.Process;
                            appInfo.Process.Exited += new EventHandler(onExited);
                        }
                        list.Add(appInfo);
                    }
                }
            IdleGameList = new ObservableCollection<SteamApp>(list);
            //if (SteamApps.Items.Any_Nullable() && GameLibrarySettings.AFKAppList.Value?.Count > 0)
            //{
            //    var apps = GameLibrarySettings.AFKAppList.Value!.Select(x => x.Key);
            //    foreach (var item in apps)
            //    {
            //        var appInfo = SteamApps.Items.FirstOrDefault(x => x.AppId == item);
            //        if (appInfo != null && RuningSteamApps.FirstOrDefault(x => x.AppId == item) == null)
            //            RuningSteamApps.Add(appInfo);
            //    }
            //    var t = new Task(() =>
            //    {
            //        foreach (var item in RuningSteamApps)
            //        {
            //            if (item.Process == null)
            //                item.Process = Process.Start(AppHelper.ProgramPath, "-clt app -id -silence " + item.AppId.ToString(CultureInfo.InvariantCulture));
            //        }
            //    });
            //    t.Start();
            //}
        }

    }
}