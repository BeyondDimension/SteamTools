using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Storage;
using ArchiSteamFarm.NLog.Targets;
using ArchiSteamFarm.Steam;
using NLog;
using System.IO;
using ArchiSteamFarm;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace System.Application.Services.Implementation
{
    public class ArchiSteamFarmServiceImpl : IArchiSteamFarmService
    {
        public Action<string>? GetConsoleWirteFunc { get; set; }

        public async void Start(string[]? args = null)
        {
            try
            {
                IArchiSteamFarmService.InitCoreLoggers?.Invoke();
                InitHistoryLogger();
                await ArchiSteamFarm.Program.Init(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Toast.Show(ex.Message);
            }
        }

        public async void Stop()
        {
            await ArchiSteamFarm.Program.InitShutdownSequence().ConfigureAwait(false);
        }

        private void InitHistoryLogger()
        {
            ArchiSteamFarm.NLog.Logging.InitHistoryLogger();

            HistoryTarget? historyTarget = ArchiSteamFarm.LogManager.Configuration.AllTargets.OfType<HistoryTarget>().FirstOrDefault();

            if (historyTarget != null)
                historyTarget.NewHistoryEntry += (object? sender, HistoryTarget.NewHistoryEntryArgs newHistoryEntryArgs) =>
                {
                    GetConsoleWirteFunc?.Invoke(newHistoryEntryArgs.Message);
                };
        }

        /// <summary>
        /// 执行asf指令
        /// </summary>
        /// <param name="command"></param>
        public async Task<string?> ExecuteCommand(string command)
        {
            Bot? targetBot = Bot.Bots?.OrderBy(bot => bot.Key, Bot.BotsComparer).Select(bot => bot.Value).FirstOrDefault();

            if (targetBot == null)
            {
                //Console.WriteLine(@"<< " + Strings.ErrorNoBotsDefined);

                return null;
            }

            //Console.WriteLine(@"<> " + Strings.Executing);

            ulong steamOwnerID = ASF.GlobalConfig?.SteamOwnerID ?? ArchiSteamFarm.Storage.GlobalConfig.DefaultSteamOwnerID;

            string? response = await targetBot.Commands.Response(steamOwnerID, command!).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// 获取bot只读集合
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Bot>? GetReadOnlyAllBots()
        {
            return Bot.BotsReadOnly;
        }

        /// <summary>
        /// 设置并保存bot
        /// </summary>
        /// <returns></returns>
        public void SaveBot(string botName)
        {

        }

        public GlobalConfig? GetGlobalConfig()
        {
            return ASF.GlobalConfig;
        }

        public string GetIPCUrl()
        {
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
                        return "http://" + IPAddress.Loopback + ":1242";
                    }
                    else
                    {
                        return url.AbsolutePath;
                    }
                }
                catch
                {
                    return "http://" + IPAddress.Loopback + ":1242";
                }
            }
            else
            {
                return "http://" + IPAddress.Loopback + ":1242";
            }
        }

        public void SaveGlobalConfig(GlobalConfig config)
        {

        }
    }
}
