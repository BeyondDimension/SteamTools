using System.Security.Cryptography.X509Certificates;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

sealed class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService
{
    readonly IPlatformService platformService;

    public YarpReverseProxyServiceImpl(IPlatformService platformService)
    {
        this.platformService = platformService;
    }

    public bool IsCertificate => throw new NotImplementedException();

    public bool ProxyRunning => throw new NotImplementedException();

    public bool IsCurrentCertificateInstalled => throw new NotImplementedException();

    public X509Certificate2? RootCertificate => throw new NotImplementedException();

    public bool DeleteCertificate()
    {
        throw new NotImplementedException();
    }

    public string? GetCerFilePathGeneratedWhenNoFileExists()
    {
        throw new NotImplementedException();
    }

    public bool IsCertificateInstalled(X509Certificate2? certificate2)
    {
        throw new NotImplementedException();
    }

    public bool SetupCertificate()
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartProxy()
    {
        throw new NotImplementedException();
    }

    public void StopProxy()
    {
        throw new NotImplementedException();
    }

    public void TrustCer()
    {
        throw new NotImplementedException();
    }

    public bool WirtePemCertificateToGoGSteamPlugins()
    {
        throw new NotImplementedException();
    }

    protected override void DisposeCore()
    {

    }
}