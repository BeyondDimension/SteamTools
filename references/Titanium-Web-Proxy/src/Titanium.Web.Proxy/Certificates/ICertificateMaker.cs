using System.Security.Cryptography.X509Certificates;

namespace Titanium.Web.Proxy.Network.Certificate
{
    /// <summary>
    ///     Abstract interface for different Certificate Maker Engines
    /// </summary>
    internal interface ICertificateMaker
    {
        X509Certificate2 MakeCertificate(string sSubjectCn, X509Certificate2? signingCert);
    }
}
