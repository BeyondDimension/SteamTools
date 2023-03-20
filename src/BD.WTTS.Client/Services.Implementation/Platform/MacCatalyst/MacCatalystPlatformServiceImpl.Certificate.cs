#if MACOS || MACCATALYST || IOS
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    internal static bool IsCertificateInstalled(X509Certificate2 certificate2)
    {
        return default;
        //using var p = new Process();
        //p.StartInfo.FileName = "security";
        //p.StartInfo.Arguments = $" verify-cert -c \"{IReverseProxyService.Instance.CertificateManager.GetCerFilePathGeneratedWhenNoFileExists()}\"";
        //p.StartInfo.UseShellExecute = false;
        //p.StartInfo.RedirectStandardOutput = true;
        //p.Start();
        //var returnStr = p.StandardOutput.ReadToEnd().TrimEnd();
        //p.Kill();
        //var r = returnStr.Contains("...certificate verification successful.", StringComparison.OrdinalIgnoreCase);
        //return r;

        // XAMARIN_MAC
        //bool result = false;
        //var scer = new SecCertificate(cer);
        //var addCertificate = new SecRecord(scer);
        //var cerTrust = SecKeyChain.QueryAsRecord(addCertificate, out var t2code);
        //if (cerTrust != SecStatusCode.ItemNotFound)
        //{
        //    using (var trust = new SecTrust(cerTrust, null))
        //    {
        //        trust.SetPolicy(policy);
        //        trust.SetAnchorCertificates(fcollection);
        //        result = trust.Evaluate(out var error);
        //        Toast.Show(error.Description);
        //    }
        //}
        //return result;
    }

    bool IPlatformService.IsCertificateInstalled(X509Certificate2 certificate2)
        => IsCertificateInstalled(certificate2);

    async ValueTask<bool?> IPlatformService.TrustRootCertificateAsync(string filePath)
    {
        var script = $"security add-trusted-cert -d -r trustRoot -k /Users/{Environment.UserName}/Library/Keychains/login.keychain-db";
        TextBoxWindowViewModel vm = new()
        {
            Title = AppResources.MacTrustRootCertificateTips,
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            Description = AppResources.MacSudoPasswordTips + $"\r\n sudo {script} \"{filePath}\"",
        };
        var scriptContent = $"osascript -e 'tell app \"Terminal\" to do script \"sudo -S {script} \\\"{filePath}\\\"\"'";
        var msg = UnixHelper.RunShell(scriptContent.ToString());
        if (await TextBoxWindowViewModel.ShowDialogAsync(vm) == null)
            return null;
        if (!string.IsNullOrWhiteSpace(msg))
        {
            Toast.Show(msg);
            return false;
        }
        return true;
    }

    async void IPlatformService.RemoveCertificate(X509Certificate2 certificate2)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
            store.Open(OpenFlags.ReadWrite);
            store.Remove(certificate2);
        }
        catch
        {
            // 出现错误尝试命令删除
            await RunShellAsync(
                $"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
        }
        // XAMARIN_MAC
        //var itemCertificate = new SecRecord(new SecCertificate(certificate2));
        //var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
        //if (code != SecStatusCode.ItemNotFound)
        //{
        //    var rcode = SecKeyChain.Remove(cers);
        //    if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
        //    {
        //        await RunShellAsync($"security delete-certificate -Z {certificate2.GetCertHashString()}", true);
        //    }
        //}
        ////using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        ////store.Open(OpenFlags.ReadOnly);
        ////var lisrts = store.Certificates.Find(X509FindType.FindByIssuerName, IHttpProxyService.RootCertificateName, false);
        ////foreach (var item in lisrts)
        ////{
        ////    var ces2 = new SecCertificate(item);
        ////    var itemCertificate = new SecRecord(ces2);
        ////    var cers = SecKeyChain.QueryAsRecord(itemCertificate, out SecStatusCode code);
        ////    if (code != SecStatusCode.ItemNotFound)
        ////    {
        ////        var rcode = SecKeyChain.Remove(cers);
        ////        if (rcode != SecStatusCode.Success && rcode != SecStatusCode.ItemNotFound)
        ////        {
        ////            await RunShellAsync($"security delete-certificate -Z {item.GetCertHashString()}", true);
        ////        }
        ////    }
        ////}

    }
}
#endif