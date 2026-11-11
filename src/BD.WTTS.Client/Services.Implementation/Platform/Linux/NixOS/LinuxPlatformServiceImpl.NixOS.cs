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
        /// 最近一次证书导出/加载失败的具体原因（成功时为 null）。
        /// NixOS 上「证书未安装」与「证书文件损坏」在 UI 上是同一条笼统文案，
        /// 这里保留真实原因，由调用方拼进提示。
        /// </summary>
        public static volatile string? LastCertificateError;


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
            {
                LastCertificateError = $"未配置证书导出路径（环境变量 {EnvName_BundledCer} 缺失或指向 /nix/store）";
                Log.Error(nameof(LinuxPlatformServiceImpl), $"NixOS: {LastCertificateError}");
                return false;
            }

            try
            {
                if (!File.Exists(sourceCerFilePath))
                {
                    LastCertificateError = $"AppData 中的 CER 不存在：{sourceCerFilePath}";
                    Log.Error(nameof(LinuxPlatformServiceImpl), $"NixOS: {LastCertificateError}");
                    return false;
                }

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
                    LastCertificateError = null;
                    Log.Info(nameof(LinuxPlatformServiceImpl),
                        $"NixOS: 已导出证书 {sourceCerFilePath} -> {targetCerFilePath}");
                    return true;
                }

                LastCertificateError = $"证书导出后回读校验失败：{targetCerFilePath}";
                Log.Error(nameof(LinuxPlatformServiceImpl), $"NixOS: {LastCertificateError}");
                return false;
            }
            catch (Exception ex)
            {
                LastCertificateError = $"导出证书到 {targetCerFilePath} 失败：{ex.Message}";
                Log.Error(nameof(LinuxPlatformServiceImpl), ex, $"NixOS: {LastCertificateError}");
                return false;
            }
        }

        /// <summary>
        /// NixOS 系统 CA 合并库路径（由 security.pki.certificates 在构建期生成）。
        /// </summary>
        public const string SystemCaBundlePath = "/etc/ssl/certs/ca-certificates.crt";

        /// <summary>
        /// 判断证书是否真的已经进入系统 CA 合并库。
        /// 注意：<see cref="TrySyncBundledCertificate"/> 只代表“已导出到可写路径”，
        /// 而 security.pki.certificates 只在 nixos-rebuild 时求值，两者之间差一次 rebuild。
        /// 若只看导出结果，就会在证书尚未被系统信任时仍然放行加速。
        /// </summary>
        public static bool IsCertificateTrustedBySystem(string cerFilePath)
        {
            try
            {
                if (!File.Exists(cerFilePath))
                {
                    LastCertificateError = $"证书文件不存在：{cerFilePath}";
                    return false;
                }
                if (!File.Exists(SystemCaBundlePath))
                {
                    LastCertificateError = $"系统 CA 合并库不存在：{SystemCaBundlePath}";
                    return false;
                }

                var certBase64 = ExtractCertificateBase64(File.ReadAllBytes(cerFilePath));
                if (certBase64 == null)
                {
                    LastCertificateError = $"无法解析证书内容：{cerFilePath}";
                    return false;
                }

                var bundle = NormalizeBase64(File.ReadAllText(SystemCaBundlePath));
                if (!bundle.Contains(certBase64, StringComparison.Ordinal))
                {
                    LastCertificateError =
                        $"证书已导出但尚未进入系统信任库（{SystemCaBundlePath}）：" +
                        "security.pki.certificates 只在 nixos-rebuild 时求值，需要再执行一次 rebuild";
                    return false;
                }

                LastCertificateError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastCertificateError = $"读取系统 CA 合并库失败：{ex.Message}";
                Log.Error(nameof(LinuxPlatformServiceImpl), ex, $"NixOS: {LastCertificateError}");
                return false;
            }
        }

        /// <summary>
        /// 从 CER 文件取出证书的 base64（兼容 PEM 与 DER 两种写法）。
        /// </summary>
        static string? ExtractCertificateBase64(byte[] data)
        {
            var text = System.Text.Encoding.ASCII.GetString(data);
            const string beginMarker = "BEGIN CERTIFICATE";
            if (text.Contains(beginMarker, StringComparison.Ordinal))
            {
                var begin = text.IndexOf(beginMarker, StringComparison.Ordinal);
                var bodyStart = text.IndexOf('\n', begin);
                var end = text.IndexOf("END CERTIFICATE", StringComparison.Ordinal);
                if (bodyStart < 0 || end <= bodyStart)
                    return null;
                return NormalizeBase64(text.Substring(bodyStart, end - bodyStart));
            }
            return Convert.ToBase64String(data);
        }

        static string NormalizeBase64(string value)
        {
            var sb = new System.Text.StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (!char.IsWhiteSpace(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
#endif
