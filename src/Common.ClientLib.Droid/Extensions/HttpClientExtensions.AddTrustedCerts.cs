using Android.Runtime;
using Java.Security.Cert;
using System.IO;
using Xamarin.Android.Net;
using static Java.Security.Cert.X509CertificateHelpers;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// 添加可信任证书，可用于调试时 HTTPS 抓包
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="x509CertificateStream">证书(文件流，内嵌资源流)</param>
        /// <param name="leaveOpen">是否不释放流</param>
        public static void AddTrustedCert(this AndroidClientHandler handler, Stream x509CertificateStream, bool leaveOpen = false)
        {
            try
            {
                using var factory = GetX509CertificateFactory();
                var x509Certificate = factory.GenerateCertificate(x509CertificateStream);
                if (x509Certificate == null)
                    throw new ArgumentNullException(nameof(x509Certificate));
                if (handler.TrustedCerts == null)
                    handler.TrustedCerts = new JavaList<Certificate>();
                handler.TrustedCerts.Add(x509Certificate);
            }
            finally
            {
                if (!leaveOpen) x509CertificateStream.Dispose();
            }
        }
    }
}