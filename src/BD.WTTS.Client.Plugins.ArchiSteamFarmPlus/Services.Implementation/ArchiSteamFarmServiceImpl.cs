#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ASFStrings = ArchiSteamFarm.Localization.Strings;
using AppResources = BD.WTTS.Client.Resources.Strings;
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

    private bool _IsWaitUserInput;

    protected readonly IArchiSteamFarmWebApiService webApiService = IArchiSteamFarmWebApiService.Instance;

    public ArchiSteamFarmServiceImpl()
    {
    }

    public static int? ASFProcessId { get; private set; }

    public Process? ASFProcess { get; set; }

    public event Action<string>? OnConsoleWirteLine;

    public TaskCompletionSource<string>? ReadLineTask { get; set; }

    public bool IsReadPasswordLine
    {
        get => _IsReadPasswordLine;
        set => this.RaiseAndSetIfChanged(ref _IsReadPasswordLine, value);
    }

    public Version CurrentVersion => SharedInfo.Version;

    public async Task ShellMessageInput(string data)
    {
        if (ASFProcess is not null)
        {
            if (_IsWaitUserInput) // 等待用户输入
            {
                var sw = ASFProcess.StandardInput;
                sw.WriteLine(data);
            }
            else // ASF 命令
            {
                var result = await ExecuteCommandAsync(data);
                if (!string.IsNullOrEmpty(result))
                    OnConsoleWirteLine?.Invoke(Environment.NewLine + result);
            }
        }
    }

    public async Task<(bool IsSuccess, string IPCUrl)> StartAsync(string[]? args = null)
    {
        try
        {
            if (isFirstStart)
            {
                await ReadEncryptionKeyAsync();
                isFirstStart = false;
            }

            (var isSuccess, var ipcUrl) = await StartProcessAsync();
            if (!isSuccess && !isFirstStart)
                throw new ArgumentException();
            else
                return (ASF.IsReady = isSuccess, ipcUrl);
        }
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "ASF Start Fail.");
            await StopAsync().ConfigureAwait(false);
            return (false, string.Empty);
        }
    }

    #region StartProcess

    private async Task<(bool IsSuccess, string Message)> StartProcessAsync()
    {
        var isStartSuccess = false;
        var ipcUrl = string.Empty;

        if (ASFProcess != null || string.IsNullOrEmpty(SharedInfo.ASFExecuteFilePath))
        {
            Toast.Show(ToastIcon.Error, BDStrings.ASF_SelectASFExePath);
            return (isStartSuccess, ipcUrl);
        }

        if (ASFSettings.CheckArchiSteamFarmExe && !await CheckFileConsistence()) // 检查文件是否被篡改
        {
            Toast.Show(ToastIcon.Error, BDStrings.ASF_ExecuteFileUnsafe);
            return (isStartSuccess, ipcUrl);
        }

        ipcUrl = GetIPCUrl();
        webApiService.SetIPCUrl(ipcUrl); // 设置 IPC 接口地址

        KillASFProcess(); // 杀死未关闭的 ASF 进程

        ASFProcess = StartAsync(SharedInfo.ASFExecuteFilePath);
        ASFProcessId = ASFProcess?.Id;

        ASFService.Current.ConsoleLogText = string.Empty;
        Task2.InBackground(ReadOutPutData, true);
        AppDomain.CurrentDomain.ProcessExit += ExitHandler;
        AppDomain.CurrentDomain.UnhandledException += ExitHandler;

        ASFProcess!.ErrorDataReceived += new DataReceivedEventHandler(ExitHandler);
        ASFProcess.BeginErrorReadLine();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        while (
            !(isStartSuccess = SocketHelper.IsUsePort(CurrentIPCPortValue)) &&
            !cancellationTokenSource.IsCancellationRequested)
        {
            await Task.Delay(1000);
            continue;
        }
        return (isStartSuccess, ipcUrl);

        Process? StartAsync(string fileName)
        {
            var options = new ProcessStartInfo(fileName);
            options.CreateNoWindow = true;
            options.UseShellExecute = false;
            options.RedirectStandardOutput = true;
            options.RedirectStandardInput = true;
            options.RedirectStandardError = true;
            options.ArgumentList.Add("--PROCESS-REQUIRED");
            if (!string.IsNullOrEmpty(EncryptionKey))
            {
                options.ArgumentList.Add("--CRYPTKEY");
                options.ArgumentList.Add(EncryptionKey);
            }
            return Process.Start(options);
        }
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
                var saveDir = Path.GetDirectoryName(savePath)!;

                var destination_asfPath = Path.Combine(saveDir, Path.GetFileName(SharedInfo.ASFExecuteFilePath));
                if (!File.Exists(destination_asfPath))
                {
                    Directory.CreateDirectory(saveDir);
                    if (await DownloadASFRelease(downloadUrl, savePath))
                        ZipFile.ExtractToDirectory(savePath, saveDir);
                    else
                        Toast.Show(ToastIcon.Error, "比对文件下载失败");
                }

                if (CalculateFileHash(SharedInfo.ASFExecuteFilePath) == CalculateFileHash(destination_asfPath))
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
        StopAsync().GetAwaiter().GetResult();
        ASFService.Current.SteamBotsSourceList.Clear();
    }

    private async void ReadOutPutData()
    {
        using (StreamReader sr = ASFProcess!.StandardOutput)
        {
            int readResult;
            char ch;
            StringBuilder sb = new();
            var len = string.Empty;

            while (ASFProcess is not null)
            {
                var readAsync = Task.Run(sr.Read);

                await Task.WhenAny(readAsync, Task.Delay(TimeSpan.FromSeconds(3)));

                _IsWaitUserInput = false;
                if (!readAsync.IsCompleted)
                {
                    if (sb.Length > 0)
                    {
                        len = sb.ToString();
                        OnConsoleWirteLine?.Invoke(len);
                        sb.Clear();
                        _IsWaitUserInput = true;
                    }
                }

                if (!readAsync.IsCompletedSuccessfully && ASF.IsReady)
                    ASFService.Current.RefreshBots();

                if ((readResult = await readAsync) != -1)
                {
                    ch = (char)readResult;
                    sb.Append(ch);

                    // Note the following common line feed chars:
                    // \n - UNIX   \r\n - DOS   \r - Mac
                    if (ch == '\r' || ch == '\n')
                    {
                        readResult = sr.Read();
                        if (readResult != -1)
                        {
                            ch = (char)readResult;
                            if (ch == '\n')
                            {
                                sb.Append(ch);
                                len = sb.ToString();
                                OnConsoleWirteLine?.Invoke(len);
                                sb.Clear();
                            }
                            else
                            {
                                len = sb.ToString();
                                OnConsoleWirteLine?.Invoke(len);
                                sb.Clear();
                                sb.Append(ch);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    public async Task StopAsync()
    {
        ASF.IsReady &= false;
        ReadLineTask?.TrySetResult("");
        if (ASFProcess != null)
        {
            await Task.WhenAny(webApiService.ASF.Exit(), Task.Delay(TimeSpan.FromSeconds(3)));
            ASFProcess.Kill();
            ASFProcess.Dispose();
            ASFProcess = null;
        }
    }

    public async Task RestartAsync()
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
                _currentIpcPortValue = ASFSettings.IPCPortId.Value;

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
        Toast.Show(ToastIcon.Success, BDStrings.ASF_EffectiveAfterRestart);
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