using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace System.Application.Internals.Certs
{
    /// <summary>
    /// 证书服务
    /// </summary>
    sealed class CertService
    {
        private const string defaultRootCertificateIssuer = "SteamTools";
        private const int KEY_SIZE_BITS = 2048;
        private readonly IMemoryCache serverCertCache;
        private readonly ILogger<CertService> logger;
        const string CaFileName = $"{defaultRootCertificateIssuer}.Certificate";
        const string CaPfxFileName = CaFileName + FileEx.PFX;

        /// <summary>
        /// 获取证书文件路径
        /// </summary>
        public string CaCerFilePath { get; } = OperatingSystem.IsLinux() ? $"{IOPath.AppDataDirectory}/{CaFileName}.crt" : $"{IOPath.AppDataDirectory}/{CaFileName}.cer";

        /// <summary>
        /// 获取私钥文件路径
        /// </summary>
        public string CaKeyFilePath { get; } = $"{IOPath.AppDataDirectory}/{CaFileName}.key";

        /// <summary>
        /// 获取pfx文件路径
        /// </summary>
        public string CaPfxFilePath { get; } = Path.Combine(IOPath.AppDataDirectory, CaPfxFileName);

        /// <summary>
        /// 证书服务
        /// </summary>
        /// <param name="serverCertCache"></param>
        /// <param name="certInstallers"></param>
        /// <param name="logger"></param>
        public CertService(
            IMemoryCache serverCertCache,
            ILogger<CertService> logger)
        {
            this.serverCertCache = serverCertCache;
            this.logger = logger;
        }

        /// <summary>
        /// 生成CA证书
        /// </summary> 
        public bool LoadCaCert(string caPath)
        {
            if (File.Exists(caPath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成CA证书
        /// </summary> 
        public bool CreateCaCertIfNotExists()
        {
            if (File.Exists(CaCerFilePath) && File.Exists(CaKeyFilePath))
            {
                return false;
            }

            File.Delete(CaCerFilePath);
            File.Delete(CaKeyFilePath);

            var validFrom = DateTime.Today.AddDays(-1);
            var validTo = DateTime.Today.AddYears(10);
            CertGenerator.GenerateBySelf(new[] { defaultRootCertificateIssuer }, KEY_SIZE_BITS, validFrom, validTo, CaCerFilePath, CaKeyFilePath);
            return true;
        }

        /// <summary>
        /// 安装和信任CA证书
        /// </summary> 
        //public void InstallAndTrustCaCert()
        //{
        //    var installer = certInstallers.FirstOrDefault(item => item.IsSupported());
        //    if (installer != null)
        //    {
        //        installer.Install(CaCerFilePath);
        //    }
        //    else
        //    {
        //        logger.LogWarning($"请根据你的系统平台手动安装和信任CA证书{CaCerFilePath}");
        //    }

        //    GitConfigSslverify(false);
        //}

        /// <summary>
        /// 设置ssl验证
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
                    Arguments = $"config --global http.sslverify {value.ToString().ToLower()}",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
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
        private static IEnumerable<string> GetDomains(string? domain)
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
}
