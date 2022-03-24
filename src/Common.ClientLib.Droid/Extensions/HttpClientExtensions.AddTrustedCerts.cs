using Android.Runtime;
using Java.Security.Cert;
using System.IO;
using Xamarin.Android.Net;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class HttpClientExtensions
    {
        const string certificateFileType = "X.509";

        static readonly Lazy<CertificateFactory?> certificateFactory = new(() =>
        {
            var certificateFactory = CertificateFactory.GetInstance(certificateFileType);
            return certificateFactory;
        });

        internal static CertificateFactory CertificateFactory
        {
            get
            {
                var certificateFactory = HttpClientExtensions.certificateFactory.Value;
                if (certificateFactory == null)
                    throw new NullReferenceException(
                        $"CertificateFactory.GetInstance Fail, Type: {certificateFileType}");
                return certificateFactory;
            }
        }

        /// <summary>
        /// 添加可信任证书，可用于调试时HTTPS抓包
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="certificateFileData">证书文件数据(文件流，内嵌资源流)</param>
        public static void AddTrustedCert(
            this AndroidClientHandler handler,
            Stream certificateFileData)
        {
            var certificate = CertificateFactory.GenerateCertificate(certificateFileData);
            if (certificate == null)
                throw new NullReferenceException("GenerateCertificate Fail");
            if (handler.TrustedCerts == null)
                handler.TrustedCerts = new JavaList<Certificate>();
            handler.TrustedCerts.Add(certificate);
        }

        public static X509Certificate Convert(this X509Certificate2 certificate2)
        {
            using var stream = new MemoryStream(certificate2.RawData);
            return CertificateFactory.GenerateCertificate(stream)!.JavaCast<X509Certificate>();
        }
    }
}