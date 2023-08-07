#if MACOS || MACCATALYST || IOS
using Security;
using SteamKit2;
using static System.CommandLine.Help.HelpBuilder;
using AppResources = BD.WTTS.Client.Resources.Strings;
using Authorization = Security.Authorization;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
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
            using (var trust = new SecTrust(cert, null))
            {
                result = trust.Evaluate(out var error);
#if DEBUG
                if (error != null)
                    Toast.Show(ToastIcon.Error, error.Description);
#endif
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

    public async ValueTask<bool?> TrustRootCertificateAsync(string filePath)
    {
        //信任系统证书
        var script = $"security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain";
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
        var code = RunRootCommand(script);
        ShowRootCommandError(code, "安装证书：{0}");
        return code == 0;
    }

    //public static 

    public int RunRootCommand(string arg)
    {
        var defaults = AuthorizationFlags.Defaults;
        using (var auth = Authorization.Create(defaults))
        {
            if (auth == null)
                Toast.Show(ToastIcon.Error, "获取授权出错");
            var args = new[] { "-c", $"\"\"{arg}\"\"" };
            return auth.ExecuteWithPrivileges("/bin/sh", defaults, args);
            //return auth.ExecuteWithPrivileges(IOPath.BaseDirectory, AuthorizationFlags.Defaults, args);
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
        var code = RunRootCommand($"security delete-certificate -Z {cert.GetCertHashString()}");
        ShowRootCommandError(code, "安装证书：{0}");
        return;
        //if (certificate2 == null) return;
        //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        //try
        //{
        //    store.Open(OpenFlags.ReadWrite);
        //    store.Remove(certificate2);
        //}
        //catch
        //{
        //    // 出现错误尝试命令删除
        //    await RunShellAsync(
        //        $"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
        //}
        // XAMARIN_MAC
        //X509Certificate2? cert = certificate2;
        //if (cert == null)
        //    return;

        //var itemCertificate = new SecRecord(new SecCertificate(cert));
        //var queryCer = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
        //if (code != SecStatusCode.ItemNotFound)
        //{
        //    if (queryCer == null)
        //        return;
        //    var rCode = SecKeyChain.Remove(queryCer);
        //    if (rCode != SecStatusCode.Success && rCode != SecStatusCode.ItemNotFound)
        //    {
        //        // await RunShellAsync($"security delete-certificate -Z {cert.GetCertHashString()}", true);
        //    }
        //}
        //else
        //{
        //    // Toast.Show(ToastIcon.Error, "证书删除失败，找不到证书。");
        //}
        //using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        //store.Open(OpenFlags.ReadOnly);
        //var lisrts = store.Certificates.Find(X509FindType.FindByIssuerName, IHttpProxyService.RootCertificateName, false);
        //foreach (var item in lisrts)
        //{
        //    var ces2 = new SecCertificate(item);
        //    var itemCertificate = new SecRecord(ces2);
        //    var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
        //    if (code != SecStatusCode.ItemNotFound)
        //    {
        //        var rcode = SecKeyChain.Remove(cers);
        //        if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
        //        {
        //            await RunShellAsync($"security delete-certificate -Z {item.GetCertHashString()}", true);
        //        }
        //    }
        //}

    }
}
#endif