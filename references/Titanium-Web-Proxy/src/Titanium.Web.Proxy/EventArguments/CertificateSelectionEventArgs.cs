using System.Security.Cryptography.X509Certificates;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    ///     An argument passed on to user for client certificate selection during mutual SSL authentication.
    /// </summary>
    public class CertificateSelectionEventArgs : ProxyEventArgsBase
    {
        public CertificateSelectionEventArgs(SessionEventArgsBase session, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) : base(session.ClientConnection)
        {
            Session = session;
            TargetHost = targetHost;
            LocalCertificates = localCertificates;
            RemoteCertificate = remoteCertificate;
            AcceptableIssuers = acceptableIssuers;
        }

        /// <value>
        ///     The session.
        /// </value>
        public SessionEventArgsBase Session { get; }

        /// <summary>
        ///     The remote hostname to which we are authenticating against.
        /// </summary>
        public string TargetHost { get; }

        /// <summary>
        ///     Local certificates in store with matching issuers requested by TargetHost website.
        /// </summary>
        public X509CertificateCollection LocalCertificates { get; }

        /// <summary>
        ///     Certificate of the remote server.
        /// </summary>
        public X509Certificate RemoteCertificate { get; }

        /// <summary>
        ///     Acceptable issuers as listed by remote server.
        /// </summary>
        public string[] AcceptableIssuers { get; }

        /// <summary>
        ///     Client Certificate we selected. Set this value to override.
        /// </summary>
        public X509Certificate? ClientCertificate { get; set; }
    }
}
