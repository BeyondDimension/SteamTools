using ArchiSteamFarm.Steam;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IArchiSteamFarmService
    {
        static IArchiSteamFarmService Instance => DI.Get<IArchiSteamFarmService>();

        static Action? InitCoreLoggers { protected get; set; }

        Action<string>? GetConsoleWirteFunc { get; set; }

        TaskCompletionSource<string>? ReadLineTask { get; }

        bool IsReadPasswordLine { get; }

        /// <summary>
        /// 启动ASF
        /// </summary>
        /// <param name="args"></param>
        Task Start(string[]? args = null);

        Task Stop();

        /// <summary>
        /// 执行asf指令
        /// </summary>
        /// <param name="command"></param>
        Task<string?> ExecuteCommand(string command);

        /// <summary>
        /// 获取IPC地址
        /// </summary>
        /// <returns></returns>
        string GetIPCUrl();

        /// <summary>
        /// 获取bot只读集合
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, Bot>? GetReadOnlyAllBots();
    }
}