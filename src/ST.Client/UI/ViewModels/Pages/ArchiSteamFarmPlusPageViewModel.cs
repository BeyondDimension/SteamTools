using ArchiSteamFarm;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class ArchiSteamFarmPlusPageViewModel
    {
        protected readonly IArchiSteamFarmService asfSerivce = IArchiSteamFarmService.Instance;

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel);

            ASFService.Current.SteamBotsSourceList
                      .Connect()
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Sort(SortExpressionComparer<Bot>.Descending(x => x.BotName))
                      .Bind(out _SteamBots)
                      .Subscribe();
        }

        public string? IPCUrl => asfSerivce.GetIPCUrl();

        /// <summary>
        /// ASF bots
        /// </summary>
        private ReadOnlyObservableCollection<Bot> _SteamBots;
        public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

        public void RunOrStopASF()
        {
            if (!ASFService.Current.IsASFRuning)
            {
                ASFService.Current.InitASF();
            }
            else
            {
                ASFService.Current.StopASF();
            }
        }

        public void ShowAddBotWindow()
        {
            IWindowManager.Instance.Show(CustomWindow.ASF_AddBot, resizeMode: ResizeMode.CanResize);
        }

        public async void PauseOrResumeBotFarming(Bot bot)
        {
            (bool success, string message) result;

            if (bot.CardsFarmer.Paused)
            {
                result = await bot.Actions.Pause(true).ConfigureAwait(false);
            }
            else
            {
                result = bot.Actions.Resume();
            }

            Toast.Show(result.success ? result.message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));
        }

        public void OpenFolder(string tag)
        {
            var path = tag switch
            {
                "asf" => ASFPathHelper.AppDataDirectory,
                "config" => Path.Combine(ASFPathHelper.AppDataDirectory, SharedInfo.ConfigDirectory),
                "plugin" => Path.Combine(ASFPathHelper.AppDataDirectory, SharedInfo.PluginsDirectory),
                "www" => ASFPathHelper.WebsiteDirectory,
                IApplication.LogDirName => Path.Combine(IApplication.LogDirPath, "ASF"),
                _ => ASFPathHelper.AppDataDirectory,
            };

            IPlatformService.Instance.OpenFolder(path);
        }

        public void OpenBrowser(string tag)
        {
            var url = tag switch
            {
                "Repo" => "https://github.com/JustArchiNET/ArchiSteamFarm",
                "Wiki" => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
                "ConfigGenerator" => "https://justarchinet.github.io/ASF-WebConfigGenerator/",
                "WebConfig" => IPCUrl + "/asf-config",
                "WebAddBot" => IPCUrl + "/asf-config",
                _ => IPCUrl,
            };

            Browser2.Open(url);
        }
    }
}
