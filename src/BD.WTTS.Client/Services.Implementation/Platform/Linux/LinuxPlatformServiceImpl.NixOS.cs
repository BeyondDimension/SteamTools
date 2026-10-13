#if LINUX

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation
{
    sealed partial class LinuxPlatformServiceImpl
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
        /// 为 NixOS 不可变系统生成系统根证书到指定目录（默认应用执行目录 IOPath.BaseDirectory）。
        /// 生成的 .cer 文件需复制到 nixos-config 仓库的 ssl/ 目录，
        /// flake 输出 sslCertificates.SteamTools 会自动将其加入系统根证书存储。
        /// </summary>
        /// <param name="targetDirectory">证书输出目录</param>
        /// <returns>生成的 .cer 文件完整路径；失败返回 null</returns>
        public static string? GenerateRootCertificateToDirectory(string targetDirectory)
        {
            try
            {
                var pfxFilePath = CertificateConstants.DefaultPfxFilePath;
                X509Certificate2 rootCertificate;

                // NixOS 打包证书源：installPhase 生成的 PFX 经 STEAMTOOLS_BUNDLED_PFX 指向 nix store，
                // 同步到 AppData 后直接加载，保证与系统信任（security.pki.certificateFiles）为同一把密钥。
                BundledCertificateHelper.TrySyncBundledCertificate(pfxFilePath);

                if (!File.Exists(pfxFilePath) || IsRootCertificateExpired(pfxFilePath))
                {
                    // 与 UI 端 CertGenerator.GenerateBySelfPfx 保持完全一致的生成逻辑（请同步维护）
                    var validFrom = DateTime.Today.AddDays(-1);
                    var validTo = DateTime.Today.AddDays(CertificateConstants.CertificateValidDays);
                    rootCertificate = GenerateRootCertificate(validFrom, validTo, pfxFilePath);
                }
                else
                {
                    rootCertificate = new X509Certificate2(pfxFilePath, (string?)null, X509KeyStorageFlags.Exportable);
                }

                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                var cerFilePath = Path.Combine(targetDirectory, CertificateConstants.CerFileName);
                rootCertificate.SaveCerCertificateFile(cerFilePath);
                return cerFilePath;
            }
            catch (Exception e)
            {
                Console.WriteLine($"生成证书失败: {e}");
                return null;
            }
        }

        static bool IsRootCertificateExpired(string pfxFilePath)
        {
            try
            {
                using var cert = new X509Certificate2(pfxFilePath, (string?)null, X509KeyStorageFlags.Exportable);
                return cert.NotAfter <= DateTime.Now;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 与 BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/Services.Implementation/Certificate/CertGenerator.cs
        /// 的 CreateCACertificate 逻辑保持一致，保证 CLI 生成与 UI 生成的是同一套根证书。
        /// </summary>
        static X509Certificate2 GenerateRootCertificate(DateTimeOffset notBefore, DateTimeOffset notAfter, string caPfxPath)
        {
            const string X500DistinguishedNameValue = $"C=CN, O=BeyondDimension, OU=Technical Department, CN={CertificateConstants.RootCertificateName}";
            const int KEY_SIZE_BITS = 2048;

            using var rsa = RSA.Create(KEY_SIZE_BITS);
            var request = new CertificateRequest(new X500DistinguishedName(X500DistinguishedNameValue), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var basicConstraints = new X509BasicConstraintsExtension(true, true, 1, true);
            request.CertificateExtensions.Add(basicConstraints);

            var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, true);
            request.CertificateExtensions.Add(keyUsage);

            var oids = new OidCollection { new("1.3.6.1.5.5.7.3.1"), new("1.3.6.1.5.5.7.3.2") };
            var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
            request.CertificateExtensions.Add(enhancedKeyUsage);

            var dnsBuilder = new SubjectAlternativeNameBuilder();
            try
            {
                dnsBuilder.AddDnsName(CertificateConstants.RootCertificateName);
            }
            catch
            {
                // 与 CertGenerator.Add 保持一致：无法解析为有效 IDN 时忽略 SAN
            }
            request.CertificateExtensions.Add(dnsBuilder.Build());

            var subjectKeyId = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
            request.CertificateExtensions.Add(subjectKeyId);

            var certificate = request.CreateSelfSigned(notBefore, notAfter);
            byte[] exported = certificate.Export(X509ContentType.Pkcs12, (string?)null);
            File.WriteAllBytes(caPfxPath, exported);
            return certificate;
        }
    }
}
#endif
