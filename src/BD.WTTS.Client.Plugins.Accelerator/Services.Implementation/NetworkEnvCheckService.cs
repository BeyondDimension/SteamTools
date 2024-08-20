using System.Net.NetworkInformation;

namespace BD.WTTS.Services.Implementation;

public class NetworkEnvCheckService
{
    const string HOST_FILE_PATH =
#if WINDOWS
     @"C:\Windows\System32\drivers\etc\hosts";
#else
     "/etc/hosts";
#endif

    public async Task CheckNetworkEnvAsync(IProgress<(NetworkEnvCheckStep Step, bool HasError, string Msg)> progress, CancellationToken cancellationToken = default)
    {
        await Parallel.ForAsync(
               (int)NetworkEnvCheckStep.HostsFileCheck,
               (int)NetworkEnvCheckStep.LSPCheck + 1,
               cancellationToken,
               async (step, ct) =>
               {
                   (bool pass, string msg) = step switch
                   {
                       (int)NetworkEnvCheckStep.HostsFileCheck => await CheckHostsAsync(),
                       (int)NetworkEnvCheckStep.NetworkInterfaceCheck => CheckNetworkInterfaces(),
                       (int)NetworkEnvCheckStep.LSPCheck =>
#if WINDOWS
                            CheckLSP(HandleWSAProtocolInfo),
#else
                           (true, string.Empty),
#endif
                       _ => (true, string.Empty)
                   };

                   progress.Report(((NetworkEnvCheckStep)step, pass, msg));
               }
           ).ConfigureAwait(false);
    }

    private static Task<(bool Pass, string Msg)> CheckHostsAsync()
    {
        bool hostFileExist = File.Exists(HOST_FILE_PATH);

        if (!hostFileExist)
            return Task.FromResult((false, "未找到 hosts 文件"));

        return Task.FromResult((hostFileExist, string.Empty));
    }

    private static (bool Pass, string ErrorMsg) CheckNetworkInterfaces(NetworkInterface[]? interfaces = null)
    {
        interfaces ??= NetworkInterface.GetAllNetworkInterfaces();

        if (interfaces == null || interfaces.Length == 0)
            return (false, "未获取到网络适配器信息!");

        // 如何判断虚拟网卡?

        var useableNetworkInterfaces =
            interfaces.Where(x =>
                !x.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                (x.NetworkInterfaceType == NetworkInterfaceType.Ethernet || x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));

        if (!useableNetworkInterfaces.Any())
            return (false, "未获取到可用的以太网,无线网络适配器");

        var checkResults =
            useableNetworkInterfaces.Select(x => new
            {
                Source = x,
                Result = CheckNetworkInterface(x)
            });

        if (checkResults.Any(x => x.Result.Pass))
            return (true, string.Empty);

        var firstError = checkResults.First(x => !x.Result.Pass);

        return (firstError.Result.Pass, $"{firstError.Source.Name}|{firstError.Source.Description} {firstError.Result.ErrorMsg}");
    }

    private static (bool Pass, string ErrorMsg) CheckNetworkInterface(NetworkInterface @interface)
    {
        var name = @interface.Name;
        var desc = @interface.Description;

        var ipProps = @interface.GetIPProperties();

        if (@interface.OperationalStatus != OperationalStatus.Up)
            return (false, "未启用网卡");

        if (ipProps == null)
            return (false, $"获取网络信息失败!");

        bool hasIPv4 = ipProps.UnicastAddresses
            .Any(x => x.Address.AddressFamily == AddressFamily.InterNetwork);

        bool hasIPv6 = ipProps.UnicastAddresses
            .Any(x => x.Address.AddressFamily == AddressFamily.InterNetworkV6);

        if (!hasIPv4 && !hasIPv6)
            return (false, "网络适配器未启用TCP/IP服务");

        // 如何判断虚拟网卡?

        var dns = ipProps.DnsAddresses;

#if WINDOWS
        var isDHCPEnable = ipProps.GetIPv4Properties().IsDhcpEnabled;
#endif
        return (true, string.Empty);
    }

