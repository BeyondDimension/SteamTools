using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 反向代理服务
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public partial interface IReverseProxyService : IDisposable, IReverseProxySettings
{
    /// <summary>
    /// 反向代理的常量与配置项，在插件中由 IPC 远程调用给子进程
    /// </summary>
    static class Constants
    {
        public static IReverseProxyService Instance => Ioc.Get<IReverseProxyService>();

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

    /// <summary>
    /// 检查端口号是否被使用
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    bool PortInUse(int port) => SocketHelper.IsUsePort(ProxyIp, port);

    /// <summary>
    /// 启动代理服务
    /// </summary>
    /// <returns></returns>
    Task<StartProxyResult> StartProxyAsync();

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
    IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

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
    int ProxyPort { get; set; }

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
    int Socks5ProxyPortId { get; set; }

    #endregion

    #region TwoLevelAgent(二级代理)

    bool TwoLevelAgentEnable { get; set; }

    ExternalProxyType TwoLevelAgentProxyType { get; set; }

    string? TwoLevelAgentIp { get; set; }

    int TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    #endregion

    IPAddress? ProxyDNS { get; set; }
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
    /// 成功
    /// </summary>
    Ok = 121,

    /// <summary>
    /// 证书安装失败，或未信任
    /// </summary>
    SetupRootCertificateFail,

    /// <summary>
    /// 出现未处理的异常
    /// </summary>
    Exception,
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