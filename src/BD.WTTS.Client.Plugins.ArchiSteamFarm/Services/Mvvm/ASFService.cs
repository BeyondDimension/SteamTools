#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
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

    public IConsoleBuilder ConsoleLogBuilder { get; } = new ConsoleBuilder();

    public SourceCache<Bot, string> SteamBotsSourceList;

    public bool IsASFRuning => archiSteamFarmService.StartTime != null;

    GlobalConfig? _GlobalConfig;

    public GlobalConfig? GlobalConfig
    {
        get => _GlobalConfig;
        set => this.RaiseAndSetIfChanged(ref _GlobalConfig, value);
    }

    private ASFService()
    {
        mCurrent = this;

        SteamBotsSourceList = new SourceCache<Bot, string>(t => t.BotName);

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

    /// <summary>
    /// 是否正在启动或停止中
    /// </summary>
    public bool IsASFRunOrStoping { get; private set; }

    public Task InitASF() => InitASFCore(true);

    public async Task InitASFCore(bool showToast)
    {
        if (IsASFRunOrStoping) return;

        if (showToast) Toast.Show(AppResources.ASF_Starting, ToastLength.Short);

        IsASFRunOrStoping = true;

        //if (ASF.GlobalConfig?.SteamOwnerID is null or 0)
        //{
        //    Toast.Show(AppResources.ASF_Starting, ToastLength.Long);
        //    return;
        //}

        var isOk = await archiSteamFarmService.Start();
        if (!isOk)
        {
            if (showToast) Toast.Show(AppResources.ASF_Stoped, ToastLength.Short);
            return;
        }

        RefreshBots();

        IPCUrl = archiSteamFarmService.GetIPCUrl();

        MainThread2.BeginInvokeOnMainThread(() =>
        {
            this.RaisePropertyChanged(nameof(IsASFRuning));
        });

        IsASFRunOrStoping = false;

        if (showToast) Toast.Show(AppResources.ASF_Started, ToastLength.Short);
    }

    public Task StopASF() => StopASFCore(true);

    public async Task StopASFCore(bool showToast)
    {
        if (IsASFRunOrStoping) return;

        if (showToast) Toast.Show(AppResources.ASF_Stoping, ToastLength.Short);

        IsASFRunOrStoping = true;

        await archiSteamFarmService.Stop();

        MainThread2.BeginInvokeOnMainThread(() =>
        {
            this.RaisePropertyChanged(nameof(IsASFRuning));
        });

        IsASFRunOrStoping = false;

        if (showToast) Toast.Show(AppResources.ASF_Stoped, ToastLength.Short);
    }

    public void RefreshBots()
    {
        var bots = archiSteamFarmService.GetReadOnlyAllBots();
        if (bots.Any_Nullable())
        {
            SteamBotsSourceList.AddOrUpdate(bots!.Values);
        }
    }

    public void RefreshConfig()
    {
        GlobalConfig = archiSteamFarmService.GetGlobalConfig();
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
                if (file.Name != SharedInfo.GlobalConfigFileName)
                {
                    if (!allowBot) continue;
                    var bot = await BotConfig.Load(file.FullName).ConfigureAwait(false);
                    config = bot.BotConfig;
                }
                else
                {
                    if (!allowGlobal) continue;
                    var g = await GlobalConfig.Load(file.FullName).ConfigureAwait(false);
                    config = g.GlobalConfig;
                }
                if (config != null)
                {
                    try
                    {
                        file.CopyTo(Path.Combine(SharedInfo.ConfigDirectory, file.Name), true);
                        num++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(ASFService), ex, nameof(ImportBotFiles));
                        continue;
                    }
                    if (allowGlobal && config is GlobalConfig g)
                    {
                        GlobalConfig = ASF.GlobalConfig = g;
                    }
                }
            }
        }
        Toast.Show(string.Format(AppResources.LocalAuth_ImportSuccessTip, num));
    }
}
#endif