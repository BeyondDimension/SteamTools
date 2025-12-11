#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using Newtonsoft.Json;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class ASFService : ReactiveObject
{
    static readonly Lazy<ASFService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ASFService Current => mCurrent.Value;

    readonly IArchiSteamFarmService archiSteamFarmService = IArchiSteamFarmService.Instance;

    string? _IPCUrl;

    public string? IPCUrl
    {
        get => _IPCUrl;
        set => this.RaiseAndSetIfChanged(ref _IPCUrl, value);
    }

    string? _ConsoleLogText;

    public string? ConsoleLogText
    {
        get => _ConsoleLogText;
        set => this.RaiseAndSetIfChanged(ref _ConsoleLogText, value);
    }

    public IConsoleBuilder ConsoleLogBuilder { get; } = new ConsoleBuilder();

    public SourceCache<BotViewModel, string> SteamBotsSourceList;

    public bool IsASFRuning => archiSteamFarmService.ASFProcess != null;

    GlobalConfig? _GlobalConfig;

    public GlobalConfig? GlobalConfig
    {
        get => _GlobalConfig;
        set => this.RaiseAndSetIfChanged(ref _GlobalConfig, value);
    }

    private ASFService()
    {
        SteamBotsSourceList = new SourceCache<BotViewModel, string>(t => t.Bot.BotName);

        archiSteamFarmService.OnConsoleWirteLine += OnConsoleWriteLine;

        ASFSettings.ConsoleMaxLine.Subscribe(x =>
        {
            var line = x;
            if (x < ASFSettings.MinRangeConsoleMaxLine) line = ASFSettings.MinRangeConsoleMaxLine;
            if (x > ASFSettings.MaxRangeConsoleMaxLine) line = ASFSettings.MaxRangeConsoleMaxLine;
            ConsoleLogBuilder.MaxLine = line;
        });
    }

    void OnConsoleWriteLine(string message)
    {
        MainThread2.BeginInvokeOnMainThread(() =>
        {
            //ConsoleLogBuilder.Append(message); // message 包含换行
            //var text = ConsoleLogBuilder.ToString();
            ConsoleLogText += message;
        });
    }

    public void ShellMessageInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return;
        }
        _ = archiSteamFarmService.ShellMessageInput(input);
    }

    /// <summary>
    /// 是否正在启动或停止中
    /// </summary>
    public bool IsASFRunOrStoping { get; private set; }

    public Task InitASFAsync() => InitASFCoreAsync(true);

    public async Task InitASFCoreAsync(bool showToast)
    {
        if (IsASFRunOrStoping) return;

        if (showToast) Toast.Show(ToastIcon.Info, AppResources.ASF_Starting, ToastLength.Short);

        IsASFRunOrStoping = true;

        ConsoleLogText = string.Empty;
        (var isOk, var ipcURL) = await archiSteamFarmService.StartAsync();
        if (!isOk)
        {
            if (showToast) Toast.Show(ToastIcon.Error, AppResources.ASF_Stoped, ToastLength.Short);
            IsASFRunOrStoping = false;
            return;
        }

        RefreshBots();

        IPCUrl = ipcURL;

        MainThread2.BeginInvokeOnMainThread(() =>
        {
            this.RaisePropertyChanged(nameof(IsASFRuning));
        });

        IsASFRunOrStoping = false;

        if (showToast) Toast.Show(ToastIcon.Success, AppResources.ASF_Started, ToastLength.Short);
    }

    public Task StopASFAsync() => StopASFCoreAsync(true);

    public async Task StopASFCoreAsync(bool showToast)
    {
        if (IsASFRunOrStoping) return;

        if (showToast) Toast.Show(ToastIcon.Info, AppResources.ASF_Stoping, ToastLength.Short);

        IsASFRunOrStoping = true;

        await archiSteamFarmService.StopAsync();

        SteamBotsSourceList.Clear();

        MainThread2.BeginInvokeOnMainThread(() =>
        {
            this.RaisePropertyChanged(nameof(IsASFRuning));
        });

        IsASFRunOrStoping = false;

        if (showToast) Toast.Show(ToastIcon.Info, AppResources.ASF_Stoped, ToastLength.Short);
    }

    public async void RefreshBots()
    {
        var bots = await archiSteamFarmService.GetReadOnlyAllBots();
        if (bots.Any_Nullable())
        {
            SteamBotsSourceList.Clear();
            SteamBotsSourceList.AddOrUpdate(bots!.Values.Select(s => (BotViewModel)s));
        }
    }

    public async void RefreshConfig()
    {
        GlobalConfig = await archiSteamFarmService.GetGlobalConfig();
    }

    public void OpenFolder(string tag)
    {
        if (string.IsNullOrEmpty(ASFSettings.ArchiSteamFarmExePath.Value))
        {
            Toast.Show(ToastIcon.Error, BDStrings.ASF_SetExePathFirst);
            return;
        }

        string folderASFPathValue = string.Empty;
        if (!Enum.TryParse<EPathFolder>(tag, true, out var folderASFPath) ||
            !Path.Exists(folderASFPathValue = folderASFPath.GetFolderPath()))
        {
            Toast.Show(ToastIcon.Error, BDStrings.ASF_SelectASFExePath);
            return;
        }

        IPlatformService.Instance.OpenFolder(folderASFPathValue);
    }

    async void OpenBrowserCore(ActionItem tag)
    {
        var url = tag switch
        {
            ActionItem.Repo => "https://github.com/JustArchiNET/ArchiSteamFarm",
            ActionItem.Wiki => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
            ActionItem.ConfigGenerator => "https://justarchinet.github.io/ASF-WebConfigGenerator",
            _ => string.Empty,
        };

        if (url != string.Empty)
        {
            await Browser2.OpenAsync(url, BrowserLaunchMode.External);
            return;
        }

        url = tag switch
        {
            ActionItem.WebConfig => IPCUrl + "/asf-config",
            ActionItem.WebAddBot => IPCUrl + "/bot/new",
            _ => IPCUrl,
        };

        if (string.IsNullOrEmpty(IPCUrl) && !Uri.TryCreate(IPCUrl, UriKind.Absolute, out _))
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
                return;
            }
        }

        await Browser2.OpenAsync(url, BrowserLaunchMode.External);
    }

    public void OpenBrowser(string? tag)
    {
        var tag_ = Enum.TryParse<ActionItem>(tag, out var @enum) ? @enum : default;
        OpenBrowserCore(tag_);
    }

    public async void ImportBotFiles(IEnumerable<string>? files) => await ImportJsonFileAsync(files, allowBot: true, allowGlobal: false);

    public async void ImportGlobalFiles(string? file) => await ImportJsonFileAsync(new[] { file }, allowBot: false, allowGlobal: true);

    async Task ImportJsonFileAsync(IEnumerable<string?>? files, bool allowBot, bool allowGlobal)
    {
        if (files == null) return;
        var num = 0;
        foreach (var filename in files)
        {
            if (string.IsNullOrWhiteSpace(filename)) continue;
            var file = new FileInfo(filename);
            if (file.Exists && string.Equals(file.Extension, SharedInfo.JsonConfigExtension, StringComparison.OrdinalIgnoreCase))
            {
                object? config;
                string json;
                try
                {
                    json = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(json))
                    {
                        ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(json)));

                        return;
                    }

                    if (file.Name != SharedInfo.GlobalConfigFileName)
                    {
                        if (!allowBot) continue;
                        var botConfig = JsonConvert.DeserializeObject<BotConfig>(json);
                        if (botConfig == null)
                        {
                            ASF.ArchiLogger.LogNullError(botConfig);
                            return;
                        }

                        if (await archiSteamFarmService.SaveBot(Path.GetFileNameWithoutExtension(file.FullName), botConfig))
                            config = botConfig;
                        else
                            return;
                    }
                    else
                    {
                        if (!allowGlobal) continue;
                        var globalConfig = JsonConvert.DeserializeObject<GlobalConfig>(json);
                        if (globalConfig == null)
                        {
                            ASF.ArchiLogger.LogNullError(globalConfig);
                            return;
                        }

                        if (await archiSteamFarmService.SaveGlobalConfig(globalConfig))
                            config = globalConfig;
                        else
                            return;
                    }

                }
                catch (Exception e)
                {
                    ASF.ArchiLogger.LogGenericException(e);

                    return;
                }
                if (config != null)
                {
                    try
                    {
                        file.CopyTo(Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ConfigDirectory, file.Name), true);
                        num++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(ASFService), ex, nameof(ImportBotFiles));
                        continue;
                    }
                    if (allowGlobal && config is GlobalConfig g)
                    {
                        GlobalConfig = g;
                    }
                }
            }
        }
        Toast.Show(ToastIcon.Success, string.Format(AppResources.LocalAuth_ImportSuccessTip_, num));
    }

    public async Task SelectASFProgramLocationAsync()
    {
        AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
            new("ArchiSteamFarm") {
                Patterns = new[] { "ArchiSteamFarm.exe", },
                //MimeTypes =
                //AppleUniformTypeIdentifiers =
                },
        };
        await FilePicker2.PickAsync((path) =>
        {
            if (!string.IsNullOrEmpty(path))
                ASFSettings.ArchiSteamFarmExePath.Value = path;
        }, fileTypes);
    }

    public async void DownloadASFAsync(string variant = "win-x64", IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var assetName = $"{nameof(ASF)}-{variant}.zip";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", IHttpPlatformHelperService.Instance.UserAgent);
            using var latestReleaseStream = await httpClient.GetStreamAsync("https://api.github.com/repos/JustArchiNET/ArchiSteamFarm/releases/latest", cancellationToken);
            using var element = await JsonDocument.ParseAsync(latestReleaseStream, cancellationToken: cancellationToken);
            if (element.RootElement.TryGetProperty("assets", out var assets) &&
                assets.EnumerateArray().FirstOrDefault(s => s.GetProperty("name").ValueEquals(assetName))
                .TryGetProperty("browser_download_url", out var downloadUrl))
            {
                var tag_name = element.RootElement.GetProperty("tag_name").GetString();
                var downloadSavingPath = Path.Combine(Plugin.Instance.AppDataDirectory, $"ASF-{tag_name}", assetName);
                var downloadSavingDir = Path.GetDirectoryName(downloadSavingPath)!;
                Directory.CreateDirectory(downloadSavingDir);

                var message = new HttpRequestMessage(HttpMethod.Get, downloadUrl.GetString());
                var result = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                using var contentStream = await result.Content.ReadAsStreamAsync(cancellationToken);

                byte batch = 0;
                long readThisBatch = 0;
                long batchIncreaseSize = result.Content.Headers.ContentLength.GetValueOrDefault() / 100;
                await using (FileStream fileStream = new(downloadSavingPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;
                    byte[] buffer = bytePool.Rent(4096);

                    try
                    {
                        while (contentStream.CanRead)
                        {
                            int read = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

                            if (read == 0)
                                break;

                            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                            if ((progress == null) || (batchIncreaseSize == 0) || (batch >= 99))
                            {
                                continue;
                            }

                            readThisBatch += read;

                            while ((readThisBatch >= batchIncreaseSize) && (batch < 99))
                            {
                                readThisBatch -= batchIncreaseSize;
                                progress.Report(++batch);
                            }
                        }
                    }
                    finally
                    {
                        bytePool.Return(buffer);
                    }
                }

                ZipFile.ExtractToDirectory(downloadSavingPath, downloadSavingDir);
                ASFSettings.ArchiSteamFarmExePath.Value = Path.Combine(downloadSavingDir, "ArchiSteamFarm.exe");
                progress?.Report(100);
                Toast.Show(ToastIcon.Success, "ASF 下载成功");
            }
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, "ASF 文件下载异常");
        }
    }
}
#endif