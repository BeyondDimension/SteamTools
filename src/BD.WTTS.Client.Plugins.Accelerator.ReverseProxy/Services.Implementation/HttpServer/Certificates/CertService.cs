// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/CertService.cs

// ReSharper disable once CheckNamespace
using BD.Common.Columns;

namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 证书服务
/// </summary>
sealed class CertService
{
    readonly IMemoryCache serverCertCache;
    readonly ILogger<CertService> logger;
    readonly IReverseProxyConfig reverseProxyConfig;
    private X509Certificate2? caCert;

    ReverseProxyServiceImpl ReverseProxyService => reverseProxyConfig.Service;

    /// <summary>
    /// 获取 CER 证书文件路径
    /// </summary>
    public string CaCerFilePath => ((ICertificateManager)ReverseProxyService.CertificateManager).CerFilePath;

    /// <summary>
    /// 获取 PFX 证书文件路径
    /// </summary>
    public string CaPfxFilePath => ((ICertificateManager)ReverseProxyService.CertificateManager).PfxFilePath;

    public CertService(
        IMemoryCache serverCertCache,
        ILogger<CertService> logger,
        IReverseProxyConfig reverseProxyConfig)
    {
        this.serverCertCache = serverCertCache;
        this.logger = logger;
        this.reverseProxyConfig = reverseProxyConfig;
    }

    /// <summary>
    /// 设置 SSL 验证
    /// </summary>
    /// <param name="value">是否验证</param>
    /// <returns></returns>
    public static bool GitConfigSslverify(bool value)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"config --global http.sslverify {value.ToLowerString()}",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取颁发给指定域名的证书
    /// </summary>
    /// <param name="domain"></param> 
    /// <returns></returns>
    public X509Certificate2? GetOrCreateServerCert(string? domain)
    {
        caCert ??= new X509Certificate2(fileName: CaPfxFilePath, password: default(string));

        var key = $"{nameof(CertService)}:{domain}";
        return serverCertCache.GetOrCreate(key, GetOrCreateCert);

        // 生成域名的 1 年证书
        X509Certificate2 GetOrCreateCert(ICacheEntry entry)
        {
            DateTimeOffset today = DateTime.Today;
            var notBefore = today.AddDays(-1);
            var notAfter = today.AddYears(1);
            entry.SetAbsoluteExpiration(notAfter);

            var subjectName = new X500DistinguishedName($"CN={domain}");
            using var serverCert = CertGenerator.CreateEndCertificate(caCert, subjectName, GetDomains());
            var serverCertPfx = serverCert.Export(X509ContentType.Pfx);
            // 将生成的证书导出后重新创建一个
            return new X509Certificate2(serverCertPfx);
        }
    }

    /// <summary>
    /// 获取域名
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    static IEnumerable<string> GetDomains()
    {
        yield return Environment.MachineName;
        yield return IPAddress.Loopback.ToString();
        yield return IPAddress.IPv6Loopback.ToString();
    }
}