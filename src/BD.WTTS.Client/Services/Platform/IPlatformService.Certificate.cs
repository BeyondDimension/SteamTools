// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 判断证书是否已安装
    /// </summary>
    /// <param name="certificate2"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    bool IsCertificateInstalled(X509Certificate2 certificate2)
        => throw new PlatformNotSupportedException();

    /// <summary>
    /// 根据证书文件路径信任根证书
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    ValueTask<bool?> TrustRootCertificate(string filePath)
        => throw new PlatformNotSupportedException();

    /// <summary>
    /// 删除证书
    /// </summary>
    void RemoveCertificate(X509Certificate2 certificate2)
        => throw new PlatformNotSupportedException();
}