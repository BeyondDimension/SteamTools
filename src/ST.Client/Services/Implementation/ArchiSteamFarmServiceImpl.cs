#if !EXCLUDE_ASF
using ArchiSteamFarm;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Library;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.NLog.Targets;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using NLog;
using ReactiveUI;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CC = System.Common.Constants;
using MSEXLog = Microsoft.Extensions.Logging;

namespace System.Application.Services.Implementation
{
    public partial class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService, IIoc
    {
        const string TAG = "ArchiSteamFarmS";

        public ArchiSteamFarmServiceImpl()
        {
            ArchiSteamFarmLibrary.Init(this, IOPath.AppDataDirectory, IApplication.LogDirPathASF);
        }

        public event Action<string>? OnConsoleWirteLine;

        public TaskCompletionSource<string>? ReadLineTask { get; set; }

        private readonly AsyncLock @lock = new AsyncLock();

        bool _IsReadPasswordLine;

        public bool IsReadPasswordLine
        {
            get => _IsReadPasswordLine;
            set => this.RaiseAndSetIfChanged(ref _IsReadPasswordLine, value);
        }

        public DateTimeOffset? StartTime { get; set; }

        public Version CurrentVersion => SharedInfo.Version;

        private bool isFirstStart = true;

        T IIoc.GetRequiredService<T>() where T : class => DI.Get<T>();

        T? IIoc.GetService<T>() where T : class => DI.Get_Nullable<T>();

        public async Task<bool> Start(string[]? args = null)
        {
            try
            {
                StartTime = DateTimeOffset.Now;

                if (isFirstStart)
                {
                    IArchiSteamFarmService.InitCoreLoggers?.Invoke();

                    InitHistoryLogger();

                    ArchiSteamFarm.NLog.Logging.GetUserInputFunc = async (bool isPassword) =>
                    {
                        using (await @lock.LockAsync())
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
                        }
                    };

                    await ReadEncryptionKeyAsync();

                    await Program.Init(args).ConfigureAwait(false);
                    isFirstStart = false;
                }
                else
                {
                    if (!await Program.InitASF().ConfigureAwait(false))
                    {
                        await Stop().ConfigureAwait(false);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG, msg: "ASF Start Fail.");
                await Stop().ConfigureAwait(false);
                return false;
            }
        }

        public async Task Stop()
        {
            StartTime = null;
            ReadLineTask?.TrySetResult("");
            await Program.InitShutdownSequence();
        }

        async Task IArchiSteamFarmHelperService.Restart()
        {
            Toast.Show(AppResources.ASF_Restarting, ToastLength.Short);

            var s = ASFService.Current;
            if (s.IsASFRuning)
            {
                await s.StopASFCore(false);
            }
            await s.InitASFCore(false);

            Toast.Show(AppResources.ASF_Restarted, ToastLength.Short);
        }

        MSEXLog.LogLevel IArchiSteamFarmHelperService.MinimumLevel => IApplication.LoggerMinLevel;

        private void InitHistoryLogger()
        {
            ArchiSteamFarm.NLog.Logging.InitHistoryLogger();

            HistoryTarget? historyTarget = ArchiSteamFarm.LogManager.Configuration.AllTargets.OfType<HistoryTarget>().FirstOrDefault();

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

            var access = targetBot.GetAccess(steamOwnerID);
            string? response = await targetBot.Commands.Response(access, command, steamOwnerID);

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

        string IPCRootUrl
        {
            get
            {
                string? value = null;
                var a = ArchiSteamFarm.IPC.ArchiKestrel.ServerAddresses;
                var loopback = IPAddress.Loopback.ToString();
                if (a != null)
                {
                    value = a.FirstOrDefault(x => x.Contains(loopback) && x.StartsWith(CC.Prefix_HTTP))
                        ?? a.FirstOrDefault(x => x.Contains(loopback))
                        ?? a.FirstOrDefault();
                }
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = $"http://{loopback}:{CurrentIPCPortValue}";
                }
                return value;
            }
        }

        public string GetIPCUrl()
        {
            var defaultUrl = IPCRootUrl;
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

        public int CurrentIPCPortValue { get; set; }

        //public static IEnumerable<HttpMessageHandler> GetAllHandlers()
        //{
        //    // 动态更改运行中的代理设置，遍历 ASF 中的 HttpClientHandler 设置新的 Proxy
        //    var asf_handler = ASF.WebBrowser?.HttpClientHandler;
        //    if (asf_handler != null) yield return asf_handler;
        //    var bots = Bot.BotsReadOnly?.Values;
        //    if (bots != null)
        //    {
        //        foreach (var bot in bots)
        //        {
        //            var bot_handler = bot.ArchiWebHandler.WebBrowser.HttpClientHandler;
        //            if (bot_handler != null) yield return bot_handler;
        //        }
        //    }
        //}
    }
}
#endif