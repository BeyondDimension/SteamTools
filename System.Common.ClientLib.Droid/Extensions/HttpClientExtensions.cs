using Android.Runtime;
using Java.Security.Cert;
using System.IO;
using Xamarin.Android.Net;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 添加可信任证书，可用于调试时HTTPS抓包
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="certificateFileData">证书文件数据(文件流，内嵌资源流)</param>
        /// <param name="certificateFileType">证书文件类型，默认使用X.509</param>
        public static void AddTrustedCerts(
            this AndroidClientHandler handler,
            Stream certificateFileData,
            string certificateFileType = "X.509")
        {
            var certificateFactory = CertificateFactory.GetInstance(certificateFileType);
            if (certificateFactory == null)
                throw new NullReferenceException(
                    $"CertificateFactory.GetInstance Fail, Type: {certificateFileType}");
            var certificate = certificateFactory.GenerateCertificate(certificateFileData);
            if (certificate == null)
                throw new NullReferenceException("GenerateCertificate Fail");
            if (handler.TrustedCerts == null)
                handler.TrustedCerts = new JavaList<Certificate>();
            handler.TrustedCerts.Add(certificate);
        }
    }
}