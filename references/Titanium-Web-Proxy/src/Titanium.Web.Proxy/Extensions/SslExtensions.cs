using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.StreamExtended;

namespace Titanium.Web.Proxy.Extensions
{
    internal static class SslExtensions
    {
        internal static readonly List<SslApplicationProtocol> Http11ProtocolAsList =
            new List<SslApplicationProtocol> { SslApplicationProtocol.Http11 };

        internal static readonly List<SslApplicationProtocol> Http2ProtocolAsList =
            new List<SslApplicationProtocol> { SslApplicationProtocol.Http2 };

        internal static string? GetServerName(this ClientHelloInfo clientHelloInfo)
        {
            if (clientHelloInfo.Extensions != null &&
                clientHelloInfo.Extensions.TryGetValue("server_name", out var serverNameExtension))
            {
                return serverNameExtension.Data;
            }

            return null;
        }

#if NETSTANDARD2_1
        internal static List<SslApplicationProtocol>? GetAlpn(this ClientHelloInfo clientHelloInfo)
        {
            if (clientHelloInfo.Extensions != null && clientHelloInfo.Extensions.TryGetValue("ALPN", out var alpnExtension))
            {
                var alpn = alpnExtension.Data.Split(',');
                if (alpn.Length != 0)
                {
                    var result = new List<SslApplicationProtocol>(alpn.Length);
                    foreach (string p in alpn)
                    {
                        string protocol = p.Trim();
                        if (protocol.Equals("http/1.1"))
                        {
                            result.Add(SslApplicationProtocol.Http11);
                        }
                        else if (protocol.Equals("h2"))
                        {
                            result.Add(SslApplicationProtocol.Http2);
                        }
                    }

                    return result;
                }
            }

            return null;
        }
#else
        internal static List<SslApplicationProtocol> GetAlpn(this ClientHelloInfo clientHelloInfo)
        {
            return Http11ProtocolAsList;
        }

        internal static Task AuthenticateAsClientAsync(this SslStream sslStream, SslClientAuthenticationOptions option,
            CancellationToken token)
        {
            return sslStream.AuthenticateAsClientAsync(option.TargetHost, option.ClientCertificates,
                option.EnabledSslProtocols, option.CertificateRevocationCheckMode != X509RevocationMode.NoCheck);
        }

        internal static Task AuthenticateAsServerAsync(this SslStream sslStream, SslServerAuthenticationOptions options,
            CancellationToken token)
        {
            return sslStream.AuthenticateAsServerAsync(options.ServerCertificate, options.ClientCertificateRequired,
                options.EnabledSslProtocols, options.CertificateRevocationCheckMode != X509RevocationMode.NoCheck);
        }
#endif
    }
}

#if !NETSTANDARD2_1
namespace System.Net.Security
{
    internal enum SslApplicationProtocol
    {
        Http11,
        Http2
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleType", Justification =
        "Reviewed.")]
    internal class SslClientAuthenticationOptions
    {
        internal bool AllowRenegotiation { get; set; }

        internal string? TargetHost { get; set; }

        internal X509CertificateCollection? ClientCertificates { get; set; }

        internal LocalCertificateSelectionCallback? LocalCertificateSelectionCallback { get; set; }

        internal SslProtocols EnabledSslProtocols { get; set; }

        internal X509RevocationMode CertificateRevocationCheckMode { get; set; }

        internal List<SslApplicationProtocol>? ApplicationProtocols { get; set; }

        internal RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

        internal EncryptionPolicy EncryptionPolicy { get; set; }
    }

    internal class SslServerAuthenticationOptions
    {
        internal bool AllowRenegotiation { get; set; }

        internal X509Certificate? ServerCertificate { get; set; }

        internal bool ClientCertificateRequired { get; set; }

        internal SslProtocols EnabledSslProtocols { get; set; }

        internal X509RevocationMode CertificateRevocationCheckMode { get; set; }

        internal List<SslApplicationProtocol>? ApplicationProtocols { get; set; }

        internal RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }

        internal EncryptionPolicy EncryptionPolicy { get; set; }
    }
}
#endif
