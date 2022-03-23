using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditAppInfo;

        private SteamApp? _App;
        public SteamApp? App
        {
            get => _App;
            set => this.RaiseAndSetIfChanged(ref _App, value);
        }

        public EditAppInfoWindowViewModel(SteamApp app)
        {
            if (app == null)
            {
                this.Close();
                return;
            }
            App = app;
            Title = App.DisplayName;
        }

        public void AddLaunchItem()
        {
            if (App.LaunchItems == null)
            {
                App.LaunchItems = new List<SteamAppLaunchItem> { new() };
            }
            else
            {
                App.LaunchItems.Add(new());
            }

            App.RaisePropertyChanged(nameof(App.LaunchItems));
        }

        public void DeleteLaunchItem(SteamAppLaunchItem item)
        {
            if (App.LaunchItems != null)
            {
                App.LaunchItems.Remove(item);
            }

            App.RaisePropertyChanged(nameof(App.LaunchItems));
        }

        public void SaveEditAppInfo()
        {

        }

        public void CancelEditAppInfo()
        {
            this.Close();
        }
    }
}