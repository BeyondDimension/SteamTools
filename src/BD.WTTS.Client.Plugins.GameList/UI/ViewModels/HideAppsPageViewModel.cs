using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed class HideAppsPageViewModel : ViewModelBase
{
    [Reactive]
    public ObservableCollection<KeyValuePair<uint, string>> HideGameList { get; set; } = new();

    [Reactive]
    public bool IsHideGameListEmpty { get; set; }

    public ICommand RemoveHideAppCommand { get; }

    public HideAppsPageViewModel()
    {
        RemoveHideAppCommand = ReactiveCommand.Create<KeyValuePair<uint, string>>(RemoveHideApp_Click);
        CompositeDisposable.Add(GameLibrarySettings.HideGameList.Subscribe(_ => LoadData()));

        LoadData();
    }

    private void LoadData()
    {
        HideGameList = new ObservableCollection<KeyValuePair<uint, string>>(GameLibrarySettings.HideGameList.Value!);
        if (HideGameList.Count == 0)
            IsHideGameListEmpty = true;
        else
            IsHideGameListEmpty = false;
    }

    private void RemoveHideApp_Click(KeyValuePair<uint, string> keyValue)
    {
        if (GameLibrarySettings.HideGameList.ContainsKey(keyValue.Key))
        {
            GameLibrarySettings.HideGameList.Remove(keyValue.Key);
            HideGameList.Remove(keyValue);
        }

        Task2.InBackground(SteamConnectService.Current.RefreshGamesListAsync);
    }
}
