using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        private readonly SteamApp? _App;

        public EditAppInfoWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_EditAppInfo;
        }

        public EditAppInfoWindowViewModel(SteamApp app) : this()
        {
            _App = app;
        }




    }
}