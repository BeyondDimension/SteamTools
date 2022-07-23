using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Titanium.Web.Proxy.Network;

namespace System.Application.Services.Implementation;

sealed partial class TitaniumCertificateManagerImpl : CertificateManagerImpl, ICertificateManager
{
    readonly CertificateManager manager;

    public TitaniumCertificateManagerImpl(
        IPlatformService platformService,
        IReverseProxyService reverseProxyService,
        CertificateManager manager) : base(platformService, reverseProxyService)
    {
        // 可选地设置证书引擎
        manager.CertificateEngine = CertificateEngine.BouncyCastle;
        //manager.PfxPassword = $"{CertificateName}";
        manager.PfxFilePath = Interface.PfxFilePath;
        manager.RootCertificateName = IReverseProxyService.RootCertificateName;

        // mac 和 ios 的证书信任时间不能超过300天
        manager.CertificateValidDays = ICertificateManager.CertificateValidDays;
        //manager.SaveFakeCertificates = true;

        manager.RootCertificate = manager.LoadRootCertificate();

        this.manager = manager;
    }

    public override X509Certificate2? RootCertificate
    {
        get => manager.RootCertificate;
        set => manager.RootCertificate = value;
    }

    protected override X509Certificate2? LoadRootCertificate()
    {
        try
        {
            return manager.LoadRootCertificate();
        }
        catch (PlatformNotSupportedException)
        {
            // https://github.com/dotnet/runtime/issues/71603
            return null;
        }
        catch (CryptographicException e)
        {
            if (e.InnerException is PlatformNotSupportedException) return null;
            throw;
        }
    }

    protected override bool SharedCreateRootCertificate()
        => manager.CreateRootCertificate(true);

    protected override void SharedRemoveTrustedRootCertificate()
        => manager.RemoveTrustedRootCertificate(false);

    protected override void SharedTrustRootCertificate()
        => manager.TrustRootCertificate(false);
}
