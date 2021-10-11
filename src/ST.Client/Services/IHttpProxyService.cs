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
    public interface IHttpProxyService : IDisposable
    {
        static IHttpProxyService Instance => DI.Get<IHttpProxyService>();

        bool IsCertificate { get; }

        void TrustCer();

        IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

        IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

        bool IsEnableScript { get; set; }

        bool IsOnlyWorkSteamBrowser { get; set; }

        string CertificateName { get; set; }

        CertificateEngine CertificateEngine { get; set; }

        int ProxyPort { get; set; }

        IPAddress ProxyIp { get; set; }

        bool IsSystemProxy { get; set; }

        bool IsProxyGOG { get; set; }

        bool OnlyEnableProxyScript { get; set; }

        bool Socks5ProxyEnable { get; set; }

        int Socks5ProxyPortId { get; set; }
        int HostProxyPortId { get; set; }


        bool TwoLevelAgentEnable { get; set; }

        ExternalProxyType TwoLevelAgentProxyType { get; set; }


        const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

        string? TwoLevelAgentIp { get; set; }

        int TwoLevelAgentPortId { get; set; }

        string? TwoLevelAgentUserName { get; set; }

        string? TwoLevelAgentPassword { get; set; }

        bool ProxyRunning { get; }

        bool SetupCertificate();

        bool DeleteCertificate();

        bool PortInUse(int port);

        bool StartProxy();

        void StopProxy();

        bool WirtePemCertificateToGoGSteamPlugins();

        bool IsCertificateInstalled(X509Certificate2? certificate2);
    }
}