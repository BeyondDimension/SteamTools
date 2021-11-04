using ArchiSteamFarm;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using static System.Application.FilePicker2;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class ArchiSteamFarmPlusPageViewModel
    {
        protected readonly IArchiSteamFarmService asfSerivce = IArchiSteamFarmService.Instance;

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel);

            SelectBotFiles = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = new FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("Json Files", new[] { SharedInfo.JsonConfigExtension, }),
                    ("All Files", new[] { "*" }),
                });
                await PickMultipleAsync(ASFService.Current.ImportBotFiles, fileTypes);
            });

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
        private readonly ReadOnlyObservableCollection<Bot> _SteamBots;
        public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

        public ICommand SelectBotFiles { get; }

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
                result = bot.Actions.Resume();
            }
            else
            {
                result = await bot.Actions.Pause(true).ConfigureAwait(false);
            }

            Toast.Show(result.success ? result.message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));
        }

        public void EnableOrDisableBot(Bot bot)
        {
            (bool success, string message) result;

            if (bot.KeepRunning)
            {
                result = bot.Actions.Stop();
            }
            else
            {
                result = bot.Actions.Start();
            }

            Toast.Show(result.success ? result.message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));
        }

        public async void RedeemKeyBot(Bot bot)
        {
            var result = await bot.Actions.RedeemKey("");

        }

        public void GoToBotSettings(Bot bot)
        {
            Browser2.Open(IPCUrl + "/bot/" + bot.BotName + "/config");
        }

        public void OpenFolder(string tag)
        {
            static string GetFolderPath(string path)
            {
                IOPath.DirCreateByNotExists(path);
                return path;
            }

            var path = tag switch
            {
                "asf" => ASFPathHelper.AppDataDirectory,
                "config" => GetFolderPath(SharedInfo.ConfigDirectory),
                "plugin" => GetFolderPath(SharedInfo.PluginsDirectory),
                "www" => ASFPathHelper.WebsiteDirectory,
                IApplication.LogDirName => IApplication.LogDirPathASF,
                _ => ASFPathHelper.AppDataDirectory,
            };

            IPlatformService.Instance.OpenFolder(path);
        }

        public void OpenBrowser(string? tag)
        {
            var url = tag switch
            {
                "Repo" => "https://github.com/JustArchiNET/ArchiSteamFarm",
                "Wiki" => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
                "ConfigGenerator" => "https://justarchinet.github.io/ASF-WebConfigGenerator/",
                "WebConfig" => IPCUrl + "/asf-config",
                "WebAddBot" => IPCUrl + "/bot/new",
                _ => IPCUrl,
            };

            if (url?.Contains(IPCUrl) == true && !ASFService.Current.IsASFRuning)
            {
                Toast.Show("请先运行ASF功能");
                return;
            }

            Browser2.Open(url);
        }

    }
}
