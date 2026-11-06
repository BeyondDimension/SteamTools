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
        /// 获取打包证书源 PFX 路径；未设置或文件不存在返回 null。
        /// </summary>
        public static string? GetBundledCerFilePath()
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(EnvName_BundledCer);
                if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
                    return value;
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// 从程序 AppData 同步至 NixOS 打包证书源。
        /// 内容一致时跳过复制（幂等）。
        /// </summary>
        /// <param name="targetPfxFilePath">软件实际使用的 CER 路径（CertificateConstants.DefaultCerFilePath）</param>
        public static bool TrySyncBundledCertificate(string targetCerFilePath)
        {
            if (IsNixOS)
            {
                try
                {
                    var bundledCerFilePath = GetBundledCerFilePath();
                    if (bundledCerFilePath == null)
                        return false;

                    if (!File.Exists(bundledCerFilePath) ||
                        !File.ReadAllBytes(targetCerFilePath).SequenceEqual(File.ReadAllBytes(bundledCerFilePath)))
                    {
                        var directory = Path.GetDirectoryName(targetCerFilePath);
                        if (!string.IsNullOrWhiteSpace(directory))
                            Directory.CreateDirectory(directory);
                        File.Copy(targetCerFilePath, bundledCerFilePath, overwrite: true);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
#endif
