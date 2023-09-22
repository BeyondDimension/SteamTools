using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed class EditAppsPageViewModel : ViewModelBase
{
    public static string DisplayName => Strings.GameList_EditedAppsSaveManger;

    readonly ReadOnlyObservableCollection<SteamApp> _SteamEditedApps;

    public ReadOnlyObservableCollection<SteamApp> SteamEditedApps => _SteamEditedApps;

    public bool IsSteamEditedAppsEmpty => !SteamEditedApps.Any_Nullable();

    public ICommand EditAppInfoClickCommand { get; }

    public EditAppsPageViewModel()
    {
        SteamConnectService.Current.SteamApps
          .Connect()
          .Filter(x => x.IsEdited)
          .ObserveOn(RxApp.MainThreadScheduler)
          .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId))
          .Bind(out _SteamEditedApps)
          .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamEditedAppsEmpty)));

        EditAppInfoClickCommand = ReactiveCommand.Create<SteamApp>(GameListPageViewModel.EditAppInfoClick);

        LoadSteamEditedApps();
    }

    public void LoadSteamEditedApps()
    {
        SteamConnectService.Current.SteamApps.Refresh();
    }

    public async Task SaveSteamEditedApps()
    {
        var stmService = ISteamService.Instance;
        if (await stmService.SaveAppInfosToSteam())
        {
            if (await MessageBox.ShowAsync(Strings.SaveEditedAppInfo_RestartSteamTip, AssemblyInfo.Trademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
            {
                await stmService.TryKillSteamProcess();
                stmService.StartSteamWithParameter();
            }
            Toast.Show(ToastIcon.Success, Strings.SaveEditedAppInfo_SaveToSteamSuccess);
        }
        else
        {
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SaveToSteamFailed);
        }
    }

    //public async Task ClearSteamEditedApps()
    //{
    //    if (await MessageBox.ShowAsync("确定要重置所有的已修改数据吗？(该操作不可还原)", AssemblyInfo.Trademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
    //    {

    //    }
    //}
}
