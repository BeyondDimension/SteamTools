using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Properties;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace System.Application.Services
{
    /// <summary>
    /// Http 代理服务
    /// </summary>
    public interface IHttpProxyService : IDisposable
    {
        /// <summary>
        /// 证书名称，硬编码不可改动，确保兼容性
        /// </summary>
        const string CertificateName = "SteamTools";
        const string RootCertificateName = $"{CertificateName} Certificate";
        const string RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
        const string LocalDomain = "local.steampp.net";
        protected const string TAG = "HttpProxyS";

        static IHttpProxyService Instance => DI.Get<IHttpProxyService>();

        bool IsCertificate { get; }

        void TrustCer();

        IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

        IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

        bool IsEnableScript { get; set; }

        bool IsOnlyWorkSteamBrowser { get; set; }

        CertificateEngine CertificateEngine { get; set; }

        int ProxyPort { get; set; }

        IPAddress ProxyIp { get; set; }

        bool IsSystemProxy { get; set; }

        bool IsProxyGOG { get; set; }

        bool OnlyEnableProxyScript { get; set; }

        bool Socks5ProxyEnable { get; set; }

        int Socks5ProxyPortId { get; set; }

        bool TwoLevelAgentEnable { get; set; }

        ExternalProxyType TwoLevelAgentProxyType { get; set; }


        const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

        string? TwoLevelAgentIp { get; set; }

        int TwoLevelAgentPortId { get; set; }

        string? TwoLevelAgentUserName { get; set; }

        string? TwoLevelAgentPassword { get; set; }

        IPAddress? ProxyDNS { get; set; }

        bool ProxyRunning { get; }

        bool SetupCertificate();

        bool DeleteCertificate();

        bool PortInUse(int port);

        Task<bool> StartProxy();

        void StopProxy();

        bool WirtePemCertificateToGoGSteamPlugins();

        bool IsCertificateInstalled(X509Certificate2? certificate2);

        const string PfxFileName = $"{CertificateName}.Certificate{FileEx.PFX}";

        const string CerFileName = $"{CertificateName}.Certificate{FileEx.CER}";

        static string CerExportFileName
        {
            get
            {
                var now = DateTime.Now;
                const string f = $"{ThisAssembly.AssemblyTrademark}  Certificate {{0}}{FileEx.CER}";
                return string.Format(f, now.ToString(DateTimeFormat.File));
            }
        }

        string PfxFilePath => Path.Combine(IOPath.AppDataDirectory, PfxFileName);

        string CerFilePath => Path.Combine(IOPath.AppDataDirectory, CerFileName);

        /// <summary>
        /// 获取 Cer 证书路径，当不存在时生成文件后返回路径
        /// </summary>
        /// <returns></returns>
        string? GetCerFilePathGeneratedWhenNoFileExists();
    }
}