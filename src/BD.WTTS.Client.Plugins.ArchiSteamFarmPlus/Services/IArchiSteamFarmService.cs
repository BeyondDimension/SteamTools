#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

// ReSharper disable once CheckNamespace
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;

namespace BD.WTTS.Services;

/// <summary>
/// ArchiSteamFarm 服务
/// </summary>
public partial interface IArchiSteamFarmService
{
    static IArchiSteamFarmService Instance => Ioc.Get<IArchiSteamFarmService>();

    event Action<string>? OnConsoleWirteLine;

    TaskCompletionSource<string>? ReadLineTask { get; }

    bool IsReadPasswordLine { get; }

    Process? ASFProcess { get; }

    Version CurrentVersion { get; }

    /// <summary>
    /// 控制台输入
    /// </summary>
    /// <param name="data"></param>
    Task ShellMessageInput(string data);

    /// <summary>
    /// 启动 ArchiSteamFarm
    /// </summary>
    /// <param name="args"></param>
    Task<(bool IsSuccess, string IPCUrl)> StartAsync(string[]? args = null);

    Task StopAsync();

    /// <summary>
    /// 执行 ArchiSteamFarm 指令
    /// </summary>
    /// <param name="command"></param>
    Task<string?> ExecuteCommandAsync(string command);

    /// <summary>
    /// 获取 IPC 地址
    /// </summary>
    /// <returns></returns>
    string GetIPCUrl();

    /// <summary>
    /// 获取 Bot 只读集合
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyDictionary<string, Bot>?> GetReadOnlyAllBots();

    Task<GlobalConfig?> GetGlobalConfig();

    Task<bool> SaveBot(string botName, BotConfig botConfig);

    Task<bool> SaveGlobalConfig(GlobalConfig config);

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
            await ExecuteCommandAsync(command);
        }
        else
        {
            ReadLineTask.TrySetResult(command);
        }
    }

    int CurrentIPCPortValue { get; }

    /// <summary>
    /// 使用弹窗密码框输入自定义密钥并设置与保存
    /// </summary>
    /// <returns></returns>
    Task SetEncryptionKeyAsync();

    Task<IApiRsp> BotResumeAsync(string botNames,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotPauseAsync(string botNames,
            BotPauseRequest request,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotStopAsync(string botNames,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotStartAsync(string botNames,
            CancellationToken cancellationToken = default);

    Task<IApiRsp<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>> BotGetUsedAndUnusedKeysAsync(string botNames,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotRedeemKeyAsync(string botNames,
            BotGamesToRedeemInBackgroundRequest request,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotResetRedeemedKeysRecordAsync(string botNames,
            CancellationToken cancellationToken = default);

    Task<IApiRsp> BotDeleteAsync(
            string botNames,
            CancellationToken cancellationToken = default);
}

#endif