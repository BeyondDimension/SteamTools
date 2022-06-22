using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services.Implementation;

partial class YarpReverseProxyServiceImpl : ICertificateManager
{
    public override ICertificateManager CertificateManager => this;

    int ICertificateManager.CertificateValidDays { get; set; }

    X509Certificate2? ICertificateManager.RootCertificate { get; set; }

    bool ICertificateManager.CreateRootCertificate(bool persistToFile)
    {
        throw new NotImplementedException();
    }

    void ICertificateManager.EnsureRootCertificate()
    {
        throw new NotImplementedException();
    }

    X509Certificate2? ICertificateManager.LoadRootCertificate()
    {
        throw new NotImplementedException();
    }

    void ICertificateManager.RemoveTrustedRootCertificate(bool machineTrusted)
    {
        throw new NotImplementedException();
    }

    void ICertificateManager.TrustRootCertificate(bool machineTrusted)
    {
        throw new NotImplementedException();
    }
}
