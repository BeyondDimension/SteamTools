// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// NixOS 不可变系统「打包证书源」支持。
/// 在 NixOS 上，系统根证书由 security.pki.certificateFiles 声明式安装（nix store 只读），
/// 打包时（installPhase）生成 CA 证书（cer + pfx）并放入包输出（如 $ssl/），
/// 通过环境变量 STEAMTOOLS_BUNDLED_PFX 指向 store 中的 PFX。
/// 软件加载 CA 证书前调用 <see cref="TrySyncBundledCertificate(string)"/>，
/// 把 store 中的 PFX 同步到 AppData（可写），保证：
///  1. 代理/CLI 使用的私钥与系统信任的 cer 为同一把（rebuild 后同步更新）；
///  2. 未设置该环境变量时行为不变（回退到软件自身生成/复用逻辑）。
/// </summary>
public static class BundledCertificateHelper
{
    /// <summary>
    /// 打包证书源环境变量名（Nix 打包 runScript 中 export）。
    /// </summary>
    public const string EnvName_BundledPfx = "STEAMTOOLS_BUNDLED_PFX";

    /// <summary>
    /// 获取打包证书源 PFX 路径；未设置或文件不存在返回 null。
    /// </summary>
    public static string? GetBundledPfxFilePath()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(EnvName_BundledPfx);
            if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
                return value;
        }
        catch
        {
        }
        return null;
    }

    /// <summary>
    /// 若配置了打包证书源，则把其 PFX 同步复制到 targetPfxFilePath（AppData）。
    /// 返回 true 表示已使用打包证书（target 路径可用）；false 表示无打包证书源，应走原逻辑。
    /// 内容一致时跳过复制（幂等）。
    /// </summary>
    /// <param name="targetPfxFilePath">软件实际使用的 PFX 路径（CertificateConstants.DefaultPfxFilePath）</param>
    public static bool TrySyncBundledCertificate(string targetPfxFilePath)
    {
        try
        {
            var bundledPfxFilePath = GetBundledPfxFilePath();
            if (bundledPfxFilePath == null)
                return false;

            if (!File.Exists(targetPfxFilePath) ||
                !File.ReadAllBytes(targetPfxFilePath).SequenceEqual(File.ReadAllBytes(bundledPfxFilePath)))
            {
                var directory = Path.GetDirectoryName(targetPfxFilePath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                File.Copy(bundledPfxFilePath, targetPfxFilePath, overwrite: true);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
