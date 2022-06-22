using System.Application.Services.Implementation.HttpServer.Certificates;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services.Implementation;

partial class YarpReverseProxyServiceImpl : ICertificateManager
{
    public override ICertificateManager CertificateManager => this;

    int ICertificateManager.CertificateValidDays { get; set; } = 300;

    X509Certificate2? ICertificateManager.RootCertificate { get; set; }

    public string PfxFilePath { get; set; } = string.Empty;

    public string? PfxPassword { get; set; }

    bool ICertificateManager.CreateRootCertificate(bool persistToFile)
    {
        if (persistToFile && RootCertificate == null)
        {
            RootCertificate = CertificateManager.LoadRootCertificate();
        }

        if (RootCertificate != null)
        {
            return true;
        }

        var validFrom = DateTime.Today.AddDays(-1);
        var validTo = DateTime.Today.AddDays(CertificateManager.CertificateValidDays);

        var rootCertificateName = IReverseProxyService.RootCertificateName;

        string? pfxPath;
        if (persistToFile)
        {
            pfxPath = PfxFilePath;
        }

        RootCertificate = CertGenerator.GenerateBySelfPfx(new[] { rootCertificateName }, CertService.KEY_SIZE_BITS, validFrom, validTo, PfxFilePath, PfxPassword);

        return RootCertificate != null;
    }

    void ICertificateManager.EnsureRootCertificate()
    {
        if (RootCertificate == null)
        {
            CertificateManager.CreateRootCertificate();
        }

        CertificateManager.TrustRootCertificate(false);
    }

    X509Certificate2? ICertificateManager.LoadRootCertificate()
    {
        try
        {

            var rootCert = new X509Certificate2(PfxFilePath, PfxPassword, X509KeyStorageFlags.Exportable);
            if (rootCert != null && rootCert.NotAfter <= DateTime.Now)
            {
                Log.Error(nameof(CertificateManager), "Loaded root certificate has expired.");
                return null;
            }
            return rootCert;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CertificateManager), ex, nameof(CertificateManager.LoadRootCertificate));
            return null;
        }
    }

    void ICertificateManager.RemoveTrustedRootCertificate(bool machineTrusted = false)
    {
        if (RootCertificate == null)
        {
            throw new Exception("Could not remove certificate as it is null or empty.");
        }

        using var x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

        try
        {
            x509Store.Open(OpenFlags.ReadWrite);

            x509Store.Remove(RootCertificate);
        }
        catch (Exception e)
        {
            Log.Error(nameof(CertificateManager), new Exception("Failed to remove root certificate trust "
                                      + $" for {StoreLocation.CurrentUser} store location. You may need admin rights.", e), nameof(CertificateManager.RemoveTrustedRootCertificate));
        }
        finally
        {
            x509Store.Close();
        }
    }

    void ICertificateManager.TrustRootCertificate(bool machineTrusted = false)
    {
        try
        {
            if (RootCertificate == null)
            {
                throw new Exception("Could not install certificate as it is null or empty.");
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
        catch (Exception)
        {
            Log.Info(nameof(CertificateManager), $"请手动安装CA证书{PfxFilePath}到“将所有的证书都放入下列存储”\\“受信任的根证书颁发机构”");
        }
    }
}
