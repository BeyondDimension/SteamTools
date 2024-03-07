// ReSharper disable once CheckNamespace
namespace BD.WTTS;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
partial class Startup // Properties
{
    string DebuggerDisplay =>
        $"IsMainProcess: {IsMainProcess}, IsCustomEntryPoint: {IsCustomEntryPoint}, IsDesignMode: {IsDesignMode}, App: {App}";

    static Startup? instance;

    public static Startup Instance => instance ?? throw new NullReferenceException("Startup init fail.");

    /// <summary>
    /// 当前的模块名称
    /// </summary>
    public string? ModuleName { get; private set; }

    /// <summary>
    /// 当前是否为主进程
    /// </summary>
    public bool IsMainProcess { get; private set; }

    /// <summary>
    /// 当前是否为控制台工具进程
    /// </summary>
    public bool IsConsoleLineToolProcess { get; private set; }

#if DEBUG
    /// <inheritdoc cref="IsConsoleLineToolProcess"/>
    [Obsolete("use IsConsoleLineToolProcess", true)]
    public bool IsCLTProcess => IsConsoleLineToolProcess;
#endif

    /// <summary>
    /// 是否使用自定义 .NET Host 入口点
    /// </summary>
    public bool IsCustomEntryPoint { get; init; }

    /// <summary>
    /// 是否在设计器中运行
    /// </summary>
    public bool IsDesignMode { get; set; }

    /// <summary>
    /// 是否在 Steam 中运行
    /// </summary>
    public bool IsSteamRun { get; set; }

    /// <summary>
    /// 是否最小化启动
    /// </summary>
    public bool IsMinimize { get; set; }

    /// <summary>
    /// 是否为代理服务
    /// </summary>
    public bool IsProxyService { get; private set; }

    /// <summary>
    /// 代理服务状态
    /// </summary>
    public OnOffToggle ProxyServiceStatus { get; private set; }

    public object? App { private get; set; }

    /// <summary>
    /// 当前读取的应用配置
    /// </summary>
    public Action<IServiceCollection>? Configuration { get; private set; }

    /// <summary>
    /// 是否存在无效的配置项
    /// </summary>
    public bool InvalidConfiguration => InvalidConfigurationFileNames.Any();

    /// <summary>
    /// 存在无效的配置项的文件名，有可能存在无效配置，但没有识别到无效的配置文件名
    /// </summary>
    public HashSet<string> InvalidConfigurationFileNames { get; private set; } = new();

    /// <summary>
    /// 存在重复的插件列表，无重复则为 <see langword="null"/>
    /// <para>string = 重复的 Id 或者 UniqueEnglishName</para>
    /// </summary>
    public Dictionary<PluginRepetitionType, Dictionary<string, IPlugin>>? RepetitivePlugins { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async ValueTask DisposeAppAsync()
    {
        var app = App;
        if (app == null)
        {
            return;
        }
        else if (app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
            return;
        }
        else if (app is IDisposable disposable)
        {
            disposable.Dispose();
            return;
        }
    }

    /// <inheritdoc cref="AppServicesLevel.AppUpdateAndTrayIcon"/>
    public bool HasTrayIcon { get; set; }

    /// <inheritdoc cref="AppServicesLevel.UI"/>
    public bool HasUI { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.ServerApiClient"/>
    public bool HasServerApiClient { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.Repositories"/>
    public bool HasRepositories { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.HttpClientFactory"/>
    public bool HasHttpClientFactory { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.HttpProxy"/>
    public bool HasHttpProxy { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.Hosts"/>
    public bool HasHosts { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.Steam"/>
    public bool HasSteam { get; private set; }

    /// <inheritdoc cref="AppServicesLevel.IPCRoot"/>
    public bool HasIPCRoot { get; private set; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    /// <summary>
    /// 是否已加载插件
    /// </summary>
    public bool HasPlugins { get; private set; }

    /// <summary>
    /// 当前加载的插件集合
    /// </summary>
    IReadOnlyCollection<IPlugin>? plugins;

    /// <summary>
    /// 当前所有的插件集合，包含禁用的插件
    /// </summary>
    IReadOnlyCollection<PluginResult<IPlugin>>? pluginResults;
#endif

    /// <summary>
    /// 尝试获取已加载的插件集合
    /// </summary>
    /// <param name="plugins"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetPlugins([NotNullWhen(true)] out IReadOnlyCollection<IPlugin>? plugins)
    {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        plugins = this.plugins;
        return HasPlugins;
#else
        plugins = default;
        return default;
#endif
    }

    /// <summary>
    /// 尝试获取所有的插件集合
    /// </summary>
    /// <param name="plugins"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetPluginResults([NotNullWhen(true)] out IReadOnlyCollection<PluginResult<IPlugin>>? pluginResults)
    {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        pluginResults = this.pluginResults;
        return HasPlugins;
#else
        pluginResults = default;
        return default;
#endif
    }

    readonly TaskCompletionSource waitConfiguredServices = new();

    public Task WaitConfiguredServices => waitConfiguredServices.Task;

    static readonly Lazy<string?> _NativeLibraryPath = new(() =>
    {
#if WINDOWS || LINUX
        return $"{GlobalDllImportResolver.GetLibraryPath(null)};{GlobalDllImportResolver.GetLibraryPath(null, IOPath.AppDataDirectory)}";
#endif
#if MACOS
        return AppContext.BaseDirectory;
#endif
        return null;
    });

    /// <summary>
    /// 自定义本机库加载路径
    /// </summary>
    public static string? NativeLibraryPath => _NativeLibraryPath.Value;
}
