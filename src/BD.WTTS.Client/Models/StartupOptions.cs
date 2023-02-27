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

    public StartupOptions(AppServicesLevel level, bool isTrace)
    {
        IsTrace = isTrace;
        mValue = this;
        HasMainProcessRequired = level.HasFlag(AppServicesLevel.MainProcessRequired);
        HasNotifyIcon = HasMainProcessRequired;
        HasGUI = level.HasFlag(AppServicesLevel.GUI);
        HasServerApiClient = level.HasFlag(AppServicesLevel.ServerApiClient);
        HasSteam = level.HasFlag(AppServicesLevel.Steam);
        HasHttpClientFactory = HasSteam || level.HasFlag(AppServicesLevel.HttpClientFactory);
        HasHttpProxy = level.HasFlag(AppServicesLevel.HttpProxy);
        HasHosts = level.HasFlag(AppServicesLevel.Hosts);
    }

    static StartupOptions? mValue;

    public static StartupOptions Value => mValue ?? throw new NullReferenceException("StartupOptions init fail.");
}