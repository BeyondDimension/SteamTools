using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using SteamTools.Win32;
using MetroRadiance.Interop.Win32;
using System.Diagnostics;
using SteamTool.Core.Common;
using System.Runtime.InteropServices;
using SteamTools.Services;
using SteamTool.Steam.Service.Web;
using SteamTool.Core;
using SteamTool.Steam.Service;
using SteamTool.Model;
using Newtonsoft.Json;
using SteamTools.Models.Settings;

namespace SteamTools.ViewModels
{
    public class SettingsGeneralViewModel : Livet.ViewModel
    {
        private SteamworksWebApiService SteamworksWebApi => SteamService.Instance.Get<SteamworksWebApiService>();
        private SteamToolService SteamTool => SteamToolCore.Instance.Get<SteamToolService>();

        private bool IsRefreshGameListCache;

        public void RefreshGameListCache()
        {
            if (SteamConnectService.Current.IsConnectToSteam)
            {
                if (IsRefreshGameListCache)
                {
                    StatusService.Current.Notify("正在下在Steam游戏数据中...");
                    return;
                }
                StatusService.Current.Notify("刷新Steam游戏数据");
                IsRefreshGameListCache = true;
                Task.Run(async () =>
                {
                    var result = await SteamworksWebApi.GetAllSteamAppsString();
                    if (GeneralSettings.IsSteamAppListLocalCache)
                        SteamTool.UpdateAppListJson(result, Const.APP_LIST_FILE);
                    var apps = JsonConvert.DeserializeObject<SteamApps>(result).AppList.Apps;
                    apps = apps.DistinctBy(d => d.AppId).ToList();
                    //SteamConnectService.Current.SteamApps = apps;
                    SteamConnectService.Current.SteamApps = SteamConnectService.Current.ApiService.OwnsApps(apps);
                }).ContinueWith(s =>
                {
                    StatusService.Current.Notify("刷新Steam游戏数据完成");
                    IsRefreshGameListCache = false;
                    s.Dispose();
                });

            }
            else
            {
                StatusService.Current.Notify("未检测到Steam运行");
            }
        }
    }
}
