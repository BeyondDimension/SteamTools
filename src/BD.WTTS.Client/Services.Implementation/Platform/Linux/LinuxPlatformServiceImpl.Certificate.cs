#if LINUX

// ReSharper disable once CheckNamespace

using System.Collections;

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
                //退出码 404 500 被使用
                var shellStr = $"CERT_NAME=\"{CertificateConstants.CertificateName}\"; CERT_RESULT=$(certutil -L -d $HOME/.pki/nssdb | grep \"$CERT_NAME\"); if [ -n \"$CERT_RESULT\" ]; then echo \"证书 '$CERT_NAME' 存在。\"; exit 200; else echo \"证书 '$CERT_NAME' 不存在。\"; exit 10; fi";
                var p = Process.Start(Process2.BinBash, new string[] { "-c", shellStr });
                p.WaitForExit();
                return p.ExitCode == 200;
            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, $"检测证书安装错误:{e}");
                return false;
            }
        }

        public bool? TrustRootCertificateAsync(string cerPath)
        {
            // 使用  Certutil  NSS 工具 添加到 $HOME/.pki/nssdb Chrome 信任此储存区
            Process.Start(Process2.BinBash, new string[] {
                        "-c",
                        $"{Certutil} -A -d $HOME/.pki/nssdb -n \"{CertificateConstants.CertificateName}\" -t C,, -i \"{cerPath}\""
                    }).WaitForExit();
            return RunRootCommand(PkexecPath, new string[] { GetAppHostPath(), "linux", "-ceri", CertificateConstants.AppDataDirectory }) == 0;
        }

        public void RemoveCertificate(byte[] certificate2)
        {
            // 使用  Certutil  NSS 工具 从 $HOME/.pki/nssdb 中删除证书
            Process.Start(Process2.BinBash, new string[] {
                        "-c",
                        $"{Certutil} -D -d $HOME/.pki/nssdb -n \"{CertificateConstants.CertificateName}\""
                    }).WaitForExit();
            RunRootCommand(PkexecPath, new string[] { GetAppHostPath(), "linux", "-cerd", CertificateConstants.AppDataDirectory });
        }

        public static int RunRootCommand(string path, string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            args.ForEach(psi.ArgumentList.Add);
            DotNetRuntimeHelper.AddEnvironment(psi);
            var p = Process.Start(psi);
            p.WaitForExit();
            return p.ExitCode;
        }

        //public void ShowRootCommandError(int code, string msg)
        //{
        //    if (code == 0)
        //        return;
        //    if (Enum.TryParse(code.ToString(), out AuthorizationStatus authStatus))
        //    {
        //        switch (authStatus)
        //        {
        //            case AuthorizationStatus.Canceled:
        //                Toast.Show(ToastIcon.Warning, msg.Format("授权被取消!"));
        //                return;
        //            case AuthorizationStatus.ToolExecuteFailure:
        //            default:
        //                Toast.Show(ToastIcon.Error, msg.Format("执行命令失败!"));
        //                return;

        //        }
        //    }
        //}

        public static bool? TrustRootCertificateCore(string cerPath)
        {
            if (!File.Exists(cerPath))
                return false;
            try
            {
                var path = GetCertStore();
                var destCertFilePath = Path.Combine(path.CaCertStorePath, CertificateConstants.CerFileName);
                if (File.Exists(destCertFilePath) && File.ReadAllBytes(cerPath).SequenceEqual(File.ReadAllBytes(destCertFilePath)))
                {
                    return true;
                }
                var sslCertFilePath = Path.Combine("/etc/ssl/certs", CertificateConstants.CerFileName);
                if (!Directory.Exists(sslCertFilePath))
                    File.Copy(cerPath, sslCertFilePath, overwrite: true);
                if (!Directory.Exists(path.CaCertStorePath))
                    return null;
                File.Copy(cerPath, destCertFilePath, overwrite: true);
                // 刷新系统证书
                Process.Start(path.CaCertUpdatePath).WaitForExit();

                // 如果存在 /bin/trust 则直接用该命令执行
                if (File.Exists(TrustPath))
                {
                    Process.Start(TrustPath, new string[] {
                        "anchor",
                        "--store",
                        cerPath
                    }).WaitForExit();
                }
                return true;
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
                var path = GetCertStore();
                var destCertFilePath = Path.Combine(path.CaCertStorePath, CertificateConstants.CerFileName);
                if (File.Exists(destCertFilePath) && File.ReadAllBytes(cerPath).SequenceEqual(File.ReadAllBytes(destCertFilePath)))
                {
                    File.Delete(destCertFilePath);
                }
                var sslCertFilePath = Path.Combine("/etc/ssl/certs", CertificateConstants.CerFileName);
                if (File.Exists(sslCertFilePath) && File.ReadAllBytes(cerPath).SequenceEqual(File.ReadAllBytes(sslCertFilePath)))
                {
                    File.Delete(sslCertFilePath);
                }

                // 刷新系统证书
                Process.Start(path.CaCertUpdatePath).WaitForExit();
                // 如果存在 /bin/trust 则直接用该命令执行
                if (File.Exists(TrustPath))
                {
                    Process.Start(TrustPath, new string[] {
                        "anchor",
                        "--remove",
                        cerPath
                    }).WaitForExit();
                }
                return;
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

        /// <summary>
        /// 使用安装脚本 会要求前置必须安装 该依赖
        /// </summary>
        const string Certutil = "certutil";

        const string PkexecPath = "pkexec";

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