    private static (bool Pass, string ErrorMsg) CheckLSP(CheckLSPPredicate predicate)
    {
        int bufferSize = 0;
        int error = 0;

        // 获取 buffer 的大小
        _ = WSCEnumProtocols(null, IntPtr.Zero, ref bufferSize, ref error);

        IntPtr protocolBuffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            int result = WSCEnumProtocols(null, protocolBuffer, ref bufferSize, ref error);
            if (result == -1)
            {
                return (false, "获取LSP信息失败!");
            }

            int structSize = Marshal.SizeOf(typeof(WSAPROTOCOL_INFO));
            int count = bufferSize / structSize;

            for (int i = 0; i < count; i++)
            {
                IntPtr item = new(protocolBuffer + (i * structSize));

                WSAPROTOCOL_INFO protocolInfo = Marshal.PtrToStructure<WSAPROTOCOL_INFO>(item);

#if DEBUG
                // Console.WriteLine($"Protocol: {protocolInfo.szProtocol}");
                // Console.WriteLine($"Address Family: {protocolInfo.iAddressFamily}");
                // Console.WriteLine($"Socket Type: {protocolInfo.iSocketType}");
                // Console.WriteLine($"Protocol: {protocolInfo.iProtocol}");
                // Console.WriteLine($"Provider ID: {protocolInfo.ProviderId}");
                // Console.WriteLine();
#endif
                var handle = predicate(in protocolInfo);

                if (!handle)
                    return (false, "LSP响应异常!");
            }

            return (true, string.Empty);
        }
        finally
        {
            Marshal.FreeHGlobal(protocolBuffer);
        }
    }

    #region P/Invoke

    /// <summary>
    /// 处理判断 协议信息是否有效
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    static bool HandleWSAProtocolInfo(in WSAPROTOCOL_INFO info)
    {
        const int MAX = 255;

        StringBuilder pathBuilder = new StringBuilder(MAX);
        int pathLength = MAX;

        Guid queryProviderId = info.ProviderId;

        int result = WSCGetProviderPath(ref queryProviderId, pathBuilder, ref pathLength, out int errorCode);

        if (result != 0)
        {
            // 获取失败
        }

        // path 可能包含系统环境变量例如 %SystemRoot%
        // 是否需要解析
        string path = pathBuilder.ToString();

        Console.WriteLine("path:" + path);
        return true;
    }

    [DllImport("Ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int WSCEnumProtocols(
       int[]? lpiProtocols,
       IntPtr lpProtocolBuffer,
       ref int lpdwBufferLength,
       ref int lpErrno);

    [DllImport("Ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int WSCGetProviderPath(
        [In] ref Guid providerId,
        [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder providerDllPath,
        [In, Out] ref int providerDllPathLength,
        [Out] out int lpErrno
        );

    private delegate bool CheckLSPPredicate(in WSAPROTOCOL_INFO protocolInfo);

    /// <summary>
    /// WSAPROTOCOL_INFO 结构用于存储或检索给定协议的完整信息。
    /// <see cref="https://learn.microsoft.com/zh-cn/windows/win32/api/winsock2/ns-winsock2-wsaprotocol_infoa"/>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "<挂起>")]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WSAPROTOCOL_INFO
    {
        /// <summary>
        /// 描述协议提供的服务的位掩码
        /// </summary>
        public uint dwServiceFlags1;

        /// <summary>
        /// 保留给其他协议属性定义。
        /// </summary>
        public uint dwServiceFlags2;

        /// <summary>
        /// 保留给其他协议属性定义。
        /// </summary>
        public uint dwServiceFlags3;

        /// <summary>
        /// 保留给其他协议属性定义。
        /// </summary>
        public uint dwServiceFlags4;

        /// <summary>
        /// 一组标志，提供有关如何在 Winsock 目录中表示此协议的信息
        /// </summary>
        public uint dwProviderFlags;

        /// <summary>
        /// 服务提供商供应商) 分配给提供程序的 GUID
        /// </summary>
        public Guid ProviderId;

        /// <summary>
        /// 由 WS2_32.DLL 为每个WSAPROTOCOL_INFO结构分配的唯 一 标识符。
        /// </summary>
        public uint dwCatalogEntryId;

        public WSAPROTOCOLCHAIN ProtocolChain;

        /// <summary>
        /// 协议版本标识符。
        /// </summary>
        public int iVersion;

        /// <summary>
        /// 要作为地址系列参数传递给 套接字 或 WSASocket 函数的
        /// </summary>
        public int iAddressFamily;

        /// <summary>
        /// 最大地址长度（以字节为单位）。
        /// </summary>
        public int iMaxSockAddr;

        /// <summary>
        /// 最小地址长度（以字节为单位）。
        /// </summary>
        public int iMinSockAddr;

        public int iSocketType;
        public int iProtocol;
        public int iProtocolMaxOffset;

        /// <summary>
        /// 目前，这些值是 BIGENDIAN 和 LITTLEENDIAN) (清单常量，分别指示值为 0 和 1 的 big-endian 或 little-endian
        /// </summary>
        public int iNetworkByteOrder;

        public int iSecurityScheme;

        /// <summary>
        /// 协议支持的最大消息大小（以字节为单位）
        /// </summary>
        public uint dwMessageSize;

        public uint dwProviderReserved;

        /// <summary>
        /// 一个字符数组，其中包含标识协议的可读名称，例如“MSAFD Tcpip [UDP/IP]”。 允许的最大字符数为 WSAPROTOCOL_LEN，定义为 255。
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szProtocol;
    }

    /// <summary>
    /// WSAPROTOCOLCHAIN 结构包含构成协议链的目录条目标识符的计数列表。
    /// <see cref="https://learn.microsoft.com/zh-cn/windows/win32/api/winsock2/ns-winsock2-wsaprotocolchain"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WSAPROTOCOLCHAIN
    {
        /// <summary>
        /// 链的长度（以字节为单位）
        /// 将 ChainLen 设置为零表示分层协议
        /// 将 ChainLen 设置为 1 指示基本协议
        /// 将 ChainLen 设置为大于 1 表示协议链
        /// ChainEntries[MAX_PROTOCOL_CHAIN]
        /// </summary>
        public int ChainLen;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public uint[] ChainEntries;
    }

    #endregion P/Invoke
}