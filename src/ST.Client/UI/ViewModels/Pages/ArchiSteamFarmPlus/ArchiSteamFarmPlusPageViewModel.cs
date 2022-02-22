using ArchiSteamFarm;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
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
                FilePickerFileType? fileTypes;
                if (IApplication.IsDesktopPlatform)
                {
                    fileTypes = new ValueTuple<string, string[]>[] {
                        ("Json Files", new[] { FileEx.JSON, }),
                        //("All Files", new[] { "*", }),
                    };
                }
                else if (OperatingSystem2.IsAndroid)
                {
                    fileTypes = new[] { MediaTypeNames.JSON };
                }
                else
                {
                    fileTypes = null;
                }
                await PickMultipleAsync(ASFService.Current.ImportBotFiles, fileTypes);
            });

            SelectGlobalFiles = ReactiveCommand.CreateFromTask(async () =>
            {
                FilePickerFileType? fileTypes;
                if (IApplication.IsDesktopPlatform)
                {
                    fileTypes = new ValueTuple<string, string[]>[] {
                        ("Json Files", new[] { FileEx.JSON, }),
                        //("All Files", new[] { "*", }),
                    };
                }
                else if (OperatingSystem2.IsAndroid)
                {
                    fileTypes = new[] { MediaTypeNames.JSON };
                }
                else
                {
                    fileTypes = null;
                }
                await PickAsync(ASFService.Current.ImportGlobalFiles, fileTypes);
            });

            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                new MenuItemCustomName(AppResources.ASF_Start, AppResources.ASF_Start)
                    {
                        Command = ReactiveCommand.Create(RunOrStopASF),
                    },
                new MenuItemCustomName(AppResources.ASF_Stop, AppResources.ASF_Stop)
                    {
                        Command = ReactiveCommand.Create(RunOrStopASF),
                    },
                new MenuItemSeparator(),
                new MenuItemCustomName(AppResources.ASF_OpenWebUIConsole, AppResources.ASF_OpenWebUIConsole)
                    {
                        Command = ReactiveCommand.Create(() =>
                        {
                            OpenBrowser(null);
                        }),
                    },
            };

            ASFService.Current.WhenAnyValue(x => x.IsASFRuning)
                .Subscribe(x =>
                {
                    MenuItems[0].IsEnabled = !x;
                    MenuItems[1].IsEnabled = x;
                });


            ASFService.Current.SteamBotsSourceList
                      .Connect()
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Sort(SortExpressionComparer<Bot>.Descending(x => x.BotName))
                      .Bind(out _SteamBots)
                      .Subscribe();

            //if (IApplication.IsDesktopPlatform)
            //{
            //    //ConsoleSelectFont = R.Fonts.FirstOrDefault(x => x.Value == ASFSettings.ConsoleFontName.Value);
            //    //this.WhenValueChanged(x => x.ConsoleSelectFont, false)
            //    //      .Subscribe(x => ASFSettings.ConsoleFontName.Value = x.Value);
            //}
        }

        public string IPCUrl => asfSerivce.GetIPCUrl();

        /// <summary>
        /// ASF bots
        /// </summary>
        private readonly ReadOnlyObservableCollection<Bot> _SteamBots;
        public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

        public ICommand SelectBotFiles { get; }

        public ICommand SelectGlobalFiles { get; }

        private bool _IsRedeemKeyDialogOpen;
        public bool IsRedeemKeyDialogOpen
        {
            get => _IsRedeemKeyDialogOpen;
            set => this.RaiseAndSetIfChanged(ref _IsRedeemKeyDialogOpen, value);
        }

        public static void StartOrStopASF(bool? startOrStop = null) => Task.Run(async () =>
        {
            var s = ASFService.Current;
            if (!s.IsASFRuning)
            {
                if (!startOrStop.HasValue || startOrStop.Value)
                    await s.InitASF();
            }
            else
            {
                if (!startOrStop.HasValue || !startOrStop.Value)
                    await s.StopASF();
            }
        });

        public void RunOrStopASF() => StartOrStopASF();

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

            ASFService.Current.SteamBotsSourceList.AddOrUpdate(bot);

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

            if (!result.success)
                Toast.Show(string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, result.message));

            ASFService.Current.SteamBotsSourceList.AddOrUpdate(bot);
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

            Toast.Show(string.Format(AppResources.ASF_RedeemKeyBotSuccessTip, validGamesToRedeemInBackground.Count));
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

        public void EditBotFile(Bot bot)
        {
            var filePath = Bot.GetFilePath(bot.BotName, Bot.EFileType.Config);
            IPlatformService.Instance.OpenFileByTextReader(filePath);
        }

        public async void DeleteBot(Bot bot)
        {
            var s = await MessageBox.ShowAsync(AppResources.ASF_DeleteBotTip, button: MessageBox.Button.OKCancel);
            if (s == MessageBox.Result.OK)
            {
                var result = await bot.DeleteAllRelatedFiles();
                if (result)
                {
                    ASFService.Current.SteamBotsSourceList.Remove(bot);
                    Toast.Show(AppResources.GameList_DeleteSuccess);
                }
            }
        }

        public void OpenFolder(string tag)
        {
            if (!Enum.TryParse<ASFPathFolder>(tag, true, out var folderASFPath)) return;
            var folderASFPathValue = IArchiSteamFarmService.GetFolderPath(folderASFPath);
            IPlatformService.Instance.OpenFolder(folderASFPathValue);
        }

        void OpenBrowserCore(ActionItem tag)
        {
            var url = tag switch
            {
                ActionItem.Repo => "https://github.com/JustArchiNET/ArchiSteamFarm",
                ActionItem.Wiki => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
                ActionItem.ConfigGenerator => "https://justarchinet.github.io/ASF-WebConfigGenerator",
                ActionItem.WebConfig => IPCUrl + "/asf-config",
                ActionItem.WebAddBot => IPCUrl + "/bot/new",
                _ => IPCUrl,
            };

            if (url.StartsWith(IPCUrl))
            {
                string? ipc_error = null;
                if (!ASFService.Current.IsASFRuning)
                {
                    ipc_error = AppResources.ASF_RequirRunASF;
                }
                else if (!ArchiSteamFarm.IPC.ArchiKestrel.IsReady)
                {
                    // IPC 未启动前无法获取正确的端口号，会导致拼接的 URL 值不正确
                    ipc_error = AppResources.ASF_IPCIsReadyFalse;
                }
                if (ipc_error != null)
                {
                    Toast.Show(ipc_error);
                    return;
                }
            }

            Browser2.Open(url);
        }

        public void OpenBrowser(string? tag)
        {
            var tag_ = Enum.TryParse<ActionItem>(tag, out var @enum) ? @enum : default;
            OpenBrowserCore(tag_);
        }

        //KeyValuePair<string, string> _ConsoleSelectFont;
        //public KeyValuePair<string, string> ConsoleSelectFont
        //{
        //    get => _ConsoleSelectFont;
        //    set => this.RaiseAndSetIfChanged(ref _ConsoleSelectFont, value);
        //}

        public async void SetEncryptionKey() => await asfSerivce.SetEncryptionKeyAsync();
    }
}
