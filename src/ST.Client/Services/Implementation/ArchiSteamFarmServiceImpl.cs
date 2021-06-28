using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;

namespace System.Application.Services.Implementation
{
    public class ArchiSteamFarmServiceImpl : IArchiSteamFarmService
    {
        public Func<string, Task<string?>>? GetConsoleWirteFunc { get; set; }

        public async void Start(string[]? args = null)
        {
            try
            {
                await ArchiSteamFarm.Program.Init(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Toast.Show(ex.Message);
            }
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
        public void SetBot(string botName)
        {

        }

    }
}
