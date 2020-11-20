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
    public class DialogWindowViewModel : MainWindowViewModelBase
    {
        public DialogWindowViewModel() : base()
        {
            this.Title = string.Format(Resources.AchievementManager, Resources.WinTitle);
        }

    }
}
