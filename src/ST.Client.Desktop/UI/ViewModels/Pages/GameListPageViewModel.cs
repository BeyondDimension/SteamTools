using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }

        private bool _IsAppInfoOpen;
        public bool IsAppInfoOpen
        {
            get => _IsAppInfoOpen;
            set => this.RaiseAndSetIfChanged(ref _IsAppInfoOpen, value);
        }

        private SteamApp? _SelectApp;
        public SteamApp? SelectApp
        {
            get => _SelectApp;
            set => this.RaiseAndSetIfChanged(ref _SelectApp, value);
        }

        private IReadOnlyCollection<SteamApp>? _SteamApps;
        public IReadOnlyCollection<SteamApp>? SteamApps
        {
            get => _SteamApps;
            set
            {
                if (_SteamApps != value)
                {
                    _SteamApps = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        //private IReadOnlyCollection<SteamApp>? _AllSteamApps;

        internal async override void Initialize()
        {
            SteamApps = await ISteamService.Instance.GetAppInfos();
#if DEBUG
            if (BuildConfig.IsAigioPC && BuildConfig.IsDebuggerAttached)
            {
                return;
            }
#endif

            if (SteamApps.Any_Nullable())
            {
                Parallel.ForEach(SteamApps, async app =>
                {
                    await ISteamService.Instance.LoadAppImageAsync(app);
                    //app.LibraryLogoStream = await IHttpService.Instance.GetImageAsync(app.LibraryLogoUrl, ImageChannelType.SteamGames);
                    //app.LibraryHeaderStream = await IHttpService.Instance.GetImageAsync(app.LibraryHeaderUrl, ImageChannelType.SteamGames);
                    //app.LibraryNameStream = await IHttpService.Instance.GetImageAsync(app.LibraryNameUrl, ImageChannelType.SteamGames);
                    //app.HeaderLogoStream = await IHttpService.Instance.GetImageAsync(app.HeaderLogoUrl, ImageChannelType.SteamGames);
                });
            }
        }

        public void AppClick(SteamApp app)
        {
            IsAppInfoOpen = true;
            SelectApp = app;
        }
    }
}