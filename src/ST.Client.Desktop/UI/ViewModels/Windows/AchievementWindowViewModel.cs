using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class AchievementWindowViewModel : WindowViewModel
    {
        public AchievementWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LocalAuth_AuthData;
        }

        public AchievementWindowViewModel(int appid)
        {

        }
    }
}