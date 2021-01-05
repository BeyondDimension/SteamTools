using System;
using System.Security.Cryptography.X509Certificates;

namespace Titanium.Web.Proxy.Network
{
    /// <summary>
    ///     An object that holds the cached certificate
    /// </summary>
    internal sealed class CachedCertificate
    {
        public CachedCertificate(X509Certificate2 certificate)
        {
            Certificate = certificate;
        }

        internal X509Certificate2 Certificate { get; }

        /// <summary>
        ///     Last time this certificate was used.
        ///     Useful in determining its cache lifetime.
        /// </summary>
        internal DateTime LastAccess { get; set; }
    }
}
