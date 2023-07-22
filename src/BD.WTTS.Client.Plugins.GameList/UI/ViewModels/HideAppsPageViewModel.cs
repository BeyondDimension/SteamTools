using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed class HideAppsPageViewModel : ViewModelBase
{
    public ObservableCollection<KeyValuePair<uint, string>> _HideGameList = new();

    public ObservableCollection<KeyValuePair<uint, string>> HideGameList
    {
        get => _HideGameList;
        set => this.RaiseAndSetIfChanged(ref _HideGameList, value);
    }

    public HideAppsPageViewModel()
    {
        GameLibrarySettings.HideGameList.Subscribe(_ => LoadData());

        LoadData();
    }

    public bool _IsHideGameListEmpty;

    public bool IsHideGameListEmpty
    {
        get => _IsHideGameListEmpty;
        set => this.RaiseAndSetIfChanged(ref _IsHideGameListEmpty, value);
    }

    public void LoadData()
    {
        HideGameList = new ObservableCollection<KeyValuePair<uint, string>>(GameLibrarySettings.HideGameList.Value!);
        if (HideGameList.Count == 0)
            IsHideGameListEmpty = true;
        else
            IsHideGameListEmpty = false;
    }

    public void RemoveHideApp_Click(KeyValuePair<uint, string> keyValue)
    {
        if (GameLibrarySettings.HideGameList.ContainsKey(keyValue.Key))
        {
            GameLibrarySettings.HideGameList.Remove(keyValue.Key);
        }
        else
        {
            GameLibrarySettings.HideGameList.Add(keyValue.Key, keyValue.Value);
        }

        //GameLibrarySettings.HideGameList.RaiseValueChanged();
        //LoadData();
        //SteamConnectService.Current.RefreshGamesListAsync();
    }
}
