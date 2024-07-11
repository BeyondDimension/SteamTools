using STUN.Client;
using STUN.StunResult;
using System.Net.NetworkInformation;

namespace BD.WTTS.Services.Implementation;

internal partial class NetworkTestService : INetworkTestService
{
    #region 常量

    // 默认测试地址
    const string DEFAULT_TESTSERVER_HOSTNAME = "stun.syncthing.net";

    const int DEFAULT_PORT = 3478;
    const int DEFAULT_TLSPORT = 5349;

    // 默认端口号
    const int DEFAULT_TESTSTUN3489_PORT = DEFAULT_PORT;

    // 默认端口号
    const int DEFAULT_TESTSTUN5389_PORT = DEFAULT_PORT;

    #endregion 常量

    public NetworkTestService()
    {
    }

    #region Ping

    /// <summary>
    /// 将 Internet 控制消息协议 (ICMP) 回显请求消息发送到远程计算机，并等待来自该计算机的 ICMP 回送答复消息。 有关 ICMP 消息的详细说明，请参阅 RFC 792，可从 https://www.ietf.org获取。
    /// </summary>
    /// <param name="testHostNameOrAddress"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PingReply> TestPingAsync(string testHostNameOrAddress, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using Ping p = new Ping();

        var reply = await p.SendPingAsync(
                testHostNameOrAddress,
                timeout,
                cancellationToken: cancellationToken
            );

