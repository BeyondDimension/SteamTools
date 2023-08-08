#if MACOS || MACCATALYST
using Security;
using Authorization = Security.Authorization;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    const string SecurityPath = "/usr/bin/security";

    internal static bool IsCertificateInstalledCore(X509CertificatePackable certificate2)
    {
        //using var p = new Process();
        //p.StartInfo.FileName = "security";
        //p.StartInfo.Arguments = $" verify-cert -c \"{IReverseProxyService.Constants.Instance.CertificateManager.GetCerFilePathGeneratedWhenNoFileExists()}\"";
        //p.StartInfo.UseShellExecute = false;
        //p.StartInfo.RedirectStandardOutput = true;
        //p.Start();
        //var returnStr = p.StandardOutput.ReadToEnd().TrimEnd();
        //p.Kill();
        //var r = returnStr.Contains("...certificate verification successful.", StringComparison.OrdinalIgnoreCase);
        //return r;
        // XAMARIN_MAC
        bool result = false;
        X509Certificate2? cert = certificate2;
        if (cert == null)
            return result;
        var scer = new SecCertificate(cert);
        var addCertificate = new SecRecord(scer);
        var cerTrust = SecKeyChain.QueryAsRecord(addCertificate, out var t2Code);
        if (t2Code != SecStatusCode.ItemNotFound)
        {
            try
            {
                using (var trust = new SecTrust(cert, null))
                {
                    result = trust.Evaluate(out var error);
#if DEBUG
                    // 不显示错误 
                    if (error != null)
                        Toast.Show(ToastIcon.Error, error.Description);
#endif
                }
            }
            catch
            {
                //任何异常视为 False
                return false;
            }
        }
        return result;
    }

    public bool IsCertificateInstalled(byte[] certificate2)
    {
        var certificate2_ = Serializable.DMP2<X509CertificatePackable>(certificate2);
        return IsCertificateInstalledCore(certificate2_);
    }

    public bool IsCertificateInstalled(X509CertificatePackable certificate2)
          => IsCertificateInstalledCore(certificate2);

    public bool? TrustRootCertificateAsync(string filePath)
    {
        //信任系统证书
        //var script = $"security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain \"{filePath}\"";
        //TextBoxWindowViewModel vm = new()
        //{
        //    Title = AppResources.MacTrustRootCertificateTips,
        //    InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        //    Description = AppResources.MacSudoPasswordTips + $"\r\n sudo {script} \"{filePath}\"",
        //};
        //var scriptContent = $"osascript -e 'tell app \"Terminal\" to do script \"sudo -S {script} \\\"{filePath}\\\"\"'";
        //var msg = UnixHelper.RunShell(scriptContent.ToString());
        //if (await TextBoxWindowViewModel.ShowDialogAsync(vm) == null)
        //    return null;
        //if (!string.IsNullOrWhiteSpace(msg))
        //{
        //    Toast.Show(ToastIcon.None, msg);
        //    return false;
        //}
        //路径不需要包裹 参数传入不需要像控制台包裹
        var code = RunRootCommand(SecurityPath, new string[] {
                   "add-trusted-cert",
                   "-d",
                   "-r",
                   "trustRoot",
                   "-k",
                   "/Library/Keychains/System.keychain",
                   filePath
                   });
        ShowRootCommandError(code, "安装证书：{0}");
        return code == 0;
    }

    //public static int TrustRootCertificate(X509CertificatePackable certificate2)
    //{
    //    //X509Certificate2? cert = certificate2;
    //    //if (cert == null)
    //    //    return (int)CommandExitCode.HttpStatusCodeInternalServerError;
    //    //var scert = new SecCertificate(cert);
    //    //var itemCertificate = new SecRecord(scert);
    //    //var queryCer = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
    //    //if (code == SecStatusCode.ItemNotFound)
    //    //{
    //    //    var rCode = SecKeyChain.Add(itemCertificate);
    //    //    Console.WriteLine(rCode);
    //    //    //using (var st = new SecTrust(scert, SecPolicy.CreateBasicX509Policy()))
    //    //    //{
    //    //    //    st.
    //    //    //}
    //    //}
    //    //else
    //    //{
    //    //    return (int)CommandExitCode.HttpStatusCodeInternalServerError;
    //    //    // Toast.Show(ToastIcon.Error, "证书删除失败，找不到证书。");
    //    //}
    //    //return 0;
    //}

    public int RunRootCommand(string path, string[] args)
    {
        var defaults = AuthorizationFlags.Defaults;
        using (var auth = Authorization.Create(defaults))
        {
            if (auth == null)
                Toast.Show(ToastIcon.Error, "获取授权出错");
            Console.WriteLine($"{path} {string.Join(" ", args)}");
            return auth.ExecuteWithPrivileges(path!, AuthorizationFlags.Defaults, args);
        }
    }

    public void ShowRootCommandError(int code, string msg)
    {
        if (code == 0)
            return;
        if (Enum.TryParse(code.ToString(), out AuthorizationStatus authStatus))
        {
            switch (authStatus)
            {
                case AuthorizationStatus.Canceled:
                    Toast.Show(ToastIcon.Warning, msg.Format("授权被取消!"));
                    return;
                case AuthorizationStatus.ToolExecuteFailure:
                default:
                    Toast.Show(ToastIcon.Error, msg.Format("执行命令失败!"));
                    return;

            }
        }
    }

    public void RemoveCertificate(byte[] certificate2)
    {
        var certificate2_ = Serializable.DMP2<X509CertificatePackable>(certificate2);
        RemoveCertificate(certificate2_);
    }

    public void RemoveCertificate(X509CertificatePackable certificate2)
    {
        X509Certificate2? cert = certificate2;
        if (cert == null)
            return;
        // -t 删除证书信任 -Z 删除证书  -c 根据名字删除证书
        var code = RunRootCommand(SecurityPath, new string[] { "delete-certificate", "-t", "-Z", cert.GetCertHashStringCompat(HashAlgorithmName.SHA1) });
        ShowRootCommandError(code, "删除证书：{0}");
    }
    //public static int RemoveCertificate(X509CertificatePackable certificate2)
    //{
    //    //X509Certificate2? cert = certificate2;
    //    //if (cert == null)
    //    //    return;
    //    //var code = RunRootCommand($"security delete-certificate -Z {cert.GetCertHashStringCompat(HashAlgorithmName.SHA1)}");
    //    //ShowRootCommandError(code, "安装证书：{0}");
    //    //return;
    //    //if (certificate2 == null) return;
    //    //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    //    //try
    //    //{
    //    //    store.Open(OpenFlags.ReadWrite);
    //    //    store.Remove(certificate2);
    //    //}
    //    //catch
    //    //{
    //    //    // 出现错误尝试命令删除
    //    //    await RunShellAsync(
    //    //        $"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
    //    //}
    //    // XAMARIN_MAC
    //    X509Certificate2? cert = certificate2;
    //    if (cert == null)
    //        return (int)CommandExitCode.HttpStatusCodeInternalServerError;

    //    var itemCertificate = new SecRecord(new SecCertificate(cert));
    //    var queryCer = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
    //    if (code != SecStatusCode.ItemNotFound)
    //    {
    //        if (queryCer == null)
    //            return (int)CommandExitCode.HttpStatusCodeInternalServerError;
    //        var rCode = SecKeyChain.Remove(queryCer);
    //        if (rCode != SecStatusCode.Success && rCode != SecStatusCode.ItemNotFound)
    //        {
    //            return (int)CommandExitCode.HttpStatusCodeInternalServerError;
    //            // await RunShellAsync($"security delete-certificate -Z {cert.GetCertHashString()}", true);
    //        }
    //        return (int)CommandExitCode.HttpStatusCodeOk;
    //    }
    //    else
    //    {
    //        return (int)CommandExitCode.HttpStatusCodeInternalServerError;
    //        // Toast.Show(ToastIcon.Error, "证书删除失败，找不到证书。");
    //    }
    //    //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    //    //store.Open(OpenFlags.ReadOnly);
    //    //var lisrts = store.Certificates.Find(X509FindType.FindByIssuerName, IHttpProxyService.RootCertificateName, false);
    //    //foreach (var item in lisrts)
    //    //{
    //    //    var ces2 = new SecCertificate(item);
    //    //    var itemCertificate = new SecRecord(ces2);
    //    //    var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
    //    //    if (code != SecStatusCode.ItemNotFound)
    //    //    {
    //    //        var rcode = SecKeyChain.Remove(cers);
    //    //        if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
    //    //        {
    //    //            await RunShellAsync($"security delete-certificate -Z {item.GetCertHashString()}", true);
    //    //        }
    //    //    }
    //    //}

    //}
}
#endif