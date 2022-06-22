using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services;

public interface ICertificateManager
{
    string PfxFilePath { get; set; }

    string RootCertificateIssuerName { get; set; }

    string RootCertificateName { get; set; }

    int CertificateValidDays { get; set; }

    X509Certificate2? RootCertificate { get; set; }

    X509Certificate2? LoadRootCertificate();

    bool CreateRootCertificate(bool persistToFile = true);

    void TrustRootCertificate(bool machineTrusted = false);

    void EnsureRootCertificate();

    void RemoveTrustedRootCertificate(bool machineTrusted = false);
}
