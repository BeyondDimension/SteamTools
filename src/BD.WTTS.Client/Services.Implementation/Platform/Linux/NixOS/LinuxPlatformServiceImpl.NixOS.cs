#if LINUX

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation
{
    sealed partial class LinuxPlatformServiceImpl : IPlatformService
    {
        /// <summary>
        /// 当前是否为 NixOS 系统。
        /// NixOS 为不可变系统，/etc/ssl/certs 与系统 CA 证书存储均为只读（Nix store 符号链接），
        /// 无法在运行时写入系统证书，需通过 flake 输出 sslCertificates.SteamTools
        /// （security.pki.certificateFiles）声明式安装到系统根证书存储。
        /// </summary>
        public static bool IsNixOS { get; } = DetectNixOS();

        static bool DetectNixOS()
        {
            try
            {
                // NixOS 标记文件
                if (File.Exists("/etc/NIXOS"))
                    return true;
                // 或 /etc/os-release 中的 ID=nixos
                if (File.Exists("/etc/os-release"))
                {
                    var osRelease = File.ReadAllText("/etc/os-release");
                    return osRelease.Contains("ID=nixos", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 打包证书源环境变量名（Nix 打包 runScript 中 export）。
        /// </summary>
        public const string EnvName_BundledCer = "STEAMTOOLS_BUNDLED_CER";

        /// <summary>
        /// 获取证书导出目标路径（由 Nix 打包 wrapper / systemd 服务通过
        /// <see cref="EnvName_BundledCer"/> 指定，例如 /var/lib/watt-toolkit/SteamTools.Certificate.cer）。
        /// 未设置、或仍指向只读的 /nix/store 时返回 null（视为“未配置导出”）。
        /// 注意：这里不要求目标文件已存在——首次启动时它还不存在，正是由本进程生成。
        /// </summary>
        public static string? GetBundledCerFilePath()
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(EnvName_BundledCer);
                if (string.IsNullOrWhiteSpace(value))
                    return null;
                var path = Path.GetFullPath(value.Trim());
                // /nix/store 为只读挂载，写入必然失败
                if (path.StartsWith("/nix/store/", StringComparison.Ordinal))
                    return null;
                return path;
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// 把程序在 AppData 中运行时生成的 CER 导出到模块声明的可写路径
        /// （AppData → 证书源，方向与维护者要求一致：私钥留在 AppData，仅导出公钥证书）。
        /// 内容一致时跳过复制（幂等），复制后回读校验。
        /// </summary>
        /// <param name="sourceCerFilePath">程序实际使用的 CER（CertificateConstants.DefaultCerFilePath）</param>
        public static bool TrySyncBundledCertificate(string sourceCerFilePath)
        {
            if (!IsNixOS)
                return false;

            var targetCerFilePath = GetBundledCerFilePath();
            if (targetCerFilePath == null)
                return false;

            try
            {
                if (!File.Exists(sourceCerFilePath))
                    return false;

                if (!File.Exists(targetCerFilePath) ||
                    !File.ReadAllBytes(sourceCerFilePath).SequenceEqual(File.ReadAllBytes(targetCerFilePath)))
                {
                    // 建的是目标文件所在目录（不是源文件目录）
                    var directory = Path.GetDirectoryName(targetCerFilePath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    File.Copy(sourceCerFilePath, targetCerFilePath, overwrite: true);
                }

                // 回读校验：文件存在且内容一致才算成功
                if (File.Exists(targetCerFilePath) &&
                    File.ReadAllBytes(sourceCerFilePath).SequenceEqual(File.ReadAllBytes(targetCerFilePath)))
                {
                    Log.Info(nameof(LinuxPlatformServiceImpl),
                        $"NixOS: 已导出证书 {sourceCerFilePath} -> {targetCerFilePath}");
                    return true;
                }

                Log.Error(nameof(LinuxPlatformServiceImpl),
                    $"NixOS: 证书导出后回读校验失败 {targetCerFilePath}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(nameof(LinuxPlatformServiceImpl), ex,
                    $"NixOS: 导出证书到 {targetCerFilePath} 失败");
                return false;
            }
        }
    }
}
#endif
