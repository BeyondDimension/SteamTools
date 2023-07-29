// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/GlobalListener.cs

using System.Net.NetworkInformation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models.Abstractions;

partial interface IReverseProxyConfig
{
    private static readonly IPGlobalProperties global = IPGlobalProperties.GetIPGlobalProperties();
    private static readonly HashSet<int> tcpListenPorts = GetListenPorts(global.GetActiveTcpListeners);
    private static readonly HashSet<int> udpListenPorts = GetListenPorts(global.GetActiveUdpListeners);

    const int SshPortDefault = 22;

    /// <summary>
    /// SSH 端口
    /// </summary>
    static int SshPort { get; } = GetAvailableTcpPort(SshPortDefault);

    const int GitHubDesktopPort = 9418;

    /// <summary>
    /// Git 端口
    /// </summary>
    static int GitPort { get; } = GetAvailableTcpPort(GitHubDesktopPort);

    const int HttpPortDefault = 80;

    /// <summary>
    /// Http 端口
    /// </summary>
    static int HttpPort { get; } = GetAvailableTcpPort(HttpPortDefault);

    const int HttpsPortDefault = 443;

    /// <summary>
    /// Https 端口
    /// </summary>
    static int HttpsPort { get; } = GetAvailableTcpPort(HttpsPortDefault);

    /// <summary>
    /// 获取已监听的端口
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    private static HashSet<int> GetListenPorts(Func<IPEndPoint[]> func)
    {
        var hashSet = new HashSet<int>();
        try
        {
            foreach (var endpoint in func())
            {
                hashSet.Add(endpoint.Port);
            }
        }
        catch (Exception)
        {
        }
        return hashSet;
    }

    /// <summary>
    /// 是否可用的 TCP 端口
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public static bool IsAvailableTcp(int port) => tcpListenPorts.Contains(port) == false;

    /// <summary>
    /// 是否可用的 UDP 端口
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public static bool IsAvailableUdp(int port) => udpListenPorts.Contains(port) == false;

    /// <summary>
    /// 是否可用的 TCP 和 UDP 端口
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public static bool IsAvailableTcpAndUdp(int port) => IsAvailableTcp(port) && IsAvailableUdp(port);

    /// <summary>
    /// 获取一个随机的可用的 TCP 端口
    /// </summary>
    /// <param name="minPort"></param>
    /// <returns></returns>
    public static int GetAvailableTcpPort(int minPort) => GetAvailablePort(IsAvailableTcp, minPort);

    /// <summary>
    /// 获取一个随机的可用的 UDP 端口 
    /// </summary>
    /// <param name="minPort"></param>
    /// <returns></returns>
    public static int GetAvailableUdpPort(int minPort) => GetAvailablePort(IsAvailableUdp, minPort);

    /// <summary>
    /// 获取一个随机的可用的 TCP 和 UDP 端口 
    /// </summary>
    /// <param name="minPort"></param>
    /// <returns></returns>
    public static int GetAvailableTcpAndUdpPort(int minPort) => GetAvailablePort(IsAvailableTcpAndUdp, minPort);

    static int GetAvailablePort(Func<int, bool> isAvailable, int minPort)
    {
        for (var port = minPort; port < IPEndPoint.MaxPort; port++)
        {
            if (isAvailable(port) == true)
            {
                return port;
            }
        }
        throw new ApplicationException("Failed to get available ports. There are no available ports.");
    }
}