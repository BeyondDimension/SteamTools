namespace BD.WTTS.Services.Implementation;

sealed class LazyReverseProxyServiceImpl : IReverseProxyService
{
    private LazyReverseProxyServiceImpl() { }

    public static IReverseProxyService Instance = new LazyReverseProxyServiceImpl();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE1006 // 命名样式
    static IReverseProxyService impl() => Ioc.Get<IReverseProxyService>();
#pragma warning restore IDE1006 // 命名样式

    public bool ProxyRunning => impl().ProxyRunning;

    public IReadOnlyCollection<ScriptIPCDTO>? Scripts
    {
        get => impl().Scripts;
        set => impl().Scripts = value;
    }

    public void Dispose() => impl().Dispose();

    public byte[]? GetFlowStatistics_Bytes() => impl().GetFlowStatistics_Bytes();

    public string? GetLogAllMessage() => impl().GetLogAllMessage();

    public async Task<StartProxyResult> StartProxyAsync(byte[] reverseProxySettings)
    {
        var result = await impl().StartProxyAsync(reverseProxySettings);
        return result;
    }

    public void Exit()
    {
        impl().Exit();
    }

    public async Task StopProxyAsync()
    {
        await impl().StopProxyAsync();
    }

    public bool WirtePemCertificateToGoGSteamPlugins() => impl().WirtePemCertificateToGoGSteamPlugins();
}