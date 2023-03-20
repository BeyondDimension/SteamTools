// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 证书管理(安装/卸载)
/// </summary>
public interface ICertificateManager
{
    const int CertificateValidDays = CertificateConstants.CertificateValidDays;

    string? PfxPassword { get; }

    /// <summary>
    /// 证书名称，硬编码不可改动，确保兼容性
    /// </summary>
    const string CertificateName = CertificateConstants.CertificateName;

    const string RootCertificateName = CertificateConstants.RootCertificateName;

    #region FileName

    /// <summary>
    /// PFX 证书文件名
    /// </summary>
    const string PfxFileName = CertificateConstants.PfxFileName;

    /// <summary>
    /// CER 证书文件名
    /// </summary>
    const string CerFileName = CertificateConstants.CerFileName;

    /// <summary>
    /// CER 证书导出文件名
    /// </summary>
    static string CerExportFileName => CertificateConstants.CerExportFileName;

    #endregion

    #region Path

    /// <summary>
    /// 默认 PFX 证书文件路径
    /// </summary>
    static string DefaultPfxFilePath => CertificateConstants.DefaultPfxFilePath;

    /// <summary>
    /// 默认 CER 证书文件路径
    /// </summary>
    static string DefaultCerFilePath => CertificateConstants.DefaultCerFilePath;

    /// <summary>
    /// PFX 证书文件路径
    /// </summary>
    string PfxFilePath => DefaultPfxFilePath;

    /// <summary>
    /// CER 证书文件路径
    /// </summary>
    string CerFilePath => DefaultCerFilePath;

    #endregion

    /// <summary>
    /// 获取当前 Root 证书
    /// </summary>
    X509Certificate2? RootCertificate { get; set; }

    /// <summary>
    /// 获取 Cer 证书路径，当不存在时生成文件后返回路径
    /// </summary>
    /// <returns></returns>
    string? GetCerFilePathGeneratedWhenNoFileExists();

    /// <summary>
    /// 由平台实现的信任根证书引导，有 Root 权限将尝试执行信任，否则则 UI 引导，跳转网页或弹窗
    /// </summary>
    ValueTask PlatformTrustRootCertificateGuideAsync();

    /// <summary>
    /// 安装根证书，如果没有证书将生成一个新的
    /// </summary>
    /// <returns>返回根证书是否受信任</returns>
    ValueTask<bool> SetupRootCertificateAsync();

    /// <summary>
    /// 删除根证书，如果没有证书将返回 <see langword="true"/>
    /// </summary>
    /// <returns></returns>
    bool DeleteRootCertificate();

    /// <summary>
    /// 当前根证书是否已安装并信任
    /// </summary>
    bool IsRootCertificateInstalled { get; }
}