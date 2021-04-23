using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Diagnostics;
using System.Properties;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class AchievementWindowViewModel : WindowViewModel
    {
        private int AppId { get; }

        #region 成就列表
        private IList<AchievementInfo> _BackAchievements;
        private IList<AchievementInfo> _Achievements;
        public IList<AchievementInfo> Achievements
        {
            get { return _Achievements; }
            set
            {
                if (this._Achievements != value)
                {
                    this._Achievements = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region 统计列表
        private IList<StatInfo> _Statistics;
        public IList<StatInfo> Statistics
        {
            get { return _Statistics; }
            set
            {
                if (this._Statistics != value)
                {
                    this._Statistics = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public AchievementWindowViewModel(int appid)
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.AchievementManage;

            SteamConnectService.Current.Initialize(appid);
            
            if (SteamConnectService.Current.IsConnectToSteam == false)
            {
                var result = MessageBoxCompat.ShowAsync("与Steam建立连接失败，可能是该游戏没有成就，或者你没有该游戏。", Title, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                {
                    EnforceClose();
                });
            }

            Achievements = new List<AchievementInfo>();
            Statistics = new List<StatInfo>();
            AppId = appid;
            string name = ISteamworksLocalApiService.Instance.GetAppData((uint)appid, "name");
            name ??= appid.ToString();
            Title = ThisAssembly.AssemblyTrademark + " | " + name;
            ToastService.Current.Set("加载成就和统计数据...");

        }

        private void EnforceClose()
        {
            Process.GetCurrentProcess().Kill();
        }


    }
}