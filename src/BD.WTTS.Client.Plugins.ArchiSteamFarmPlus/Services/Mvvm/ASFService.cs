#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using Newtonsoft.Json;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class ASFService : ReactiveObject
{
    static ASFService? mCurrent;

    public static ASFService Current => mCurrent ?? new();

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

    string? _ConsoleLogInput;

    public string? ConsoleLogInput
    {
        get => _ConsoleLogInput;
        set => this.RaiseAndSetIfChanged(ref _ConsoleLogInput, value);
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
        mCurrent = this;

        SteamBotsSourceList = new SourceCache<BotViewModel, string>(t => t.Bot.BotName);

        archiSteamFarmService.OnConsoleWirteLine += OnConsoleWirteLine;

        ASFSettings.ConsoleMaxLine.Subscribe(x =>
        {
            var line = x;
            if (x < ASFSettings.MinRangeConsoleMaxLine) line = ASFSettings.MinRangeConsoleMaxLine;
            if (x > ASFSettings.MaxRangeConsoleMaxLine) line = ASFSettings.MaxRangeConsoleMaxLine;
            ConsoleLogBuilder.MaxLine = line;
        });
    }

    void OnConsoleWirteLine(string message)
    {
        MainThread2.InvokeOnMainThreadAsync(() =>
        {
            ConsoleLogBuilder.AppendLine(message);
            var text = ConsoleLogBuilder.ToString();
            ConsoleLogText = text;
        });
    }

    public void ShellMessageInput()
    {
        if (string.IsNullOrEmpty(ConsoleLogInput))
        {
            return;
        }
        archiSteamFarmService.ShellMessageInput(ConsoleLogInput);
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
        if (!Enum.TryParse<EPathFolder>(tag, true, out var folderASFPath)) return;
        var folderASFPathValue = folderASFPath.GetFolderPath();
        IPlatformService.Instance.OpenFolder(folderASFPathValue);
    }

    async void OpenBrowserCore(ActionItem tag)
    {
        var url = tag switch
        {
            ActionItem.Repo => "https://github.com/JustArchiNET/ArchiSteamFarm",
            ActionItem.Wiki => "https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Home-zh-CN",
            ActionItem.ConfigGenerator => "https://justarchinet.github.io/ASF-WebConfigGenerator",
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

    public async Task SelectASFProgramLocation()
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
}
#endif