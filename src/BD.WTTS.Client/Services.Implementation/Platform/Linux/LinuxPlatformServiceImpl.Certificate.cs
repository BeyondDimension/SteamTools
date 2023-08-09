#if LINUX

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation
{
    partial class LinuxPlatformServiceImpl
    {

        public bool IsCertificateInstalled(byte[] certificate2)
        {
            var certificate2_ = Serializable.DMP2<X509CertificatePackable>(certificate2);
            return IsCertificateInstalledCore(certificate2_);
        }

        public bool IsCertificateInstalledCore(X509CertificatePackable certificate2)
        {
            try
            {
                X509Certificate2? cert = certificate2;
                if (cert == null)
                    return false;
                var path = GetCertStore();
                var destCertFilePath = Path.Combine(path.CaCertStorePath, CertificateConstants.CerFileName);
                return File.Exists(destCertFilePath) && cert.GetRawCertData().SequenceEqual(File.ReadAllBytes(destCertFilePath));

            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, $"安装证书错误:{e}");
                return false;
            }
        }

        public static bool? TrustRootCertificateAsync(string cerPath)
        {
            return false;
        }

        public static bool? TrustRootCertificateCore(string cerPath)
        {
            if (!File.Exists(cerPath))
                return false;
            try
            {
                // 如果存在 /bin/trust 则直接用该命令执行
                if (File.Exists(TrustPath))
                {
                    Process.Start(TrustPath, new string[] {
                        "anchor",
                        "--store",
                        cerPath
                    }).WaitForExit();
                    return true;
                }
                else
                {
                    var path = GetCertStore();
                    var destCertFilePath = Path.Combine(path.CaCertStorePath, CertificateConstants.CerFileName);
                    if (File.Exists(destCertFilePath) && File.ReadAllBytes(cerPath).SequenceEqual(File.ReadAllBytes(destCertFilePath)))
                    {
                        return true;
                    }
                    if (!Directory.Exists(path.CaCertStorePath))
                        return null;
                    File.Copy(cerPath, destCertFilePath, overwrite: true);
                    // 刷新系统证书
                    Process.Start(path.CaCertUpdatePath).WaitForExit();
                    return true;
                }
            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, $"安装证书错误:{e}");
                return false;
            }
        }

        public static void RemoveCertificate(string cerPath)
        {
            // 不存在证书则直接跳过
            if (!File.Exists(cerPath))
                return;
            try
            {
                // 如果存在 /bin/trust 则直接用该命令执行
                if (File.Exists(TrustPath))
                {
                    Process.Start(TrustPath, new string[] {
                        "anchor",
                        "--remove",
                        cerPath
                    }).WaitForExit();
                    return;
                }
                else
                {
                    var path = GetCertStore();
                    var destCertFilePath = Path.Combine(path.CaCertStorePath, CertificateConstants.CerFileName);
                    if (File.Exists(destCertFilePath) && File.ReadAllBytes(cerPath).SequenceEqual(File.ReadAllBytes(destCertFilePath)))
                    {
                        File.Delete(destCertFilePath);
                    }
                    // 刷新系统证书
                    Process.Start(path.CaCertUpdatePath).WaitForExit();
                    return;
                }
            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, $"删除证书错误:{e}");
                return;
            }
        }

        public static (string CaCertUpdatePath, string CaCertStorePath) GetCertStore()
        {
            if (File.Exists(DebianCaCertUpdatePath))
            {
                return (DebianCaCertUpdatePath, DebianCaCertStorePath);
            }
            else
            {
                return (RedHatCaCertUpdatePath, RedHatCaCertStorePath);
            }
        }

        #region trust
        const string TrustPath = "/usr/bin/trust";
        #endregion

        #region Debian
        const string DebianCaCertUpdatePath = "/usr/sbin/update-ca-certificates";

        const string DebianCaCertStorePath = "/usr/local/share/ca-certificates";

        #endregion

        #region RedHat
        const string RedHatCaCertUpdatePath = "/usr/bin/update-ca-trust";

        const string RedHatCaCertStorePath = "/etc/pki/ca-trust/source/anchors";
        #endregion
    }
}

#endif
