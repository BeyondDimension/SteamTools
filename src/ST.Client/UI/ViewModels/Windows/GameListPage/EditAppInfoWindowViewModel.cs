using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditAppInfo;

        readonly SteamApp? _App;

        public EditAppInfoWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
        }

        public EditAppInfoWindowViewModel(SteamApp app) : this()
        {
            _App = app;
        }
    }
}