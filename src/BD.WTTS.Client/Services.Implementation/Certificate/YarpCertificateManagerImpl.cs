#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class YarpCertificateManagerImpl : CertificateManagerImpl, ICertificateManager
{
    public YarpCertificateManagerImpl(IPlatformService platformService) : base(platformService)
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

        var rootCertificateName = ICertificateManager.RootCertificateName;

        RootCertificate = CertGenerator.GenerateBySelfPfx(new[] { rootCertificateName }, validFrom, validTo, Interface.PfxFilePath, Interface.PfxPassword);

        return RootCertificate != null;
    }

    protected override X509Certificate2? LoadRootCertificate()
    {
        try
        {
            ICertificateManager thiz = this;
            if (!File.Exists(thiz.PfxFilePath)) return null;
            X509Certificate2 rootCert;
            try
            {
                rootCert = new(thiz.PfxFilePath, thiz.PfxPassword, X509KeyStorageFlags.Exportable);
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

        using var x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

        try
        {
            x509Store.Open(OpenFlags.ReadWrite);
            var subjectName = RootCertificate.Subject[3..];
            foreach (var item in x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            {
                //if (item.Thumbprint == RootCertificate.Thumbprint)
                //{
                x509Store.Remove(item);
                //}
            }
            //x509Store.Remove(RootCertificate);
        }
        catch (Exception e)
        {
            Log.Error(TAG, new ApplicationException(
                "Failed to remove root certificate trust for LocalMachine store location. You may need admin rights.", e), nameof(SharedRemoveTrustedRootCertificate));
        }
        finally
        {
            x509Store.Close();
        }
    }

    protected override void SharedTrustRootCertificate()
    {
        if (RootCertificate == null)
        {
            throw new ApplicationException(
                "Could not install certificate as it is null or empty.");
        }

        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        try
        {
            store.Open(OpenFlags.ReadWrite);

            //var subjectName = RootCertificate.Subject[3..];
            //foreach (var item in store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            //{
            //    if (item.Thumbprint != RootCertificate.Thumbprint)
            //    {
            //        store.Remove(item);
            //    }
            //}

            if (store.Certificates.Find(X509FindType.FindByThumbprint, RootCertificate.Thumbprint, true).Count == 0)
            {
                store.Add(RootCertificate);
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                $"Please manually install the CA certificate {Interface.PfxFilePath} to a trusted root certificate authority.");
        }
        finally
        {
            store.Close();
        }
    }
}
#endif