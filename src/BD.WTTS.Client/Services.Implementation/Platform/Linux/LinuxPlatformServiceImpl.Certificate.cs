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
                var shellStr =
$"""
#!/bin/bash

# 参数：证书名称
CERT_NAME="{CertificateConstants.CertificateName}"

# 检查证书是否存在
CERT_RESULT=$(certutil -L -d sql:$HOME/.pki/nssdb | grep "$CERT_NAME")

if [ -n "$CERT_RESULT" ]; then
    echo "证书 '$CERT_NAME' 存在。"
    exit 200
else
    echo "证书 '$CERT_NAME' 不存在。"
    exit 404
fi
""";
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
            // 使用  Certutil  NSS 工具 添加到 sql:$HOME/.pki/nssdb Chrome 信任此储存区
            Process.Start(Certutil, new string[] {
                        "-A",
                        "-d",
                        "sql:$HOME/.pki/nssdb",
                        "-n",
                        CertificateConstants.CertificateName,
                        "-t",
                        "C,,",
                        "-i",
                        cerPath
                    }).WaitForExit();
            if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
                return false;
            return RunRootCommand(PkexecPath, new string[] { Environment.ProcessPath!, "-ceri", CertificateConstants.AppDataDirectory }) == 0;
        }

        public void RemoveCertificate(byte[] certificate2)
        {
            // 使用  Certutil  NSS 工具 从 sql:$HOME/.pki/nssdb 中删除证书
            Process.Start(Certutil, new string[] {
                        "-D",
                        "-d",
                        "sql:$HOME/.pki/nssdb",
                        "-n",
                        CertificateConstants.CertificateName
                    }).WaitForExit();
            if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
                return;

            RunRootCommand(PkexecPath, new string[] { Environment.ProcessPath!, "-cerd", CertificateConstants.AppDataDirectory });
        }

        public static int RunRootCommand(string path, string[] args)
        {
            var p = Process.Start(path, args);
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
