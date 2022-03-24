using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Java.Security;
using Javax.Net.Ssl;

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        static IX509TrustManager? X509TrustManager
        {
            get
            {
                var instance = KeyStore.GetInstance("AndroidCAStore");
                if (instance == null) return null;
                instance.Load(null);
                var instance2 = TrustManagerFactory.GetInstance(TrustManagerFactory.DefaultAlgorithm);
                if (instance2 == null) return null;
                instance2.Init(instance);
                var trustManager = instance2.GetTrustManagers()?
                       .Select(x => x is IX509TrustManager x509TrustManager ? x509TrustManager : null)
                       .FirstOrDefault(x => x != null);
                return trustManager;
            }
        }

        internal static bool IsCertificateInstalled(X509Certificate2 certificate2)
        {
            var x509TrustManager = X509TrustManager;
            if (x509TrustManager == null) return false;
            using var certificate = certificate2.Convert();
            var authType = certificate.SigAlgName;
            try
            {
                x509TrustManager.CheckServerTrusted(new[] { certificate }, authType);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        bool IPlatformService.IsCertificateInstalled(X509Certificate2 certificate2) => IsCertificateInstalled(certificate2);
    }
}
