using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services;

public interface ICertificateManager
{
    int CertificateValidDays { get; set; }

    X509Certificate2? RootCertificate { get; set; }

    X509Certificate2? LoadRootCertificate();

    bool CreateRootCertificate(bool persistToFile = true);

    void TrustRootCertificate(bool machineTrusted = false);

    void EnsureRootCertificate();

    void RemoveTrustedRootCertificate(bool machineTrusted = false);
}
