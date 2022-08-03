#if !EXCLUDE_ASF
using ArchiSteamFarm;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Storage;
using ReactiveUI;
using System.Application.Settings;
using System.Application.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// ASF 服务
    /// </summary>
    public partial interface IArchiSteamFarmService
    {
        static new IArchiSteamFarmService Instance => DI.Get<IArchiSteamFarmService>();

        static Action? InitCoreLoggers { protected get; set; }

        event Action<string>? OnConsoleWirteLine;

        TaskCompletionSource<string>? ReadLineTask { get; }

        bool IsReadPasswordLine { get; }

        DateTimeOffset? StartTime { get; }

        Version CurrentVersion { get; }

        /// <summary>
        /// 启动ASF
        /// </summary>
        /// <param name="args"></param>
        Task<bool> Start(string[]? args = null);

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

        GlobalConfig? GetGlobalConfig();

        async void CommandSubmit(string? command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            if (ReadLineTask is null)
            {
                if (command[0] == '!')
                {
                    command = command.Remove(0, 1);
                }
                await ExecuteCommand(command);
            }
            else
            {
                ReadLineTask.TrySetResult(command);
            }
        }
    }
}
#endif