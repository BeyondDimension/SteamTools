using Titanium.Web.Proxy.Network;

namespace System.Application.Services.Implementation;

sealed class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService
{
    public YarpReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalysis) : base(platformService, dnsAnalysis)
    {
        const bool userTrustRootCertificate = true;
        const bool machineTrustRootCertificate = false;
        const bool trustRootCertificateAsAdmin = false;
        CertificateManager = new(null, null, userTrustRootCertificate, machineTrustRootCertificate, trustRootCertificateAsAdmin, OnException);

        InitCertificateManager();
    }

    public override CertificateManager CertificateManager { get; }

    public override bool ProxyRunning => throw new NotImplementedException();

    public Task<bool> StartProxy()
    {
        throw new NotImplementedException();
    }

    public void StopProxy()
    {
        throw new NotImplementedException();
    }

    protected override void DisposeCore()
    {
        CertificateManager.Dispose();
    }
}