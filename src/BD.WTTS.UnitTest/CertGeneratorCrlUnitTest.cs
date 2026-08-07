using System.Formats.Asn1;
using System.Net;
using System.Net.Sockets;

namespace BD.WTTS.UnitTest;

public sealed class CertGeneratorCrlUnitTest
{
    [Test]
    public void EndCertificate_ContainsCrlDistributionPoint()
    {
        using var caCert = CreateTestCa();

        const string crlUrl = "http://127.0.0.1:26502/crl";
        using var endCert = CertGenerator.CreateEndCertificate(
            caCert, new X500DistinguishedName("CN=github.com"), null, crlDistributionPointUrl: crlUrl);

        var cdp = endCert.Extensions["2.5.29.31"];
        Assert.That(cdp, Is.Not.Null, "叶子证书应包含 CRL 分发点扩展");

        // 从 CRLDistributionPoints 扩展的 DER 中读取第一个 uniformResourceIdentifier 并断言
        Assert.That(
            new AsnReader(cdp!.RawData, AsnEncodingRules.DER)
                .ReadSequence() // CRLDistributionPoints
                .ReadSequence() // DistributionPoint
                .ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0)) // distributionPoint [0]
                .ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0)) // fullName [0]
                .ReadCharacterString(
                    UniversalTagNumber.IA5String,
                    new Asn1Tag(TagClass.ContextSpecific, 6)), // uniformResourceIdentifier
            Is.EqualTo(crlUrl));
    }

    [Test]
    public void EmptyCrl_Issuer_IsCa()
    {
        using var caCert = CreateTestCa();

        var crlBytes = BuildEmptyCrl(caCert);

        Assert.That(crlBytes, Is.Not.Null);
        Assert.That(crlBytes.Length, Is.GreaterThan(0));
        Assert.That(ContainsSubsequence(crlBytes, caCert.SubjectName.RawData), Is.True,
            "CRL 应包含根 CA 的 Issuer 名称");
    }

    [Test]
    public void EndCertificate_ChainsToCa_WithNoRevocationCheck()
    {
        using var caCert = CreateTestCa();

        using var endCert = CertGenerator.CreateEndCertificate(
            caCert, new X500DistinguishedName("CN=github.com"), null,
            crlDistributionPointUrl: "http://127.0.0.1:26502/crl");

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        chain.ChainPolicy.ExtraStore.Add(caCert);

        Assert.That(chain.Build(endCert), Is.True,
            $"叶子证书应能构建到 CA: {string.Join("; ", chain.ChainStatus.Select(s => s.StatusInformation))}");
    }

    [Test]
    [Platform("Win")]
    public void CrlServer_ServesValidCrlOverHttp()
    {
        using var caCert = CreateTestCa();
        var crlBytes = BuildEmptyCrl(caCert);

        using var server = new LocalCrlServer(crlBytes);

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        Assert.That(http.GetByteArrayAsync(server.CrlUrl).GetAwaiter().GetResult(), Is.EqualTo(crlBytes),
            "CDP 指向的 URL 应能返回完整 CRL 字节");
    }

    [Test]
    [Platform("Win")]
    public void EndCertificate_OnlineRevocation_PassesWithLocalCrl()
    {
        // 模拟真实场景：根 CA 受信任 + 本地 HttpListener 提供 CRL。
        // 根信任用 X509ChainPolicy.CustomTrustStore（临时信任库，不写系统证书库、不弹窗、无残留）。
        using var caCert = CreateTestCa();
        var crlBytes = BuildEmptyCrl(caCert);

        using var server = new LocalCrlServer(crlBytes);

        using var endCert = CertGenerator.CreateEndCertificate(
            caCert, new X500DistinguishedName("CN=github.com"), null,
            crlDistributionPointUrl: server.CrlUrl);

        var chain = new X509Chain();
        var chainDisposed = false;
        try
        {
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(10);
            // 将测试根 CA 加入临时信任库，替代写系统 Root 存储（避免证书弹窗与残留）
            chain.ChainPolicy.CustomTrustStore.Add(caCert);
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            var buildTask = Task.Run(() => chain.Build(endCert));
            if (!buildTask.Wait(TimeSpan.FromSeconds(30)))
            {
                // 超时保护：不释放 chain（后台线程可能仍在使用），避免访问冲突；仅报告失败
                Assert.Fail("X509Chain.Build(Online) 超时（30s）：本地 CRL 未能在限定时间内完成吊销检查。");
                return;
            }
            chainDisposed = true;
            Assert.That(buildTask.Result, Is.True,
                $"吊销检查失败: {string.Join("; ", chain.ChainStatus.Select(s => s.StatusInformation))}");
        }
        finally
        {
            if (chainDisposed)
            {
                chain.Dispose();
            }
        }
    }

    /// <summary>
    /// 构建由指定 CA 签名的空 CRL
    /// </summary>
    static byte[] BuildEmptyCrl(X509Certificate2 caCert)
    {
        return new CertificateRevocationListBuilder().Build(
            caCert,
            System.Numerics.BigInteger.One,
            DateTimeOffset.UtcNow.AddDays(30),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    /// <summary>
    /// 判断 haystack 中是否包含 needle 子序列
    /// </summary>
    static bool ContainsSubsequence(byte[] haystack, byte[] needle)
    {
        if (needle.Length == 0) return true;
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        return false;
    }

    /// <summary>
    /// 创建测试用根 CA 证书
    /// </summary>
    static X509Certificate2 CreateTestCa()
    {
        return CertGenerator.CreateCACertificate(
            new X500DistinguishedName("CN=WattToolkitTestCA"),
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));
    }

    /// <summary>
    /// 获取一个空闲的本地端口（HttpListener 不支持端口 0 自动分配后回读）
    /// </summary>
    static int GetIdlePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    /// <summary>
    /// 在本地通过 HTTP 托管指定的 CRL 字节，供在线吊销检查测试使用。
    /// 端口自动分配以避免冲突；Dispose 时停止监听器。
    /// </summary>
    sealed class LocalCrlServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly byte[] _crlBytes;

        public string CrlUrl { get; }

        public LocalCrlServer(byte[] crlBytes)
        {
            _crlBytes = crlBytes;
            int port = GetIdlePort();
            CrlUrl = $"http://127.0.0.1:{port}/crl";
            _listener = new HttpListener();
            _listener.Prefixes.Add($"{CrlUrl}/");
            _listener.Start();
            _ = Task.Run(ServeLoop);
        }

        async Task ServeLoop()
        {
            while (_listener.IsListening)
            {
                HttpListenerContext? context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch
                {
                    break; // 监听器已停止
                }
                var response = context.Response;
                response.StatusCode = 200;
                response.ContentType = "application/pkix-crl";
                response.ContentLength64 = _crlBytes.Length;
                await response.OutputStream.WriteAsync(_crlBytes);
                response.Close();
            }
        }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
