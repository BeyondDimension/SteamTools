using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class SaveEditedAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditedAppsSaveManger;

        public SaveEditedAppInfoWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            SteamConnectService.Current.SteamApps
              .Connect()
              .Filter(x => x.IsEdited)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId))
              .Bind(out _SteamEditedApps)
              .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamEditedAppsEmpty)));

            LoadSteamEditedApps();
        }

        readonly ReadOnlyObservableCollection<SteamApp> _SteamEditedApps;

        public ReadOnlyObservableCollection<SteamApp> SteamEditedApps => _SteamEditedApps;

        public bool IsSteamEditedAppsEmpty => !SteamEditedApps.Any_Nullable();

        public void LoadSteamEditedApps()
        {
            SteamConnectService.Current.SteamApps.Refresh();
        }

        public async Task SaveSteamEditedApps()
        {
            var stmService = ISteamService.Instance;
            if (await stmService.SaveAppInfosToSteam())
            {
                if (await MessageBox.ShowAsync(AppResources.SaveEditedAppInfo_RestartSteamTip, ThisAssembly.AssemblyTrademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
                {
                    await stmService.ShutdownSteamAsync();
                    stmService.StartSteamWithParameter();
                }
                Toast.Show(AppResources.SaveEditedAppInfo_SaveToSteamSuccess);
            }
            else
            {
                Toast.Show(AppResources.SaveEditedAppInfo_SaveToSteamFailed);
            }
        }

        //public async Task ClearSteamEditedApps()
        //{
        //    if (await MessageBox.ShowAsync("确定要重置所有的已修改数据吗？(该操作不可还原)", ThisAssembly.AssemblyTrademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
        //    {

        //    }
        //}

        public static void EditSteamApp(SteamApp app) => GameListPageViewModel.EditAppInfoClick(app);

        public static void NavAppToSteamView(SteamApp app) => GameListPageViewModel.NavAppToSteamView(app);

        public static void OpenFolder(SteamApp app) => GameListPageViewModel.OpenFolder(app);

        public static void OpenAppStoreUrl(SteamApp app) => GameListPageViewModel.OpenAppStoreUrl(app);

        public static void OpenSteamDBUrl(SteamApp app) => GameListPageViewModel.OpenSteamDBUrl(app);

        public static void OpenSteamCardUrl(SteamApp app) => GameListPageViewModel.OpenSteamCardUrl(app);
    }
}