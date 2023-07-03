namespace BD.WTTS.Services.Implementation;

sealed class LazyReverseProxyServiceImpl : IReverseProxyService
{
    public static IReverseProxyService Instance = new LazyReverseProxyServiceImpl();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE1006 // 命名样式
    static IReverseProxyService instance() => Ioc.Get<IReverseProxyService>();
#pragma warning restore IDE1006 // 命名样式

    public bool ProxyRunning => instance().ProxyRunning;

    public IReadOnlyCollection<ScriptDTO>? Scripts
    {
        get => instance().Scripts;
        set => instance().Scripts = value;
    }

    public void Dispose() => instance().Dispose();

    public FlowStatistics? GetFlowStatistics() => instance().GetFlowStatistics();

    public async Task<StartProxyResult> StartProxyAsync(byte[] reverseProxySettings)
    {
        var result = await instance().StartProxyAsync(reverseProxySettings);
        return result;
    }

    public async Task StopProxyAsync()
    {
        await instance().StopProxyAsync();
    }

    public bool WirtePemCertificateToGoGSteamPlugins() => instance().WirtePemCertificateToGoGSteamPlugins();
}