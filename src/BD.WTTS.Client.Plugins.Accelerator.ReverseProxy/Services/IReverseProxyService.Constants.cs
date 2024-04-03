using dotnetCampus.Ipc.CompilerServices.Attributes;

#if APP_REVERSE_PROXY

using InstanceReverseProxyService = BD.WTTS.Services.Implementation.YarpReverseProxyServiceImpl;

#else
using InstanceReverseProxyService = BD.WTTS.Services.IReverseProxyService;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 反向代理服务
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public partial interface IReverseProxyService : IDisposable
{
    /// <summary>
    /// 反向代理的常量与配置项，在插件中由 IPC 远程调用给子进程
    /// </summary>
    static class Constants
    {
        internal static InstanceReverseProxyService Instance => Ioc.Get<InstanceReverseProxyService>();

        /// <inheritdoc cref="CertificateConstants.CertificateName"/>
        public const string CertificateName = CertificateConstants.CertificateName;

        /// <inheritdoc cref="CertificateConstants.RootCertificateName"/>
        public const string RootCertificateName = CertificateConstants.RootCertificateName;

        public const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

        public const string LocalDomain = "local.steampp.net";

        public static IPAddress DefaultProxyIp => IPAddress.Any;
    }

    /// <summary>
    /// 当前代理服务是否正在运行
    /// </summary>
    bool ProxyRunning { get; }

    /// <inheritdoc cref="IReverseProxySettings.Scripts"/>
    IReadOnlyCollection<ScriptIPCDTO>? Scripts { get; set; }

    /// <summary>
    /// 启动代理服务
    /// </summary>
    Task<StartProxyResult> StartProxyAsync(byte[] reverseProxySettings);

    /// <summary>
    /// 退出进程
    /// </summary>
    void Exit();

    /// <summary>
    /// 停止代理服务
    /// </summary>
    /// <returns></returns>
    Task StopProxyAsync();

#if DEBUG

    string GetDebugString()
    {
        return $"Pid: {Environment.ProcessId}, Exe: {Environment.ProcessPath}, Asm: {Assembly.GetAssembly(GetType())?.FullName}";
    }

    Task<StartProxyResult> GetDebugString2()
    {
        StartProxyResult r = new(StartProxyResultCode.Exception, new Exception("aaaa"));
        return Task.FromResult(r);
    }

#endif
}

/// <summary>
/// 反向代理服务的设置项
/// </summary>
public partial interface IReverseProxySettings
{
    /// <summary>
    /// 当前勾选的加速项目组
    /// </summary>
    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    /// <summary>
    /// 当前勾选的脚本集
    /// </summary>
    IReadOnlyCollection<ScriptIPCDTO>? Scripts { get; set; }

    /// <summary>
    /// 是否启用脚本
    /// </summary>
    bool IsEnableScript { get; set; }

    /// <summary>
    /// 是否只针对 Steam 内置浏览器启用脚本
    /// </summary>
    bool IsOnlyWorkSteamBrowser { get; set; }

    /// <summary>
    /// 代理服务器端口号
    /// </summary>
    ushort ProxyPort { get; set; }

    /// <summary>
    /// 代理服务器 IP 地址
    /// </summary>
    IPAddress ProxyIp { get; set; }

    /// <summary>
    /// 代理模式
    /// </summary>
    ProxyMode ProxyMode { get; set; }

    /// <summary>
    /// 启用 GOG 插件代理
    /// </summary>
    bool IsProxyGOG { get; set; }

    /// <summary>
    /// 开启加速后仅代理脚本而不加速
    /// </summary>
    bool OnlyEnableProxyScript { get; set; }

    /// <summary>
    /// 启用 Http 链接转发到 Https
    /// </summary>
    bool EnableHttpProxyToHttps { get; set; }

    #region Socks5

    /// <summary>
    /// Socks5 Enable
    /// </summary>
    bool Socks5ProxyEnable { get; set; }

    /// <summary>
    /// Socks5 监听端口
    /// </summary>
    ushort Socks5ProxyPortId { get; set; }

    #endregion Socks5

    #region TwoLevelAgent(二级代理)

    bool TwoLevelAgentEnable { get; set; }

    ExternalProxyType TwoLevelAgentProxyType { get; set; }

    string? TwoLevelAgentIp { get; set; }

    ushort TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    #endregion TwoLevelAgent(二级代理)

    IPAddress? ProxyDNS { get; set; }

    /// <summary>
    /// 是否支持 IPv6
    /// </summary>
    bool IsSupportIpv6 { get; set; }

    bool UseDoh { get; set; }

    string? CustomDohAddres { get; set; }

    /// <summary>
    /// 服务端代理令牌
    /// </summary>
    string? ServerSideProxyToken { get; set; }
}

