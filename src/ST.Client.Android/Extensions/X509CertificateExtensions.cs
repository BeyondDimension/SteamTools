using Android.Runtime;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using static Java.Security.Cert.X509CertificateHelpers;
using JX509Certificate = Java.Security.Cert.X509Certificate;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class X509CertificateExtensions
    {
        /// <summary>
        /// 将 .NET 中的 X509 证书对象 转换为 JVM 中的 X509 证书对象
        /// <para>Java.Security.Cert.X509Certificate</para>
        /// <para>=></para>
        /// <para>System.Security.Cryptography.X509Certificates.X509Certificate2</para>
        /// </summary>
        /// <param name="certificate2"></param>
        /// <returns></returns>
        public static JX509Certificate Convert(this X509Certificate2 certificate2)
        {
            using var x509CertificateStream = new MemoryStream(certificate2.RawData);
            using var factory = GetX509CertificateFactory();
            var x509Certificate = factory.GenerateCertificate(x509CertificateStream);
            if (x509Certificate == null)
                throw new ArgumentNullException(nameof(x509Certificate));
            return x509Certificate.JavaCast<JX509Certificate>();
        }
    }
}

namespace Java.Security.Cert
{
    internal static partial class X509CertificateHelpers
    {
        const string certificateFileType = "X.509";

        public static CertificateFactory GetX509CertificateFactory()
        {
            var x509CertificateFactory = CertificateFactory.GetInstance(certificateFileType);
            return x509CertificateFactory ?? throw new ArgumentNullException(nameof(x509CertificateFactory));
        }
    }
}
