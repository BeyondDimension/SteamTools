using System.Security.Cryptography.X509Certificates;
using System.Properties;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services;

/// <summary>
/// 证书管理(安装/卸载)
/// </summary>
public interface ICertificateManager
{
    const int CertificateValidDays = 300;

    string? PfxPassword { get; }

    #region FileName

    /// <summary>
    /// PFX 证书文件名
    /// </summary>
    const string PfxFileName = $"{CertificateName}.Certificate{FileEx.PFX}";

    /// <summary>
    /// CER 证书文件名
    /// </summary>
    const string CerFileName = $"{CertificateName}.Certificate{FileEx.CER}";

    /// <summary>
    /// CER 证书导出文件名
    /// </summary>
    static string CerExportFileName
    {
        get
        {
            var now = DateTime.Now;
            const string f = $"{ThisAssembly.AssemblyTrademark}  Certificate {{0}}{FileEx.CER}";
            return string.Format(f, now.ToString(DateTimeFormat.File));
        }
    }

    #endregion

    #region Path

    /// <summary>
    /// 默认 PFX 证书文件路径
    /// </summary>
    static string DefaultPfxFilePath => Path.Combine(IOPath.AppDataDirectory, PfxFileName);

    /// <summary>
    /// 默认 CER 证书文件路径
    /// </summary>
    static string DefaultCerFilePath => Path.Combine(IOPath.AppDataDirectory, CerFileName);

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
    /// 信任根证书
    /// </summary>
    void TrustRootCertificate();

    /// <summary>
    /// 安装根证书，如果没有证书将生成一个新的
    /// </summary>
    /// <returns>返回根证书是否受信任</returns>
    bool SetupRootCertificate();

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