[MP2Obj(SerializeLayout.Explicit)]
public readonly partial record struct ReverseProxySettings(
    [property: MP2Key(0)]
    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains,
    [property:MP2Key(1)]
    IReadOnlyCollection<ScriptIPCDTO>? Scripts,
    [property:MP2Key(2)]
    bool IsEnableScript,
    [property:MP2Key(3)]
    bool IsOnlyWorkSteamBrowser,
    [property:MP2Key(4)]
    ushort ProxyPort,
    [property:MP2Key(5)]
    string? ProxyIp,
    [property:MP2Key(6)]
    ProxyMode ProxyMode,
    [property:MP2Key(7)]
    bool IsProxyGOG,
    [property:MP2Key(8)]
    bool OnlyEnableProxyScript,
    [property:MP2Key(9)]
    bool EnableHttpProxyToHttps,
    [property:MP2Key(10)]
    bool Socks5ProxyEnable,
    [property:MP2Key(11)]
    ushort Socks5ProxyPortId,
    [property:MP2Key(12)]
    bool TwoLevelAgentEnable,
    [property:MP2Key(13)]
    ExternalProxyType TwoLevelAgentProxyType,
    [property:MP2Key(14)]
    string? TwoLevelAgentIp,
    [property:MP2Key(15)]
    ushort TwoLevelAgentPortId,
    [property:MP2Key(16)]
    string? TwoLevelAgentUserName,
    [property:MP2Key(17)]
    string? TwoLevelAgentPassword,
    [property:MP2Key(18)]
    string? ProxyDNS,
    [property:MP2Key(19)]
    bool IsSupportIpv6,
    [property:MP2Key(20)]
    bool UseDoh,
    [property:MP2Key(21)]
    string? CustomDohAddres,
    [property:MP2Key(22)]
    string? ServerSideProxyToken)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IPAddress GetProxyIp() => GetProxyIp(ProxyIp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IPAddress? GetProxyDNS()
    {
        var result = IPAddress2.TryParse(ProxyDNS, out var proxyDNS) ? proxyDNS : null;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetTwoLevelAgentIp(string? twoLevelAgentIp)
    {
        var result = IPAddress2.TryParse(twoLevelAgentIp, out var twoLevelAgentIp_) ?
            twoLevelAgentIp_.ToString() : IPAddress.Loopback.ToString();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IPAddress GetProxyIp(string? proxyIp)
    {
        var result = IPAddress2.TryParse(proxyIp, out var ip) ? ip : IPAddress.Any;
        return result;
    }

#if APP_REVERSE_PROXY
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetValue(IReverseProxySettings settings)
    {
        settings.ProxyDomains = ProxyDomains;
        settings.Scripts = Scripts;
        settings.IsEnableScript = IsEnableScript;
        settings.IsOnlyWorkSteamBrowser = IsOnlyWorkSteamBrowser;
        settings.ProxyPort = ProxyPort == default ? ReverseProxyServiceImpl.DefaultProxyPort : ProxyPort;
        settings.ProxyIp = GetProxyIp();
        settings.ProxyMode = ProxyMode;
        settings.IsProxyGOG = IsProxyGOG;
        settings.OnlyEnableProxyScript = OnlyEnableProxyScript;
        settings.EnableHttpProxyToHttps = EnableHttpProxyToHttps;
        settings.Socks5ProxyEnable = Socks5ProxyEnable;
        settings.Socks5ProxyPortId = Socks5ProxyPortId;
        settings.TwoLevelAgentEnable = TwoLevelAgentEnable;
        settings.TwoLevelAgentProxyType = TwoLevelAgentProxyType;
        settings.TwoLevelAgentIp = TwoLevelAgentIp;
        settings.TwoLevelAgentPortId = TwoLevelAgentPortId;
        settings.TwoLevelAgentUserName = TwoLevelAgentUserName;
        settings.TwoLevelAgentPassword = TwoLevelAgentPassword;
        settings.ProxyDNS = GetProxyDNS();
        settings.IsSupportIpv6 = IsSupportIpv6;
        settings.UseDoh = UseDoh;
        settings.CustomDohAddres = CustomDohAddres;
        settings.ServerSideProxyToken = ServerSideProxyToken;
    }
#endif
}

/// <summary>
/// 启动代理结果状态码
/// </summary>
public enum StartProxyResultCode : byte
{
    /// <summary>
    /// Ipc 调用失败时候将返回默认值，或显示设置的默认值
    /// </summary>
    IpcCallFailOrDefault = default,

    /// <summary>
    /// 生成证书失败
    /// </summary>
    GenerateCertificateFail,

    /// <summary>
    /// 生成 Cer 文件路径失败
    /// </summary>
    GenerateCerFilePathFail,

    /// <summary>
    /// 获取证书数据失败
    /// </summary>
    GetCertificatePackableFail,

    /// <summary>
    /// 信任证书失败
    /// </summary>
    TrustRootCertificateFail,

    GetX509Certificate2Fail,

    /// <summary>
    /// 成功
    /// </summary>
    Ok = 121,

    /// <summary>
    /// 证书安装或生成失败，或未信任
    /// </summary>
    [Obsolete("use GenerateCertificateFail/TrustRootCertificateFail")]
    SetupRootCertificateFail,

    /// <summary>
    /// 反序列化 <see cref="ReverseProxySettings"/> 失败
    /// </summary>
    DeserializeReverseProxySettingsFail,

    /// <summary>
    /// 出现未处理的异常
    /// </summary>
    Exception,

    /// <summary>
    /// 绑定端口错误
    /// </summary>
    BindPortError,
}

/// <summary>
/// 启动代理结果
/// </summary>
/// <param name="Code">状态码</param>
/// <param name="Exception">未处理的异常</param>
public readonly record struct StartProxyResult(StartProxyResultCode Code, Exception? Exception)
{
    public static implicit operator bool(StartProxyResult result) => result.Code == StartProxyResultCode.Ok;

    public static implicit operator StartProxyResult(StartProxyResultCode code) => new(code, default);

    public static implicit operator StartProxyResult(Exception? exception) => exception == null ? new(StartProxyResultCode.Ok, default) : new(StartProxyResultCode.Exception, exception);
}