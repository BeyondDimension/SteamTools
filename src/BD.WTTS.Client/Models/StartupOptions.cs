using BD.WTTS.Plugins;
using BD.WTTS.Plugins.Abstractions;

namespace BD.WTTS.Models;

public sealed class StartupOptions
{
    public bool HasMainProcessRequired { get; set; }

    public bool HasNotifyIcon { get; set; }

    public bool HasGUI { get; set; }

    public bool HasServerApiClient { get; set; }

    public bool HasHttpClientFactory { get; set; }

    public bool HasHttpProxy { get; set; }

    public bool HasHosts { get; set; }

    public bool HasSteam { get; set; }

    public bool IsTrace { get; set; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    public bool HasPlugins { get; }

    public HashSet<IPlugin>? Plugins { get; }
#endif

    public StartupOptions(AppServicesLevel level, bool isTrace)
    {
        IsTrace = isTrace;
        HasMainProcessRequired = level.HasFlag(AppServicesLevel.MainProcessRequired);
        HasNotifyIcon = HasMainProcessRequired;
        HasGUI = level.HasFlag(AppServicesLevel.GUI);
        HasServerApiClient = level.HasFlag(AppServicesLevel.ServerApiClient);
        HasSteam = level.HasFlag(AppServicesLevel.Steam);
        HasHttpClientFactory = HasSteam || level.HasFlag(AppServicesLevel.HttpClientFactory);
        HasHttpProxy = level.HasFlag(AppServicesLevel.HttpProxy);
        HasHosts = level.HasFlag(AppServicesLevel.Hosts);
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        Plugins = PluginsCore.InitPlugins();
        HasPlugins = Plugins != null;
#endif
        mValue = this;
    }

    static StartupOptions? mValue;

    public static StartupOptions Value => mValue ?? throw new NullReferenceException("StartupOptions init fail.");
}