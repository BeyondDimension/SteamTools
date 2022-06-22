// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/CertService.cs

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Application.Models;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services.Implementation.HttpServer.Certificates;

/// <summary>
/// 证书服务
/// </summary>
sealed class CertService
{
    readonly IMemoryCache serverCertCache;
    readonly ILogger<CertService> logger;
    readonly IReverseProxyConfig reverseProxyConfig;

    const int KEY_SIZE_BITS = 2048;

    IReverseProxyService ReverseProxyService => reverseProxyConfig.Service;

    /// <summary>
    /// 获取 CER 证书文件路径
    /// </summary>
    public string CaCerFilePath => ReverseProxyService.CerFilePath;

    /// <summary>
    /// 获取 PFX 证书文件路径
    /// </summary>
    public string CaPfxFilePath => ReverseProxyService.PfxFilePath;

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
    /// 生成 CA 证书
    /// </summary> 
    [Obsolete("move to YarpReverseProxyServiceImpl.ICertificateManager.CreateRootCertificate", true)]
    public bool CreateCaCertIfNotExists()
    {
        if (File.Exists(CaCerFilePath))
        {
            return false;
        }

        File.Delete(CaCerFilePath);

        var validFrom = DateTime.Today.AddDays(-1);
        var validTo = DateTime.Today.AddYears(10);

        int value = reverseProxyConfig.Service.CertificateManager.CertificateValidDays;
        var rootCertificateIssuerName = IReverseProxyService.RootCertificateIssuerName;
        var rootCertificateName = IReverseProxyService.RootCertificateName;

        throw new NotImplementedException("");

        CertGenerator.GenerateBySelf(new[] { IReverseProxyService.CertificateName, }, KEY_SIZE_BITS, validFrom, validTo, CaCerFilePath, null);
        return true;
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
    public X509Certificate2 GetOrCreateServerCert(string? domain)
    {
        var key = $"{nameof(CertService)}:{domain}";
        return serverCertCache.GetOrCreate(key, GetOrCreateCert);

        // 生成域名的1年证书
        X509Certificate2 GetOrCreateCert(ICacheEntry entry)
        {
            var domains = GetDomains(domain).Distinct();
            var validFrom = DateTime.Today.AddDays(-1);
            var validTo = DateTime.Today.AddYears(1);

            entry.SetAbsoluteExpiration(validTo);
            return CertGenerator.GenerateByCaPfx(domains, KEY_SIZE_BITS, validFrom, validTo, CaPfxFilePath);
        }
    }

    /// <summary>
    /// 获取域名
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    static IEnumerable<string> GetDomains(string? domain)
    {
        if (string.IsNullOrEmpty(domain) == false)
        {
            yield return domain;
            yield break;
        }

        yield return Environment.MachineName;
        yield return IPAddress.Loopback.ToString();
        yield return IPAddress.IPv6Loopback.ToString();
    }
}
