using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Properties;

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

        
        public ObservableCollection<KeyValuePair<uint, string>> _IdleGameList = new();
        public ObservableCollection<KeyValuePair<uint, string>> IdleGameList
        {
            get => _IdleGameList;
            set => this.RaiseAndSetIfChanged(ref _IdleGameList, value);
        }

        public void Refresh_Click()
        {
            IdleGameList = new ObservableCollection<KeyValuePair<uint, string>>(GameLibrarySettings.AFKAppList.Value!);
        }

    }
}