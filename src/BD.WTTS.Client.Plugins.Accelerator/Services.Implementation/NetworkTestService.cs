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

    #region Open/Upload/Download Speed Test

    public async Task<(bool Success, long? DelayMs)> TestOpenUrlAsync(string url, Func<HttpClient>? httpClientFunc = null, CancellationToken cancellationToken = default)
    {
        using var testClient = httpClientFunc?.Invoke() ?? new HttpClient();

        Stopwatch watch = Stopwatch.StartNew();

        try
        {
            var resp = await testClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken);

            watch.Stop();

            return (true, watch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(NetworkTestService), ex, "测试打开 URL 异常");
            return (false, default);
        }
    }

    public async Task<(bool Success, double? Rate)> TestUploadSpeedAsync(
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
            MultipartFormDataContent content = new MultipartFormDataContent();

            content.Add(new ByteArrayContent(data));

            return content;
        }
    }

    public async Task<(bool Success, double? Rate)> TestDownloadSpeedAsync(string downloadUrl, CancellationToken cancellationToken = default)
    {
        using HttpClient testClient = new HttpClient();

        return await TestDownloadSpeedCoreAsync(downloadUrl, () => testClient, cancellationToken);
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
    static async Task<(bool Success, double? Rate)> TestUploadSpeedCoreAsync(
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
    static async Task<(bool Success, double? Rate)> TestDownloadSpeedCoreAsync(string url, Func<HttpClient>? httpClientFunc = null, CancellationToken cancellationToken = default)
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

    #endregion Open/Upload/Download Speed Test

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
            Log.Error(nameof(NetworkTestService), ex, "TestStunClient3489Async 异常");
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

    #region DNS 查询

    /// <summary>
    /// 测试 DNS UDP
    /// </summary>
    /// <param name="testDomain">要测试的域名</param>
    /// <param name="dnsServerIp">DNS 服务地址</param>
    /// <param name="dnsServerPort">DNS 服务端口</param>
    /// <param name="dnsRecordType"> dns 测试类型 </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(long DelayMs, IPAddress[] Address)> TestDNSAsync(
            string testDomain,
            string dnsServerIp,
            int dnsServerPort,
            DnsQueryAnswerRecord.DnsRecordType dnsRecordType = DnsQueryAnswerRecord.DnsRecordType.A,
            CancellationToken cancellationToken = default
        )
    {
        using UdpClient udpClient = new UdpClient(dnsServerIp, dnsServerPort);

        var query = new DnsQueryUdpRequest(testDomain, dnsRecordType);

        Stopwatch watch = Stopwatch.StartNew();

        await udpClient.SendAsync(query, cancellationToken);

        var receivedData = await udpClient.ReceiveAsync(cancellationToken);

        watch.Stop();

        var dnsQueryResponse = new DnsQueryUdpResponse(receivedData.Buffer);

        return (watch.ElapsedMilliseconds, dnsQueryResponse.GetAddresses().ToArray());
    }

    /// <summary>
    /// 测试 DNS Over Https
    /// </summary>
    /// <param name="testDomain"></param>
    /// <param name="dohServer"></param>
    /// <param name="dnsRecordType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(long DelayMs, IPAddress[] Address)> TestDNSOverHttpsAsync(
        string testDomain,
        string dohServer,
        DnsQueryAnswerRecord.DnsRecordType dnsRecordType = DnsQueryAnswerRecord.DnsRecordType.A,
        CancellationToken cancellationToken = default)
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            UseProxy = false,
            Proxy = HttpNoProxy.Instance,
        };
        using HttpClient client = new HttpClient(handler);

        string queryUrl = $"{dohServer}?name={testDomain}&type={dnsRecordType}";

        Stopwatch watch = Stopwatch.StartNew();

        HttpResponseMessage? resp = null;
        try
        {
            resp = await client.GetAsync(queryUrl, HttpCompletionOption.ResponseContentRead, cancellationToken);

            resp.EnsureSuccessStatusCode();
        }
        finally
        {
            watch.Stop();
        }

        var queryResp = await resp.Content.ReadFromJsonAsync<DnsQueryJsonResponse>();

        return (watch.ElapsedMilliseconds, queryResp?.GetAddresses().ToArray() ?? Array.Empty<IPAddress>());
    }

    #region DNS 解析定义

    internal interface IDnsQueryResponse
    {
        List<DnsQueryAnswerRecord> Answers { get; }

        IEnumerable<IPAddress> GetAddresses();
    }

    internal record DnsQueryUdpResponse : DnsQueryUdpRequest, IDnsQueryResponse
    {
        public ushort AnswerCount { get; init; } // (ushort)((_origin[6] << 8) | _origin[7]);

        public ushort AuthorityCount { get; init; } // (ushort)((_origin[8] << 8) | _origin[9]);

        public ushort AdditionalCount { get; init; } // (ushort)((_origin[10] << 8) | _origin[11]);

        public List<DnsQueryAnswerRecord> Answers { get; init; }

        private readonly byte[] _origin;

        public DnsQueryUdpResponse(byte[] origin) : base(origin)
        {
            _origin = origin;

            AnswerCount = (ushort)((origin[Position++] << 8) | origin[Position++]);
            AuthorityCount = (ushort)((origin[Position++] << 8) | origin[Position++]);
            AdditionalCount = (ushort)((origin[Position++] << 8) | origin[Position++]);

            //  跳过查询问题部分
            for (int i = 0; i < QuestionCount; i++)
            {
                Position = SkipName(origin, Position);
                Position += 4; // 跳过类型
            }

            Answers = new List<DnsQueryAnswerRecord>();

            Position = ParseRecords(origin, Position, AnswerCount, Answers);
            Position = ParseRecords(origin, Position, AuthorityCount, Answers);
            Position = ParseRecords(origin, Position, AdditionalCount, Answers);
        }

        private static int ParseRecords(byte[] response, int position, int recordCount, List<DnsQueryAnswerRecord> answers)
        {
            for (int i = 0; i < recordCount; i++)
            {
                position = ParseName(response, position, out var parsedName);

                int @type = (response[position++] << 8) | response[position++];
                int @class = (response[position++] << 8) | response[position++];
                long ttl = (response[position++] << 24) | (response[position++] << 16) | (response[position++] << 8) | response[position++];
                int dataLength = (response[position++] << 8) | response[position++];

                if (@type == (int)DnsQueryAnswerRecord.DnsRecordType.A && dataLength == 4) // Type A (IPv4)
                {
                    var ipAddress = new byte[4];
                    Array.Copy(response, position, ipAddress, 0, 4);

                    answers.Add(
                            new DnsQueryAnswerRecord.A(
                                    parsedName!,
                                    (DnsQueryAnswerRecord.DnsRecordType)@type,
                                    ttl,
                                    new IPAddress(ipAddress)
                                )
                        );
                }
                else if (@type == (int)DnsQueryAnswerRecord.DnsRecordType.AAAA && dataLength == 16) // Type AAAA (IPv6)
                {
                    var ipAddress = new byte[16];
                    Array.Copy(response, position, ipAddress, 0, 16);

                    answers.Add(
                          new DnsQueryAnswerRecord.AAAA(
                                  parsedName!,
                                  (DnsQueryAnswerRecord.DnsRecordType)@type,
                                  ttl,
                                  new IPAddress(ipAddress)
                              )
                      );
                }
                else if (type == (int)DnsQueryAnswerRecord.DnsRecordType.CNAME)
                {
                    ParseName(response, position, out var parsedCName);

                    if (string.IsNullOrEmpty(parsedCName))
                    {
                        Log.Error(nameof(NetworkTestService), $"解析 DNS CName字段异常 name :{parsedName}, data length :{dataLength}");
                        continue;
                    }

                    answers.Add(
                            new DnsQueryAnswerRecord.CName(
                                  parsedName!,
                                  (DnsQueryAnswerRecord.DnsRecordType)@type,
                                  ttl,
                                  parsedCName!
                                )
                            );
                }
                else
                {
                    Log.Error(nameof(NetworkTestService), $"未支持的 DNS响应记录 type :{@type} ,class :{@class} name :{parsedName}, data length :{dataLength}");
                    continue;
                }

                position += dataLength;
            }

            return position;
        }

        private static int ParseName(byte[] response, int position, out string? name)
        {
            StringBuilder nameBuilder = new StringBuilder();
            int originalPosition = position;
            bool isPointer = false;

            // 0表示结束
            while (response[position] != 0)
            {
                // dns 协议 名称可能是 指针 或者 直接表示
                // 如果大于等于 192 表示是指针开始
                if (response[position] >= 192)
                {
                    if (!isPointer)
                    {
                        originalPosition = position + 2;
                        isPointer = true;
                    }
                    // 因为 00111111 可以忽略前两位 用 0x3F 掩码获取指针
                    int pointer = ((response[position] & 0x3F) << 8) | response[position + 1];
                    position = pointer;
                }
                else
                {
                    int length = response[position];
                    position++;
                    nameBuilder.Append(Encoding.ASCII.GetString(response, position, length));
                    position += length;
                    if (response[position] != 0)
                    {
                        nameBuilder.Append('.');
                    }
                }
            }

            if (!isPointer)
            {
                position++;
            }
            else
            {
                position = originalPosition;
            }

            name = nameBuilder.ToString();
            return position;
        }

        private static int SkipName(byte[] response, int position)
        {
            if (response[position] >= 192)
            {
                return position + 2; // Name is a pointer
            }

            while (response[position] != 0)
            {
                position += response[position] + 1;
            }
            return position + 1;
        }

        public IEnumerable<IPAddress> GetAddresses()
        {
            foreach (var answer in Answers)
            {
                if (answer is DnsQueryAnswerRecord.A a)
                {
                    yield return a.Data;
                }
                else if (answer is DnsQueryAnswerRecord.AAAA aaaa)
                {
                    yield return aaaa.Data;
                }
            }
            yield break;
        }
    }

    internal record DnsQueryUdpRequest
    {
        public ushort QueryId { get; init; } // (ushort)((_origin[0] << 8) | _origin[1]);

        public ushort Flags { get; init; } //(ushort)((_origin[2] << 8) | _origin[3]);

        public ushort QuestionCount { get; init; } //  (ushort)((_origin[4] << 8) | _origin[5]);

        public int Length => _origin.Length;

        protected int Position { get; set; }

        private readonly byte[] _origin;

        public DnsQueryUdpRequest(byte[] origin)
        {
            _origin = origin;

            Position = default;
            QueryId = (ushort)((origin[Position++] << 8) | origin[Position++]);
            Flags = (ushort)((origin[Position++] << 8) | origin[Position++]);
            QuestionCount = (ushort)((origin[Position++] << 8) | origin[Position++]);
        }

        public DnsQueryUdpRequest(string domainName, DnsQueryAnswerRecord.DnsRecordType type = DnsQueryAnswerRecord.DnsRecordType.A)
            : this(CreateDnsQuery(domainName, type))
        {
        }

        public static byte[] CreateDnsQuery(string domainName, DnsQueryAnswerRecord.DnsRecordType type = DnsQueryAnswerRecord.DnsRecordType.A)
        {
            // 12字节报文头部
            const int DNS_QUERY_HEADER_BYTECOUNT = 12;
            // 2字节终止符长度
            const int DNS_QUERY_END_BYTECOUNT = 2;
            // 4字节查询类型和查询类字段
            const int DNS_QUERY_QUERYTYPE_BYTECOUNT = 4;

            // dns queryId 范围是一个16位字段,16位无符号的范围就是0-65535
            var queryId = (ushort)Random.Shared.Next(0, 65536);

            byte[] dnsQuery = new byte[
                DNS_QUERY_HEADER_BYTECOUNT
                + domainName.Length
                + DNS_QUERY_END_BYTECOUNT
                + DNS_QUERY_QUERYTYPE_BYTECOUNT
            ];

            BuildQueryHeader(queryId, dnsQuery);
            BuildBody(domainName, dnsQuery);

            return dnsQuery;

            void BuildQueryHeader(ushort queryId, byte[] query)
            {
                // 查询 ID 是一个 16 位（2 字节）的字段。
                // 为了将这个 16 位的整数值分配到字节数组 dnsQuery 的两个字节中，
                // 我们需要将其高 8 位和低 8 位分别存储在数组的第一个和第二个字节中
                query[0] = (byte)(queryId >> 8);
                query[1] = (byte)(queryId & 0xFF);

                // 标志字段（Flags） 2 - 3 包含各种控制标志
                // 如查询/响应（QR）、操作码（Opcode）、授权回答（AA）、截断（TC）、递归期望（RD）、递归可用（RA）等
                query[2] = 0x01;

                // 问题计数（QDCOUNT）4 - 5 表示查询问题部分的数量
                query[5] = 0x01;

                // 其他查询报文中通常为 0
                // 回答记录计数（ANCOUNT),授权记录计数（NSCOUNT）,附加记录计数（ARCOUNT）
            }

            void BuildBody(string domainName, byte[] query)
            {
                // 获取域名组成部分 因为后面要根据部分长度和内容填充结果
                string[] domainParts = domainName.Split('.');

                int currentPosition = 12;

                // 根据 dns 查询报文 每一部分开始都要填充这一部分的长度
                foreach (var domainPart in domainParts)
                {
                    // 标志当前 part 的长度
                    query[currentPosition++] = (byte)domainPart.Length;

                    // 获取填充的字节
                    var fillBytes = Encoding.ASCII.GetBytes(domainPart);

                    // 移动到当前查询体中
                    fillBytes.CopyTo(query, currentPosition);

                    // 移动当前填充开始位置
                    currentPosition += fillBytes.Length;
                }
                // 0 表示域名结束
                query[currentPosition++] = 0x00;

                // 填充 查询类型和查询类字段
                query[currentPosition++] = 0x00;
                query[currentPosition++] = (byte)type;  // 表示查询类型为 A 记录（IPv4 地址）。
                query[currentPosition++] = 0x00;
                query[currentPosition++] = 0x01;  // 表示查询类为 IN（Internet 类）。
            }
        }

        public static implicit operator byte[](DnsQueryUdpRequest data)
        {
            return data._origin;
        }

        public static implicit operator ReadOnlyMemory<byte>(DnsQueryUdpRequest data)
        {
            return data._origin;
        }
    }

    internal record DnsQueryJsonResponse : IDnsQueryResponse
    {
        [S_JsonProperty("Status")]
        public int Status { get; init; }

        [S_JsonProperty("TC")]
        public bool TC { get; init; }

        [S_JsonProperty("RD")]
        public bool RD { get; init; }

        [S_JsonProperty("RA")]
        public bool RA { get; init; }

        [S_JsonProperty("AD")]
        public bool AD { get; init; }

        [S_JsonProperty("CD")]
        public bool CD { get; init; }

        [S_JsonProperty("Answer")]
        public List<DnsQueryAnswerRecord> Answers { get; init; }

        public DnsQueryJsonResponse(
                int status,
                bool tc,
                bool rd,
                bool ra,
                bool ad,
                bool cd,
                List<DnsQueryAnswerRecord>? answers
            )
        {
            Status = status;
            TC = tc;
            RD = rd;
            RA = ra;
            AD = ad;
            CD = cd;
            Answers = answers?.ToList() ?? new();
        }

        public IEnumerable<IPAddress> GetAddresses()
        {
            return Answers.Aggregate(Enumerable.Empty<IPAddress>(), (acc, x) =>
            {
                if (x is DnsQueryAnswerRecord.A ipv4)
                {
                    return acc.Append(ipv4.Data);
                }
                else if (x is DnsQueryAnswerRecord.AAAA ipv6)
                {
                    return acc.Append(ipv6.Data);
                }
                return acc;
            });
        }
    }

    private class DnsQueryAnswerRecordConverter : System.Text.Json.Serialization.JsonConverter<DnsQueryAnswerRecord>
    {
        public override DnsQueryAnswerRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
        {
            if (typeToConvert == typeof(DnsQueryAnswerRecord))
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var json = JsonDocument.ParseValue(ref reader);

                    if (json.RootElement.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetInt32();

                        if (type == (int)DnsQueryAnswerRecord.DnsRecordType.A)
                        {
                            return json.RootElement.Deserialize<DnsQueryAnswerRecord.A>();
                        }
                        else if (type == (int)DnsQueryAnswerRecord.DnsRecordType.AAAA)
                        {
                            return json.RootElement.Deserialize<DnsQueryAnswerRecord.AAAA>();
                        }
                        else if (type == (int)DnsQueryAnswerRecord.DnsRecordType.CNAME)
                        {
                            return json.RootElement.Deserialize<DnsQueryAnswerRecord.CName>();
                        }
                    }
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DnsQueryAnswerRecord value, SystemTextJsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(DnsQueryAnswerRecordConverter))]
    [JsonDerivedType(typeof(DnsQueryAnswerRecord.A))]
    [JsonDerivedType(typeof(DnsQueryAnswerRecord.AAAA))]
    [JsonDerivedType(typeof(DnsQueryAnswerRecord.CName))]
    internal record DnsQueryAnswerRecord
    {
        [S_JsonProperty("name")]
        public string Name { get; set; }

        [S_JsonProperty("type")]
        public DnsRecordType Type { get; set; }

        [S_JsonProperty("TTL")]
        public long Ttl { get; set; }

        public DnsQueryAnswerRecord(
                string name,
                DnsRecordType type,
                long ttl
            )
        {
            Name = name;
            Type = @type;
            Ttl = ttl;
            //Data = data;
            //DataLength = data.Length;
        }

        #region DnsQueryAnswerRecord 子类派生

        internal record A : DnsQueryAnswerRecord
        {
            public A(string name, DnsRecordType type, long ttl, IPAddress data) : base(name, type, ttl)
            {
                Data = data;
            }

            [S_JsonProperty("data")]
            [SystemTextJsonConverter(typeof(IPAddressConverter))]
            public IPAddress Data { get; set; }
        }

        internal record AAAA : DnsQueryAnswerRecord
        {
            public AAAA(string name, DnsRecordType type, long ttl, IPAddress data) : base(name, type, ttl)
            {
                Data = data;
            }

            [S_JsonProperty("data")]
            [SystemTextJsonConverter(typeof(IPAddressConverter))]
            public IPAddress Data { get; set; }
        }

        internal record CName : DnsQueryAnswerRecord
        {
            public CName(string name, DnsRecordType type, long ttl, string data) : base(name, type, ttl)
            {
                Data = data;
            }

            [S_JsonProperty("data")]
            public string Data { get; set; }
        }

        #endregion DnsQueryAnswerRecord 子类派生

        class IPAddressConverter : System.Text.Json.Serialization.JsonConverter<IPAddress>
        {
            public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, SystemTextJsonSerializerOptions options)
            {
                IPAddress? result = null;

                // 目前只解析字符串
                if (reader.TokenType == JsonTokenType.String)
                {
                    string? ipString = reader.GetString();
                    result = !string.IsNullOrWhiteSpace(ipString) ? IPAddress.Parse(ipString) : default;
                }
                // byte 数组支持
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    List<byte> bytes = new List<byte>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;

                        if (reader.TokenType == JsonTokenType.Number)
                        {
                            bytes.Add((byte)reader.GetInt32());
                        }
                        else if (reader.TokenType == JsonTokenType.String)
                        {
                            if (byte.TryParse(reader.GetString(), out byte byteValue))
                            {
                                bytes.Add(byteValue);
                            }
                        }
                    }

                    result = bytes.Count == 4 || bytes.Count == 16 ? new IPAddress(bytes.ToArray()) : default;
                }

                return result;
            }

            public override void Write(Utf8JsonWriter writer, IPAddress value, SystemTextJsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        internal enum DnsRecordClass : ushort
        {
            Internet = 1,     // Internet(IN)
            Chaos = 3,        // Chaos(CH)
            Hesiod = 4,       // Hesiod(HS)
            QCLASSNONE = 254, // QCLASS NONE
            QCLASSANY = 255,  // QCLASS * (ANY)
        }

        // 域名系统记录类型 (省略了有些未使用的类型)
        internal enum DnsRecordType : byte
        {
            A = 1,       // 指定域名对应的IPv4地址
            AAAA = 28,   // 指定域名对应的IPv6地址
            CNAME = 5,   // 别名记录,
            MX = 15,     // 邮件交换记录,
            NS = 2,      // 指定该域名由哪个DNS服务器来进行解析
            TXT = 16,    // 主机名或域名的说明
            SOA = 6,     // 起始授权机构
            PTR = 12,    // 反向IP查询
            ANY = 255,   // 所有DNS记录类型
        }
    }

    #endregion DNS 解析定义

    #endregion DNS 查询
}