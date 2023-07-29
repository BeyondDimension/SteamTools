// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static class CertificateConstants
{
    public const int CertificateValidDays = 300;

    /// <summary>
    /// 证书名称，硬编码不可改动，确保兼容性
    /// </summary>
    public const string CertificateName = "SteamTools";

    public const string RootCertificateName = $"{CertificateName} Certificate";

    /// <summary>
    /// PFX 证书文件名
    /// </summary>
    public const string PfxFileName = $"{CertificateName}.Certificate{FileEx.PFX}";

    /// <summary>
    /// CER 证书文件名
    /// </summary>
    public const string CerFileName = $"{CertificateName}.Certificate{FileEx.CER}";

    /// <summary>
    /// CER 证书导出文件名
    /// </summary>
    public static string CerExportFileName
    {
        get
        {
            var now = DateTime.Now;
            const string f = $"{AssemblyInfo.Trademark}  Certificate {{0}}{FileEx.CER}";
            return string.Format(f, now.ToString(DateTimeFormat.File));
        }
    }

    static string? _AppDataDirectory;

    public static string AppDataDirectory
    {
        get => _AppDataDirectory ?? IOPath.AppDataDirectory;
        set => _AppDataDirectory = value;
    }

    /// <summary>
    /// 默认 PFX 证书文件路径
    /// </summary>
    public static string DefaultPfxFilePath => Path.Combine(AppDataDirectory, PfxFileName);

    /// <summary>
    /// 默认 CER 证书文件路径
    /// </summary>
    public static string DefaultCerFilePath => Path.Combine(AppDataDirectory, CerFileName);
}
