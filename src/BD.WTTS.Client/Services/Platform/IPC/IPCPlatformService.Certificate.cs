// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <summary>
    /// 判断证书是否已安装
    /// </summary>
    /// <param name="certificate2"></param>
    /// <returns></returns>
    bool IsCertificateInstalled(byte[] certificate2)
        => throw new PlatformNotSupportedException();

    /// <summary>
    /// 根据证书文件路径信任根证书
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool? TrustRootCertificateAsync(string filePath)
        => throw new PlatformNotSupportedException();

    /// <summary>
    /// 删除证书
    /// </summary>
    void RemoveCertificate(byte[] certificate2)
        => throw new PlatformNotSupportedException();
}

public static partial class IPCPlatformServiceExtensions
{
    /// <inheritdoc cref="IPCPlatformService.IsCertificateInstalled(byte[])"/>
    public static bool IsCertificateInstalled(
        this IPCPlatformService platformService,
        X509CertificatePackable certificate2)
    {
        var certificate2_ = Serializable.SMP2(certificate2);
        return platformService.IsCertificateInstalled(certificate2_);
    }

    /// <inheritdoc cref="IPCPlatformService.RemoveCertificate(byte[])"/>
    public static void RemoveCertificate(
        this IPCPlatformService platformService,
        X509CertificatePackable certificate2)
    {
        var certificate2_ = Serializable.SMP2(certificate2);
        platformService.RemoveCertificate(certificate2_);
    }
}