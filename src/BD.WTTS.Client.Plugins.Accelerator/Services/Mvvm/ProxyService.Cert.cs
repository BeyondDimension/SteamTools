// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial class ProxyService
{
    /// <summary>
    /// 检查根证书，生成，信任，减少 Ipc 往返次数
    /// </summary>
    /// <param name="certificateManager"></param>
    /// <returns></returns>
    StartProxyResultCode CheckRootCertificate(ICertificateManager certificateManager)
    {
        string? cerFilePathLazy = null;
        string? GetCerFilePath()
        {
            if (cerFilePathLazy != null)
                return cerFilePathLazy;
            cerFilePathLazy = certificateManager.GetCerFilePathGeneratedWhenNoFileExists();
            return cerFilePathLazy;
        }

        // 获取证书数据
        var packable = certificateManager.RootCertificatePackable;
        var packable_eq = EqualityComparer<X509CertificatePackable>.Default;
        if (packable_eq.Equals(packable, default)) // 证书为默认值时
        {
            // 生成证书
            var cerFilePath = GetCerFilePath();
            if (cerFilePath == null)
                return StartProxyResultCode.GenerateCerFilePathFail; // 生成证书 Cer 文件路径失败

            // 再次获取证书检查是否为默认值
            packable = certificateManager.RootCertificatePackable;
            if (packable_eq.Equals(packable, default))
            {
                return StartProxyResultCode.GetCertificatePackableFail; // 获取证书数据失败
            }
        }

        X509Certificate2? certificate2 = packable;
        if (certificate2 == null)
            return StartProxyResultCode.GetX509Certificate2Fail;

        bool IsCertificateInstalled()
        {
            // 直接传递平台服务，避免 IPC 调用开销
            var result = ICertificateManager.Constants.IsCertificateInstalled(
                platformService,
                packable);
            return result;
        }

        var isRootCertificateInstalled = IsCertificateInstalled();
        if (!isRootCertificateInstalled)
        {
            // 安装证书
            ICertificateManager.Constants.TrustRootCertificate(
                GetCerFilePath, platformService, certificate2);

            // 安装后检查证书是否已成功安装
            isRootCertificateInstalled = IsCertificateInstalled();
            if (!isRootCertificateInstalled)
                return StartProxyResultCode.TrustRootCertificateFail;
        }

        return StartProxyResultCode.Ok;
    }
}