        return reply;
    }

    #endregion Ping

    #region Upload/Download Speed Test

    public async Task<(bool success, double? rate)> TestUploadSpeedAsync(
            string uploadServerUrl,
            byte[] uploadBytes,
            CancellationToken cancellationToken = default
        )
    {
        using HttpClient testClient = new HttpClient();

        return await TestUploadSpeedCoreAsync(
                () => testClient,
                BuildUploadContent,
                uploadServerUrl,
                uploadBytes,
                cancellationToken
            );

        static MultipartFormDataContent BuildUploadContent(byte[] data)
        {
            MultipartFormDataContent cotent = new MultipartFormDataContent();

            cotent.Add(new ByteArrayContent(data));

            return cotent;
        }
    }

    public async Task<(bool success, double? rate)> TestDownloadSpeedAsync(string downloadUrl, CancellationToken cancellationToken = default)
    {
        using HttpClient testClient = new HttpClient();

        return await TestDownloadSpeedCoreAsync(downloadUrl, () => testClient);
    }

    /// <summary>
    /// 测试上传速度
    /// </summary>
    /// <param name="httpClientFunc">自定义用于发送 http 请求的 client </param>
    /// <param name="uploadContentFunc">自定义上传内容类型</param>
    /// <param name="uploadServerUrl"></param>
    /// <param name="uploadBytes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<(bool success, double? rate)> TestUploadSpeedCoreAsync(
                Func<HttpClient> httpClientFunc,
                Func<byte[], HttpContent> uploadContentFunc,
                string uploadServerUrl,
                byte[] uploadBytes,
                CancellationToken cancellationToken = default
            )
    {
        if (string.IsNullOrWhiteSpace(uploadServerUrl))
            return (false, null);

        var uploadContent = uploadContentFunc(uploadBytes);
        try
        {
            var httpClient = httpClientFunc();

            Stopwatch watch = Stopwatch.StartNew();

            var resp = await httpClient.PostAsync(uploadServerUrl, uploadContent, cancellationToken);

            resp.EnsureSuccessStatusCode();

            watch.Stop();

            return (true, GetRate(uploadBytes.Length, watch.Elapsed.TotalSeconds));
        }
        catch (Exception ex)
        {
            Log.Error(nameof(NetworkTestService), ex, "测试上传异常");
        }

        return (false, null);
    }

    /// <summary>
    /// 测试下载速度
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpClientFunc"></param>
    /// <returns></returns>
    static async Task<(bool success, double? rate)> TestDownloadSpeedCoreAsync(string url, Func<HttpClient>? httpClientFunc = null, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFunc?.Invoke() ?? new HttpClient();

        try
        {
            Stopwatch watch = Stopwatch.StartNew();

            var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            resp.EnsureSuccessStatusCode();

            var bytesArray = await resp.Content.ReadAsByteArrayAsync(cancellationToken);

            watch.Stop();

            return (true, GetRate(bytesArray.Length, watch.Elapsed.TotalSeconds));
        }
        catch (Exception ex)
        {
            Log.Error(nameof(NetworkTestService), ex, "测试下载异常");
            return (false, null);
        }
        finally
        {
            if (httpClientFunc == null)
                httpClient.Dispose();
        }
    }

    static double GetRate(long byteArrayLength, double seconds)
    {
        if (seconds == 0)
            throw new ArgumentException("时间不能小于0", nameof(seconds));

        long bits = byteArrayLength * 8;
        double megabits = bits / 1_000_000.0; // 将比特转换为兆比特

        return megabits / seconds; // 返回速率，单位是 Mbps
    }

    #endregion Upload/Download Speed Test

    #region STUN 测试

    #region RFC3489

    StunClient3489? _stunClient;

    /// <summary>
    /// 测试 STUN 客户端 基于 RFC3489
    /// </summary>
    /// <param name="testServerHostName">测试服务地址</param>
    /// <param name="testServerPort">测试服务端口</param>
    /// <param name="localIPEndPoint">本机 IP </param>
    /// <param name="force">强制刷新</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ClassicStunResult?> TestStunClient3489Async(
            string? testServerHostName = DEFAULT_TESTSERVER_HOSTNAME,
            int? testServerPort = DEFAULT_TESTSTUN3489_PORT,
            IPEndPoint? localIPEndPoint = null,
            bool force = false,
            CancellationToken cancellationToken = default
        )
    {
        testServerHostName ??= DEFAULT_TESTSERVER_HOSTNAME;
        testServerPort ??= DEFAULT_TESTSTUN3489_PORT;

        if (_stunClient == null || force)
            _stunClient = await GetStunClient3489Async(testServerHostName, testServerPort.Value, localIPEndPoint);

        try
        {
            await _stunClient.QueryAsync(cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Log.Error(nameof(NetworkTestViewModel), ex, "TestStunClient3489Async 异常");
            return null;
        }

        return _stunClient.State;
    }

    private async ValueTask<StunClient3489> GetStunClient3489Async(string serverHostName, int port, IPEndPoint? localIPEndPoint = null)
    {
        IPEndPoint serverEndpoint = await GetServerIPEndPointAsync(serverHostName, port);

        IPEndPoint localEndpoint = localIPEndPoint ?? new IPEndPoint(IPAddress.Any, default);

        var client = new StunClient3489(
             serverEndpoint,
             localEndpoint
             );

        return client;
    }

    private async Task<IPEndPoint> GetServerIPEndPointAsync(string hostNameOrAddress, int port)
    {
        var serverIps = await Dns.GetHostAddressesAsync(hostNameOrAddress);

        IPEndPoint serverEndpoint = new IPEndPoint(serverIps.First(), port);

        return serverEndpoint;
    }

    #endregion RFC3489

    #region RFC5389

    /// <summary>
    /// 测试 STUN 客户端 基于 RFC5389
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="testServerHostName"></param>
    /// <param name="testServerPort"></param>
    /// <param name="localIPEndPoint"></param>
    /// <param name="force"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<StunResult5389?> TestStunClient5389Async(
            TransportProtocol protocol,
            string? testServerHostName = DEFAULT_TESTSERVER_HOSTNAME,
            int? testServerPort = DEFAULT_TESTSTUN5389_PORT,
            IPEndPoint? localIPEndPoint = null,
            CancellationToken cancellationToken = default
        )
    {
        testServerHostName ??= DEFAULT_TESTSERVER_HOSTNAME;
        testServerPort ??= DEFAULT_TESTSTUN5389_PORT;

        IStunClient5389 client = await GetStunClient5389Async(protocol, testServerHostName, testServerPort.Value, localIPEndPoint);

        if (client == null)
            return null;

        try
        {
            await client.QueryAsync(cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Log.Error(nameof(NetworkTestService), ex, "TestStunClient5389Async 异常");
            return null;
        }

        return client.State;
    }

    private async ValueTask<IStunClient5389> GetStunClient5389Async(TransportProtocol protocol, string serverHostName, int port, IPEndPoint? localIPEndPoint = null)
    {
        IPEndPoint serverEndpoint = await GetServerIPEndPointAsync(serverHostName, port);

        IPEndPoint localEndpoint = localIPEndPoint ?? new IPEndPoint(IPAddress.Any, default);

        IStunClient5389 client = protocol == TransportProtocol.Tcp
            ? new StunClient5389TCP(serverEndpoint, localEndpoint)
            : new StunClient5389UDP(serverEndpoint, localEndpoint);

        return client;
    }

    #endregion RFC5389

    #endregion STUN 测试
}