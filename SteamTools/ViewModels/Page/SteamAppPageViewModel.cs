using SteamTool.Core;
using SteamTool.Model;
using SteamTool.Steam.Service;
using SteamTool.Steam.Service.Local;
using SteamTool.Steam.Service.Web;
using SteamTools.Models;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reactive;
using System.Reactive.Subjects;
using MetroTrilithon.Mvvm;
using System.Reactive.Linq;
using System.Diagnostics;
using SteamTool.Core.Common;
using SteamTools.Properties;
using SteamTools.Views;
using System.Globalization;
using System.IO;
using System.Reflection;
using SteamTools.Models.Settings;

namespace SteamTools.ViewModels
{
    public class SteamAppPageViewModel : TabItemViewModel
    {
        private readonly Subject<Unit> updateSource = new Subject<Unit>();
        private SteamworksWebApiService SteamworksWebApi => SteamService.Instance.Get<SteamworksWebApiService>();
        private SteamToolService SteamTool => SteamToolCore.Instance.Get<SteamToolService>();

        public override string Name
        {
            get { return Properties.Resources.GameList; }
            protected set { throw new NotImplementedException(); }
        }

        #region IsReloading 変更通知

        private bool _IsReloading;

        public bool IsReloading
        {
            get { return this._IsReloading; }
            set
            {
                if (this._IsReloading != value)
                {
                    this._IsReloading = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Games 变更通知
        private IReadOnlyCollection<SteamApp> _Games;
        public IReadOnlyCollection<SteamApp> Games
        {
            get { return this._Games; }
            set
            {
                if (this._Games != value)
                {
                    this._Games = value;
                    InstalledCount = value.Count(s => s.IsInstalled);
                    this.RaisePropertyChanged();
                }
            }
        }

        private int _InstalledCount;
        public int InstalledCount
        {
            get { return this._InstalledCount; }
            set
            {
                if (this._InstalledCount != value)
                {
                    this._InstalledCount = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public Array AppTypes => typeof(SteamAppTypeEnum).GetEnumValues();
        public SteamAppSortWorker SortWorker { get; }
        public SteamAppNameFilter AppNameFilter { get; }
        public SteamAppTypeFilter AppTypeFilter { get; }


        public SteamAppPageViewModel()
        {
            SortWorker = new SteamAppSortWorker();
            AppNameFilter = new SteamAppNameFilter(this.Update);
            AppTypeFilter = new SteamAppTypeFilter(this.Update);
        }

        internal override void Initialize()
        {
            StatusService.Current.Notify("加载Steam游戏数据");
            Task.Run(async () =>
           {
               var apps = SteamTool.GetAppListJson(Path.Combine(AppContext.BaseDirectory, Const.APP_LIST_FILE));
               if (apps == null || !apps.Any())
               {
                   var result = await SteamworksWebApi.GetAllSteamAppsString();
                   if (string.IsNullOrEmpty(result))
                   {
                       StatusService.Current.Notify("下载Steam游戏数据失败，请尝试开启社区反代刷新");
                       return;
                   }
                   if (GeneralSettings.IsSteamAppListLocalCache)
                       SteamTool.UpdateAppListJson(result, Path.Combine(AppContext.BaseDirectory, Const.APP_LIST_FILE));
                   apps = JsonConvert.DeserializeObject<SteamApps>(result).AppList.Apps;
               }
               apps = apps.DistinctBy(d => d.AppId).ToList();
                //SteamConnectService.Current.SteamApps = apps;
                SteamConnectService.Current.SteamApps = SteamConnectService.Current.ApiService.OwnsApps(apps);

               this.updateSource
               .Do(_ => this.IsReloading = true)
               .SelectMany(x => this.UpdateAsync())
               .Do(_ => this.IsReloading = false)
               .Subscribe()
               .AddTo(this);

               SteamConnectService.Current.Subscribe(nameof(SteamConnectService.Current.SteamApps), this.Update).AddTo(this);

           }).ContinueWith(s => { 
               Logger.Error(s.Exception); 
               WindowService.Current.ShowDialogWindow(s.Exception.Message, "加载Steam游戏数据失败");
           }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s =>
           {
               StatusService.Current.Notify("加载Steam游戏数据完成");
               SteamConnectService.Current.DisposeSteamClient();
               s.Dispose();
           });
        }

        public void Update()
        {
            this.updateSource.OnNext(Unit.Default);
        }
        private IObservable<Unit> UpdateAsync()
        {
            return Observable.Start(() =>
            {
                var list = SteamConnectService.Current.SteamApps
                .Where(this.AppNameFilter.Predicate)
                .Where(this.AppTypeFilter.Predicate);

                this.Games = this.SortWorker.Sort(list)
                    .Select((x, i) => new SteamApp
                    {
                        Index = i + 1,
                        AppId = x.AppId,
                        Name = x.Name,
                        Type = x.Type,
                        IsInstalled = x.IsInstalled,
                    }).ToList();
            });
        }

        public void Sort(SortableColumn column)
        {
            this.SortWorker.SetFirst(column);
            this.Update();
        }

        public void LogoImage_Click(uint appid)
        {
            Process.Start(string.Format(Const.STORE_APP_URL, appid.ToString()));
        }

        public void UnlockAchievement_Click(SteamApp app)
        {
            switch (app.Type)
            {
                case SteamAppTypeEnum.Application:
                case SteamAppTypeEnum.Game:
                    //var achievement = new AchievementWindowViewModel();
                    //WindowService.Current.MainWindow.Transition(achievement, typeof(AchievementWindow));
                    var nApp = app.Clone();
                    //nApp.Process = Process.Start($"{ProductInfo.Title}.exe", app.AppId.ToString(CultureInfo.InvariantCulture));
                    nApp.Process = Process.Start(Environment.GetCommandLineArgs()[0], "-app " + app.AppId.ToString(CultureInfo.InvariantCulture));
                    SteamConnectService.Current.RuningSteamApps.Add(nApp);
                    break;
                default:
                    StatusService.Current.Notify(Resources.Unsupported_Operation);
                    break;
            }
        }

        public void InstallSteamApp_Click(SteamApp app)
        {
            switch (app.Type)
            {
                case SteamAppTypeEnum.Media:
                    Process.Start(string.Format(Const.STEAM_MEDIA_URL, app.AppId.ToString()));
                    break;
                default:
                    if (app.IsInstalled)
                    {
                        TaskbarService.Current.Notify($"正在启动《{app.Name}》", Resources.CurrentAppInstalled);
                        Process.Start(string.Format(Const.STEAM_RUNGAME_URL, app.AppId.ToString()));
                        return;
                    }
                    Process.Start(string.Format(Const.STEAM_INSTALL_URL, app.AppId.ToString()));
                    break;
            }
        }
    }
}
