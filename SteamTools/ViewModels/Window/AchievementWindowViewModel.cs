using MetroTrilithon.Mvvm;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class AchievementWindowViewModel : MainWindowViewModelBase
    {
        public AchievementWindowViewModel(bool isMainWindow) : base(isMainWindow)
        {
            this.Title = string.Format(Resources.AchievementManager, Resources.WinTitle);
        }

        public void Initialize(int appid)
        {
            base.Initialize();
            //await Task.Yield();
            SteamConnectService.Current.Initialize(appid);
            if (!SteamConnectService.Current.IsConnectToSteam)
            {
                App.Current.Shutdown();
            }
        }
    }
}
