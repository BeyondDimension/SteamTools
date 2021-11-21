using ArchiSteamFarm;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.NLog.Targets;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService
    {
        readonly IHttpService httpService;

        public ArchiSteamFarmServiceImpl(IHttpService http)
        {
            httpService = http;
        }

        public event Action<string>? OnConsoleWirteLine;

        public TaskCompletionSource<string>? ReadLineTask { get; set; }

        bool _IsReadPasswordLine;
        public bool IsReadPasswordLine
        {
            get => _IsReadPasswordLine;
            set => this.RaiseAndSetIfChanged(ref _IsReadPasswordLine, value);
        }

        public DateTimeOffset? StartTime { get; set; }

        public Version CurrentVersion => SharedInfo.Version;

        private bool isFirstStart = true;
        public async Task Start(string[]? args = null)
        {
            try
            {
                StartTime = DateTimeOffset.Now;
                if (isFirstStart)
                {
                    IArchiSteamFarmService.InitCoreLoggers?.Invoke();

                    // 初始化文件夹
                    var folders = Enum2.GetAll<ASFPathFolder>();
                    Array.ForEach(folders, f => IArchiSteamFarmService.GetFolderPath(f));

                    InitHistoryLogger();

                    ArchiSteamFarm.NLog.Logging.GetUserInputFunc = async (bool isPassword) =>
                    {
                        ReadLineTask = new(TaskCreationOptions.AttachedToParent);
                        IsReadPasswordLine = isPassword;
#if NET6_0_OR_GREATER
                        var result = await ReadLineTask.Task.WaitAsync(TimeSpan.FromSeconds(60));
#else
                        var result = await ReadLineTask.Task;
#endif
                        if (IsReadPasswordLine)
                            IsReadPasswordLine = false;
                        ReadLineTask = null;
                        return result;
                    };

                    await Program.Init(args).ConfigureAwait(false);
                    isFirstStart = false;
                }
                else
                {
                    if (!await Program.InitASF().ConfigureAwait(false))
                    {
                        await Stop().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.Show(ex.Message);
                await Stop().ConfigureAwait(false);
            }
        }

        public async Task Stop()
        {
            StartTime = null;
            ReadLineTask?.TrySetResult("");
            await Program.InitShutdownSequence();
        }

        private void InitHistoryLogger()
        {
            ArchiSteamFarm.NLog.Logging.InitHistoryLogger();

            HistoryTarget? historyTarget = LogManager.Configuration.AllTargets.OfType<HistoryTarget>().FirstOrDefault();

            if (historyTarget != null)
                historyTarget.NewHistoryEntry += (object? sender, HistoryTarget.NewHistoryEntryArgs newHistoryEntryArgs) =>
                {
                    OnConsoleWirteLine?.Invoke(newHistoryEntryArgs.Message);
                };
        }

        /// <summary>
        /// 执行asf指令
        /// </summary>
        /// <param name="command"></param>
        public async Task<string?> ExecuteCommand(string command)
        {
            Bot? targetBot = Bot.Bots?.OrderBy(bot => bot.Key, Bot.BotsComparer).Select(bot => bot.Value).FirstOrDefault();

            ASF.ArchiLogger.LogGenericInfo(command);

            if (targetBot == null)
            {
                ASF.ArchiLogger.LogGenericWarning(Strings.ErrorNoBotsDefined);
                return null;
            }

            ASF.ArchiLogger.LogGenericInfo(Strings.Executing);

            ulong steamOwnerID = ASF.GlobalConfig?.SteamOwnerID ?? GlobalConfig.DefaultSteamOwnerID;

            string? response = await targetBot.Commands.Response(steamOwnerID, command!);

            if (!string.IsNullOrEmpty(response))
                ASF.ArchiLogger.LogGenericInfo(response);
            return response;
        }

        /// <summary>
        /// 获取bot只读集合
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Bot>? GetReadOnlyAllBots()
        {
            var bots = Bot.Bots;
            //if (bots is not null)
            //    foreach (var bot in bots.Values)
            //    {
            //        bot.AvatarUrl = GetAvatarUrl(bot);
            //    }
            return bots;
        }

        /// <summary>
        /// 设置并保存bot
        /// </summary>
        /// <returns></returns>
        public async void SaveBot(Bot bot)
        {
            //var bot = Bot.GetBot(botName);
            string filePath = Bot.GetFilePath(bot.BotName, Bot.EFileType.Config);
            bool result = await BotConfig.Write(filePath, bot.BotConfig).ConfigureAwait(false);
        }

        public GlobalConfig? GetGlobalConfig()
        {
            return ASF.GlobalConfig;
        }

        public async void SaveGlobalConfig(GlobalConfig config)
        {
            string filePath = ASF.GetFilePath(ASF.EFileType.Config);
            bool result = await GlobalConfig.Write(filePath, config).ConfigureAwait(false);
            if (result)
            {
                Toast.Show("SaveGlobalConfig  " + result);
            }
        }

        public string GetIPCUrl()
        {
            var defaultUrl = "http://" + IPAddress.Loopback + ":1242";
            string absoluteConfigDirectory = Path.Combine(ASFPathHelper.AppDataDirectory, SharedInfo.ConfigDirectory);
            string customConfigPath = Path.Combine(absoluteConfigDirectory, SharedInfo.IPCConfigFile);
            if (File.Exists(customConfigPath))
            {
                var configRoot = new ConfigurationBuilder().SetBasePath(absoluteConfigDirectory).AddJsonFile(SharedInfo.IPCConfigFile, false, true).Build();
                var urlSection = configRoot.GetSection("Url").Value;
                try
                {
                    var url = new Uri(urlSection);
                    if (IPAddress.Any.ToString() == url.Host)
                    {
                        return defaultUrl;
                    }
                    else
                    {
                        return url.AbsolutePath;
                    }
                }
                catch
                {
                    return defaultUrl;
                }
            }
            else
            {
                return defaultUrl;
            }
        }
    }
}
