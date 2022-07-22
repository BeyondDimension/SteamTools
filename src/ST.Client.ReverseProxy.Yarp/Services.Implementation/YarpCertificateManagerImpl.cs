using System.Application.Services.Implementation.HttpServer.Certificates;
using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services.Implementation;

sealed partial class YarpCertificateManagerImpl : CertificateManagerImpl, ICertificateManager
{
    public YarpCertificateManagerImpl(IPlatformService platformService, IReverseProxyService reverseProxyService) : base(platformService, reverseProxyService)
    {
        RootCertificate = LoadRootCertificate();
    }

    public override X509Certificate2? RootCertificate { get; set; }

    protected override bool SharedCreateRootCertificate()
    {
        if (RootCertificate == null)
        {
            RootCertificate = LoadRootCertificate();
        }

        if (RootCertificate != null)
        {
            return true;
        }

        var validFrom = DateTime.Today.AddDays(-1);
        var validTo = DateTime.Today.AddDays(ICertificateManager.CertificateValidDays);

        var rootCertificateName = IReverseProxyService.RootCertificateName;

        RootCertificate = CertGenerator.GenerateBySelfPfx(new[] { rootCertificateName }, CertService.KEY_SIZE_BITS, validFrom, validTo, Interface.PfxFilePath, Interface.PfxPassword);

        return RootCertificate != null;
    }

    protected override X509Certificate2? LoadRootCertificate()
    {
        try
        {
            ICertificateManager thiz = this;
            X509Certificate2 rootCert = new(thiz.PfxFilePath, thiz.PfxPassword, X509KeyStorageFlags.Exportable);
            if (rootCert.NotAfter <= DateTime.Now)
            {
                Log.Error(TAG, "Loaded root certificate has expired.");
                return null;
            }
            return rootCert;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, nameof(LoadRootCertificate));
            return null;
        }
    }

    protected override void SharedRemoveTrustedRootCertificate()
    {
        if (RootCertificate == null)
        {
            throw new ApplicationException(
                "Could not remove certificate as it is null or empty.");
        }

        using var x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

        try
        {
            x509Store.Open(OpenFlags.ReadWrite);

            x509Store.Remove(RootCertificate);
        }
        catch (Exception e)
        {
            Log.Error(TAG, new ApplicationException(
                "Failed to remove root certificate trust for CurrentUser store location. You may need admin rights.", e), nameof(SharedRemoveTrustedRootCertificate));
        }
        finally
        {
            x509Store.Close();
        }
    }

    protected override void SharedTrustRootCertificate()
    {
        try
        {
            if (RootCertificate == null)
            {
                throw new ApplicationException(
                    "Could not install certificate as it is null or empty.");
            }
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            var subjectName = RootCertificate.Subject[3..];
            foreach (var item in store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            {
                if (item.Thumbprint != RootCertificate.Thumbprint)
                {
                    store.Remove(item);
                }
            }
            if (store.Certificates.Find(X509FindType.FindByThumbprint, RootCertificate.Thumbprint, true).Count == 0)
            {
                store.Add(RootCertificate);
            }
            store.Close();
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                $"Please manually install the CA certificate {Interface.PfxFilePath} to a trusted root certificate authority.");
        }
    }
}
