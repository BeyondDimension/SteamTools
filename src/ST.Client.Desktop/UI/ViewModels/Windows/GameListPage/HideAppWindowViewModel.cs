using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class HideAppWindowViewModel : WindowViewModel
    {
        public HideAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_HideGameManger;

            GameLibrarySettings.HideGameList.Subscribe(_ => Refresh_Click());

            Refresh_Click();
        }
        public bool _IsHideGameListEmpty;
        public bool IsHideGameListEmpty
        {
            get => _IsHideGameListEmpty;
            set => this.RaiseAndSetIfChanged(ref _IsHideGameListEmpty, value);
        }

        public ObservableCollection<KeyValuePair<uint, string>> _HideGameList = new();
        public ObservableCollection<KeyValuePair<uint, string>> HideGameList
        {
            get => _HideGameList;
            set => this.RaiseAndSetIfChanged(ref _HideGameList, value);
        }

        //private string? _SearchText;
        //public string? SearchText
        //{
        //    get => _SearchText;
        //    set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        //}

        public void Refresh_Click()
        {
            HideGameList = new ObservableCollection<KeyValuePair<uint, string>>(GameLibrarySettings.HideGameList.Value!);
            if (HideGameList.Count == 0)
                IsHideGameListEmpty = true;
            else
                IsHideGameListEmpty = false;

        }

        public void ChangeCheck(KeyValuePair<uint, string> keyValue)
        {
            if (GameLibrarySettings.HideGameList.Value!.ContainsKey(keyValue.Key))
            {
                GameLibrarySettings.HideGameList.Value!.Remove(keyValue.Key);
            }
            else
            {
                GameLibrarySettings.HideGameList.Value!.Add(keyValue.Key, keyValue.Value);
            }
        }

        public void SaveChange_Click()
        {
            GameLibrarySettings.HideGameList.RaiseValueChanged();
            Refresh_Click();
            SteamConnectService.Current.RefreshGamesList();
        }
    }
}