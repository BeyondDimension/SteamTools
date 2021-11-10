using ArchiSteamFarm;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

        private bool _IsRedeemKeyDialogOpen;
        public bool IsRedeemKeyDialogOpen
        {
            get => _IsRedeemKeyDialogOpen;
            set => this.RaiseAndSetIfChanged(ref _IsRedeemKeyDialogOpen, value);
        }

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

        public async Task<(IReadOnlyDictionary<string, string>? UnusedKeys, IReadOnlyDictionary<string, string>? UsedKeys)> GetUsedAndUnusedKeys(Bot bot)
        {
            return await bot.GetUsedAndUnusedKeys();
        }

        public void RedeemKeyBot(Bot bot, IOrderedDictionary keys)
        {
            var validGamesToRedeemInBackground = Bot.ValidateGamesToRedeemInBackground(keys);

            if (validGamesToRedeemInBackground.Count == 0)
            {
                Toast.Show(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(validGamesToRedeemInBackground)));
                return;
            }

            bot.AddGamesToRedeemInBackground(validGamesToRedeemInBackground);

            Toast.Show("已将" + validGamesToRedeemInBackground.Count + "个Key添加到激活队列");
            //var result = await bot.Actions.RedeemKey(keys);
            //if (result != null)
            //{
            //    if (result.Result == SteamKit2.EResult.OK)
            //    {

            //    }
            //}
        }

        public bool ResetbotRedeemedKeysRecord(Bot bot)
        {
            return bot.DeleteRedeemedKeysFiles();
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
