using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Extensions;

namespace Titanium.Web.Proxy
{
    public partial class ProxyServer
    {
        /// <summary>
        ///     Call back to override server certificate validation
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="sessionArgs">The http session.</param>
        /// <param name="certificate">The remote certificate.</param>
        /// <param name="chain">The certificate chain.</param>
        /// <param name="sslPolicyErrors">Ssl policy errors</param>
        /// <returns>Return true if valid certificate.</returns>
        internal bool ValidateServerCertificate(object sender, SessionEventArgsBase sessionArgs, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // if user callback is registered then do it
            if (ServerCertificateValidationCallback != null)
            {
                var args = new CertificateValidationEventArgs(sessionArgs, certificate, chain, sslPolicyErrors);

                // why is the sender null?
                ServerCertificateValidationCallback.InvokeAsync(this, args, ExceptionFunc).Wait();
                return args.IsValid;
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            // By default
            // do not allow this client to communicate with unauthenticated servers.
            return true;
        }

        /// <summary>
        ///     Call back to select client certificate used for mutual authentication
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="sessionArgs">The http session.</param>
        /// <param name="targetHost">The remote hostname.</param>
        /// <param name="localCertificates">Selected local certificates by SslStream.</param>
        /// <param name="remoteCertificate">The remote certificate of server.</param>
        /// <param name="acceptableIssuers">The acceptable issues for client certificate as listed by server.</param>
        /// <returns></returns>
        internal X509Certificate? SelectClientCertificate(object sender, SessionEventArgsBase sessionArgs, string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            X509Certificate? clientCertificate = null;

            if (acceptableIssuers != null && acceptableIssuers.Length > 0 && localCertificates != null &&
                localCertificates.Count > 0)
            {
                foreach (var certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                    {
                        clientCertificate = certificate;
                    }
                }
            }

            if (localCertificates != null && localCertificates.Count > 0)
            {
                clientCertificate = localCertificates[0];
            }

            // If user call back is registered
            if (ClientCertificateSelectionCallback != null)
            {
                var args = new CertificateSelectionEventArgs(sessionArgs, targetHost, localCertificates, remoteCertificate, acceptableIssuers)
                {
                    ClientCertificate = clientCertificate
                };

                // why is the sender null?
                ClientCertificateSelectionCallback.InvokeAsync(this, args, ExceptionFunc).Wait();
                return args.ClientCertificate;
            }

            return clientCertificate;
        }
    }
}
