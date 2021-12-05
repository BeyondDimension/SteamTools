using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditAppInfo;

        SteamApp App { get; }

        public EditAppInfoWindowViewModel(SteamApp app)
        {
            if (app == null)
            {
                this.Close();
                return;
            }
            App = app;
            Title = App.GetIdAndName();
        }
    }
}