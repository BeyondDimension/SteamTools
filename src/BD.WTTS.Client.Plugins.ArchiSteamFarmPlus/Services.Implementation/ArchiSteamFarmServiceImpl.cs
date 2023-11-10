#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ASFStrings = ArchiSteamFarm.Localization.Strings;
using AppResources = BD.WTTS.Client.Resources.Strings;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Storage;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.IPC;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public partial class ArchiSteamFarmServiceImpl : ReactiveObject, IArchiSteamFarmService
{
    const string TAG = "ArchiSteamFarmS";

    private readonly AsyncLock @lock = new AsyncLock();

    private bool isFirstStart = true;

    private bool _IsReadPasswordLine;

    private HttpClient _httpClient = new();

    protected readonly IArchiSteamFarmWebApiService webApiService = IArchiSteamFarmWebApiService.Instance;

    public ArchiSteamFarmServiceImpl()
    {
    }

    public Process? ASFProcess { get; set; }

    public event Action<string>? OnConsoleWirteLine;

    public TaskCompletionSource<string>? ReadLineTask { get; set; }

    public bool IsReadPasswordLine
    {
        get => _IsReadPasswordLine;
        set => this.RaiseAndSetIfChanged(ref _IsReadPasswordLine, value);
    }

    public DateTimeOffset? StartTime { get; set; }

    public Version CurrentVersion => SharedInfo.Version;

    public async Task<bool> StartAsync(string[]? args = null)
    {
        try
        {
            var isOk = false;
            StartTime = DateTimeOffset.Now;
            if (isFirstStart)
            {
                await ReadEncryptionKeyAsync();
                isFirstStart = false;
            }

            if (!await StartProcess() && !isFirstStart)
            {
                await StopAsync().ConfigureAwait(false);
            }
            else
            {
                isOk = true;
                ASF.IsReady = true;
            }
            return isOk;
        }
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "ASF Start Fail.");
            await StopAsync().ConfigureAwait(false);
            return false;
        }
    }

    #region StartProcess

    private async Task<bool> StartProcess()
    {
        var isStartSuccess = false;
        try
        {
            if (ASFProcess != null || string.IsNullOrEmpty(SharedInfo.ASFExecuteFilePath))
            {
                Toast.Show(ToastIcon.Error, BDStrings.ASF_SelectASFExePath);
                return isStartSuccess;
            }

            if (ASFSettings.CheckArchiSteamFarmExe && !await CheckFileConsistence()) // 检查文件是否被篡改
            {
                Toast.Show(ToastIcon.Error, BDStrings.ASF_ExecuteFileUnsafe);
                return isStartSuccess;
            }

            webApiService.SetIPCUrl(GetIPCUrl()); // 设置 IPC 接口地址

            KillASFProcess(); // 杀死未关闭的 ASF 进程

            var options = new ProcessStartInfo(SharedInfo.ASFExecuteFilePath);
            options.CreateNoWindow = true;
            options.UseShellExecute = false;
            options.RedirectStandardOutput = true;
            options.RedirectStandardInput = true;
            options.RedirectStandardError = true;
            if (!string.IsNullOrEmpty(EncryptionKey))
            {
                options.ArgumentList.Add("--CRYPTKEY");
                options.ArgumentList.Add(EncryptionKey);
            }
            ASFProcess = Process.Start(options);
            AppDomain.CurrentDomain.ProcessExit += ExitHandler;
            AppDomain.CurrentDomain.UnhandledException += ExitHandler;
            ASFProcess!.ErrorDataReceived += new DataReceivedEventHandler(ExitHandler);
            ASFProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    OnConsoleWirteLine?.Invoke(e.Data);
            });

            ASFProcess.BeginOutputReadLine();
            ASFProcess.BeginErrorReadLine();
            while (!SocketHelper.IsUsePort(CurrentIPCPortValue))
            {
                Thread.Sleep(1000);
                continue;
            }
            isStartSuccess = true;
        }
        catch (Exception)
        {
        }
        return isStartSuccess;
    }

    private void KillASFProcess()
    {
        var processName = Path.GetFileNameWithoutExtension(SharedInfo.ASFExecuteFilePath);
        var process_list = Process.GetProcessesByName(processName);
        if (process_list.Any())
            process_list.ForEach(x =>
            {
                if (Path.GetFullPath(x.MainModule?.FileName ?? string.Empty) == Path.GetFullPath(SharedInfo.ASFExecuteFilePath))
                {
                    x.Kill(); x.Dispose();
                }
            });
    }

    private async Task<bool> CheckFileConsistence()
    {
        var isConsistence = false;
        if (File.Exists(SharedInfo.ASFExecuteFilePath))
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(SharedInfo.ASFExecuteFilePath);

                var fileName = "ASF-win-x64.zip";
                var downloadUrl = $"https://github.com/JustArchiNET/ArchiSteamFarm/releases/download/{versionInfo.FileVersion}/{fileName}";
                var savePath = Path.Combine(Plugin.Instance.AppDataDirectory, $"ASF-{versionInfo.FileVersion}", fileName);
                var destDir = Path.GetDirectoryName(savePath)!;

                var online_asfPath = Path.Combine(destDir, Path.GetFileName(SharedInfo.ASFExecuteFilePath));

                if (!File.Exists(online_asfPath))
                {
                    Directory.CreateDirectory(destDir);
                    if (await DownloadASFRelease(downloadUrl, savePath))
                        ZipFile.ExtractToDirectory(savePath, destDir);
                }
                if (File.Exists(online_asfPath) && CalculateFileHash(SharedInfo.ASFExecuteFilePath) == CalculateFileHash(online_asfPath))
                    isConsistence = true;
            }
            catch (Exception)
            {
            }

        }
        return isConsistence;

        string CalculateFileHash(string filePath)
        {
            using (var hasher = SHA256.Create())
            using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
            {
                byte[] hashBytes = hasher.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        async Task<bool> DownloadASFRelease(string downloadUrl, string savePath)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

                // Ensure a successful response before proceeding
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }

    private void ExitHandler(object? sender, EventArgs eventArgs)
    {
        ASFService.Current.StopASFAsync().GetAwaiter().GetResult();
    }
    #endregion

    public async Task StopAsync()
    {
        StartTime = null;
        ASF.IsReady &= false;
        ReadLineTask?.TrySetResult("");
        if (ASFProcess != null)
        {
            await webApiService.ASF.Exit();
            ASFProcess.Kill();
            ASFProcess.Dispose();
            ASFProcess = null;
        }
    }

    public async Task Restart()
    {
        Toast.Show(ToastIcon.Info, AppResources.ASF_Restarting, ToastLength.Short);

        var s = ASFService.Current;
        if (s.IsASFRuning)
        {
            await s.StopASFCoreAsync(false);
        }
        await s.InitASFCoreAsync(false);

        Toast.Show(ToastIcon.Success, AppResources.ASF_Restarted, ToastLength.Short);
    }

    LogLevel MinimumLevel => IApplication.LoggerMinLevel;

    public async Task<string?> ExecuteCommandAsync(string command)
    {
        (var isSuccess, _) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return string.Empty;

        var request = new CommandRequest(command);
        var r = await webApiService.Command.CommandPost(request);
        return r.IsSuccess ? r.Content : null;
    }

    public async Task<IReadOnlyDictionary<string, Bot>?> GetReadOnlyAllBots()
    {
        (var isSuccess, var rsp) = VerifyApiReady<IReadOnlyDictionary<string, Bot>?>();
        if (!isSuccess) return rsp.Content;

        var r = await webApiService.Bot.BotGet("asf");
        var bots = r.Content;
        //if (bots is not null)
        //    foreach (var bot in bots.Values)
        //    {
        //        bot.AvatarUrl = GetAvatarUrl(bot);
        //    }
        return bots;
    }

    public async Task<bool> SaveBot(string botName, BotConfig botConfig)
    {
        (var isSuccess, _) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return isSuccess;

        var request = new BotRequest() { BotConfig = botConfig };
        var r = await webApiService.Bot.BotPost(botName, request);
        return r.IsSuccess;
    }

    public async Task<GlobalConfig?> GetGlobalConfig()
    {
        (var isSuccess, _) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return null;

        var r = await webApiService.ASF.Get();
        return r.IsSuccess ? r.Content.GlobalConfig : null;
    }

    public async Task<bool> SaveGlobalConfig(GlobalConfig config)
    {
        (var isSuccess, _) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return isSuccess;

        var request = new ASFRequest() { GlobalConfig = config };
        var r = await webApiService.ASF.Post(request);
        if (r.IsSuccess)
            Toast.Show(ToastIcon.Info, "SaveGlobalConfig  " + r.IsSuccess);
        return r.IsSuccess;
    }

    string IPCRootUrl { get => $"http://{IPAddress.Loopback}:{CurrentIPCPortValue}"; }

    public string GetIPCUrl()
    {
        var ipcUrl = new Uri(IPCRootUrl);
        string absoluteConfigDirectory = Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ConfigDirectory);
        string customConfigPath = Path.Combine(absoluteConfigDirectory, SharedInfo.IPCConfigFile);
        if (File.Exists(customConfigPath))
        {
            var jsonObj = JObject.Parse(File.ReadAllText(customConfigPath));
            var urlSection = jsonObj.SelectToken("Kestrel.Endpoints.Http.Url")?.Value<string>();
            try
            {
                var url = new Uri(urlSection!);
                if (url.AbsoluteUri != ipcUrl.AbsoluteUri)
                {
                    jsonObj["Kestrel"]!["Endpoints"]!["Http"]!["Url"] = ipcUrl.OriginalString;
                    File.WriteAllText(customConfigPath, jsonObj.ToString());
                }
            }
            catch
            {
            }
        }
        else
        {
            var iPCConfig = new IPCConfig(ipcUrl);
            File.WriteAllText(customConfigPath, System.Text.Json.JsonSerializer.Serialize(iPCConfig));
        }
        CurrentIPCPortValue = ipcUrl.Port;
        return ipcUrl.OriginalString;
    }

    private int _currentIpcPortValue;

    public int CurrentIPCPortValue
    {
        get
        {
            if (_currentIpcPortValue == default)
            {
                _currentIpcPortValue = ASFSettings.IPCPortId.Value != default(int)
                    ? ASFSettings.IPCPortId.Value
                    : ASFSettings.DefaultIPCPortIdValue;

                if (ASFSettings.IPCPortOccupiedRandom.Value)
                {
                    if (SocketHelper.IsUsePort(_currentIpcPortValue))
                    {
                        _currentIpcPortValue = SocketHelper.GetRandomUnusedPort(IPAddress.Loopback);
                        return _currentIpcPortValue;
                    }
                }
            }
            return _currentIpcPortValue;
        }

        set
        {
            _currentIpcPortValue = value;
        }
    }

    const string ASF_CRYPTKEY = "ASF_CRYPTKEY";
    const string ASF_CRYPTKEY_DEF_VALUE = nameof(ArchiSteamFarm);

    public string? EncryptionKey { get; set; }

    public async Task SetEncryptionKeyAsync()
    {
        var result = await TextBoxWindowViewModel.ShowDialogAsync(new TextBoxWindowViewModel
        {
            Title = AppResources.ASF_SetCryptKey,
            Placeholder = ASF_CRYPTKEY,
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        });
        var isUseDefaultCryptKey = string.IsNullOrEmpty(result) || result == ASF_CRYPTKEY_DEF_VALUE;
        if (!isUseDefaultCryptKey)
        {
            await ISecureStorage.Instance.SetAsync(ASF_CRYPTKEY, result);
        }
        else
        {
            await ISecureStorage.Instance.RemoveAsync(ASF_CRYPTKEY);
            result = ASF_CRYPTKEY_DEF_VALUE;
        }
        EncryptionKey = result;
    }

    /// <summary>
    /// 尝试读取已保存的自定义密钥并应用
    /// </summary>
    /// <returns></returns>
    private async Task ReadEncryptionKeyAsync()
    {
        if (!string.IsNullOrEmpty(EncryptionKey))
        {
            // 当前运行中已设置了自定义密钥，则跳过
            return;
        }
        var result = await ISecureStorage.Instance.GetAsync(ASF_CRYPTKEY);
        if (!string.IsNullOrEmpty(result))
            EncryptionKey = result;
    }

    public async Task<IApiRsp> BotResumeAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.ResumePost(botNames, cancellationToken);
    }

    public async Task<IApiRsp> BotPauseAsync(string botNames, BotPauseRequest request, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.PausePost(botNames, request, cancellationToken);
    }

    public async Task<IApiRsp> BotStopAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.StopPost(botNames, cancellationToken);
    }

    public async Task<IApiRsp> BotStartAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.StartPost(botNames, cancellationToken);
    }

    public async Task<IApiRsp<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>> BotGetUsedAndUnusedKeysAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.GamesToRedeemInBackgroundGet(botNames, cancellationToken);
    }

    public async Task<IApiRsp> BotRedeemKeyAsync(string botNames, BotGamesToRedeemInBackgroundRequest request, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.GamesToRedeemInBackgroundPost(botNames, request, cancellationToken);
    }

    public async Task<IApiRsp> BotResetRedeemedKeysRecordAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.GamesToRedeemInBackgroundDelete(botNames, cancellationToken);
    }

    public async Task<IApiRsp> BotDeleteAsync(string botNames, CancellationToken cancellationToken = default)
    {
        (var isSuccess, var rsp) = VerifyApiReady<GenericResponse>();
        if (!isSuccess) return rsp;
        return await webApiService.Bot.BotDelete(botNames, cancellationToken);
    }

    private (bool isSuccess, IApiRsp<TResponseBody?>? apiRsp) VerifyApiReady<TResponseBody>()
    {
        string? ipc_error = null;
        if (!ASFService.Current.IsASFRuning)
        {
            ipc_error = BDStrings.ASF_RequirRunASF;
        }
        else if (!ASF.IsReady)
        {
            // IPC 未启动前无法获取正确的端口号，会导致拼接的 URL 值不正确
            ipc_error = BDStrings.ASF_IPCIsReadyFalse;
        }
        if (ipc_error != null)
        {
            Toast.Show(ToastIcon.Error, ipc_error);
            return (false, ApiRspHelper.Fail<TResponseBody>(ipc_error));
        }
        return (true, null);
    }
}

#endif