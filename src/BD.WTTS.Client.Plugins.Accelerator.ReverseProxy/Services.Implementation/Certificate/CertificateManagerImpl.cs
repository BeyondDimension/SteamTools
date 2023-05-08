#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class CertificateManagerImpl : ICertificateManager
{
    const string TAG = "CertificateManager";

    ICertificateManager Interface => this;

    readonly IPCSubProcessService ipc;
    readonly IPCPlatformService platformService;
    readonly IPCToastService toast;

    public CertificateManagerImpl(IPCSubProcessService ipc)
    {
        this.ipc = ipc;
        platformService = ipc.GetService<IPCPlatformService>().ThrowIsNull(nameof(platformService));
        toast = ipc.GetService<IPCToastService>().ThrowIsNull(nameof(toast));
    }

    /// <inheritdoc cref="ICertificateManager.RootCertificate"/>
    public X509Certificate2? RootCertificate { get; set; }

    /// <inheritdoc cref="ICertificateManager.PfxPassword"/>
    public string? PfxPassword { get; set; }

    X509Certificate2? LoadRootCertificate()
    {
        try
        {
            ICertificateManager thiz = Interface;
            if (!File.Exists(thiz.PfxFilePath))
                return null;
            X509Certificate2 rootCert;
            try
            {
                rootCert = new(thiz.PfxFilePath, thiz.PfxPassword, X509KeyStorageFlags.Exportable);
            }
            catch (PlatformNotSupportedException)
            {
                // https://github.com/dotnet/runtime/issues/71603
                return null;
            }
            catch (CryptographicException e)
            {
                if (e.InnerException is PlatformNotSupportedException) return null;
                throw;
            }
            if (rootCert.NotAfter <= DateTime.Now)
            {
                Log.Error(TAG, "Loaded root certificate has expired.");
                return null;
            }
            return rootCert;
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, nameof(LoadRootCertificate));
            return null;
        }
    }

    void SharedTrustRootCertificate()
    {
        if (RootCertificate == null)
        {
            throw new ApplicationException(
                "Could not install certificate as it is null or empty.");
        }

        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        try
        {
            store.Open(OpenFlags.ReadWrite);

            //var subjectName = RootCertificate.Subject[3..];
            //foreach (var item in store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            //{
            //    if (item.Thumbprint != RootCertificate.Thumbprint)
            //    {
            //        store.Remove(item);
            //    }
            //}

            if (store.Certificates.Find(X509FindType.FindByThumbprint, RootCertificate.Thumbprint, true).Count == 0)
            {
                store.Add(RootCertificate);
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                $"Please manually install the CA certificate {Interface.PfxFilePath} to a trusted root certificate authority.");
        }
        finally
        {
            store.Close();
        }
    }

    bool SharedCreateRootCertificate()
    {
        RootCertificate ??= LoadRootCertificate();

        if (RootCertificate != null)
        {
            return true;
        }

        var validFrom = DateTime.Today.AddDays(-1);
        var validTo = DateTime.Today.AddDays(ICertificateManager.CertificateValidDays);

        var rootCertificateName = ICertificateManager.RootCertificateName;

        RootCertificate = CertGenerator.GenerateBySelfPfx(new[] { rootCertificateName }, validFrom, validTo, Interface.PfxFilePath, Interface.PfxPassword);

        return RootCertificate != null;
    }

    void SharedRemoveTrustedRootCertificate()
    {
        if (RootCertificate == null)
        {
            throw new ApplicationException(
                "Could not remove certificate as it is null or empty.");
        }

        using var x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

        try
        {
            x509Store.Open(OpenFlags.ReadWrite);
            var subjectName = RootCertificate.Subject[3..];
            foreach (var item in x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            {
                //if (item.Thumbprint == RootCertificate.Thumbprint)
                //{
                x509Store.Remove(item);
                //}
            }
            //x509Store.Remove(RootCertificate);
        }
        catch (Exception e)
        {
            Log.Error(TAG, new ApplicationException(
                "Failed to remove root certificate trust for LocalMachine store location. You may need admin rights.", e), nameof(SharedRemoveTrustedRootCertificate));
        }
        finally
        {
            x509Store.Close();
        }
    }

    static readonly object lockGenerateCertificate = new();

    /// <inheritdoc cref="ICertificateManager.GetCerFilePathGeneratedWhenNoFileExists"/>
    public string? GetCerFilePathGeneratedWhenNoFileExists()
    {
        var filePath = Interface.CerFilePath;
        lock (lockGenerateCertificate)
        {
            if (!File.Exists(filePath))
            {
                if (!GenerateCertificateUnlock(filePath)) return null;
            }
            return filePath;
        }
    }

    /// <summary>
    /// (❌🔒)生成 Root 证书
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool GenerateCertificateUnlock(string filePath)
    {
        var result = SharedCreateRootCertificate();
        if (!result || RootCertificate == null)
        {
            Log.Error(TAG, "Failed to create certificate");
            toast.Show(IPCToastService.ToastText.CreateCertificateFaild);
            return false;
        }

        RootCertificate.SaveCerCertificateFile(filePath);

        return true;
    }

    /// <summary>
    /// (✔️🔒)生成 Root 证书
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool GenerateCertificate(string? filePath = null)
    {
        filePath ??= Interface.CerFilePath;
        lock (lockGenerateCertificate)
        {
            return GenerateCertificateUnlock(filePath);
        }
    }

    async ValueTask TrustRootCertificateAsync()
    {
        try
        {
            SharedTrustRootCertificate();
        }
#if DEBUG
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "TrustRootCertificate Error");
        }
#else
        catch { }
#endif

        await PlatformTrustRootCertificateGuideAsync();
    }

    /// <inheritdoc cref="ICertificateManager.PlatformTrustRootCertificateGuideAsync"/>
    public async ValueTask PlatformTrustRootCertificateGuideAsync()
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                await TrustRootCertificateMacOSAsync();
            }
            else if (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid())
            {
                TrustRootCertificateLinux();
            }
        }
#if DEBUG
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "PlatformTrustRootCertificateGuide Error");
        }
#else
        catch { }
#endif
    }

    [SupportedOSPlatform("macOS")]
    async ValueTask TrustRootCertificateMacOSAsync()
    {
        var filePath = GetCerFilePathGeneratedWhenNoFileExists();
        if (filePath == null) return;
        var state = await platformService.TrustRootCertificateAsync(filePath);
        //await platformService.RunShellAsync($"security add-trusted-cert -d -r trustRoot -k /Users/{Environment.UserName}/Library/Keychains/login.keychain-db \\\"{filePath}\\\"", true);
        if (state != null && !IsRootCertificateInstalled)
            await TrustRootCertificateMacOSAsync();
    }

    [SupportedOSPlatform("Linux")]
    void TrustRootCertificateLinux()
    {
        var filePath = GetCerFilePathGeneratedWhenNoFileExists();
        if (filePath == null) return;
        //全部屏蔽 Linux 浏览器全部不信任系统证书 只能手动导入 如需导入请手动操作
        //var crtFile = $"{Path.Combine(IOPath.AppDataDirectory, $@"{ICertificateManager.CertificateName}.Certificate.crt")}";
        ////复制一份Crt导入系统用 ca-certificates 只识别Crt后缀 
        //platformService.RunShell($"cp -f \"{filePath}\" \"{crtFile}\"", false);
        //platformService.RunShell($"cp -f \"{crtFile}\" \"/usr/local/share/ca-certificates\" && sudo update-ca-certificates", true);
        //浏览器不信任系统证书列表
        Browser2.Open(Constants.Urls.OfficialWebsite_LiunxSetupCer);
    }

    /// <inheritdoc cref="ICertificateManager.SetupRootCertificate"/>
    public async ValueTask<bool> SetupRootCertificateAsync()
    {
        if (!GenerateCertificate()) return false;
        if (!IsRootCertificateInstalled)
        {
            await TrustRootCertificateAsync();
            return IsRootCertificateInstalled;
        }
        return true;
    }

    /// <inheritdoc cref="ICertificateManager.DeleteRootCertificate"/>
    public bool DeleteRootCertificate()
    {
        //if (reverseProxyService.ProxyRunning)
        //    return false;
        if (RootCertificate == null)
            return true;
        try
        {
            if (OperatingSystem2.IsMacOS())
            {
                DeleteRootCertificateMacOS();
            }
            else if (OperatingSystem2.IsLinux())
            {
                DeleteRootCertificateLinux();
            }
            else
            {
                SharedRemoveTrustedRootCertificate();
            }
            if (!IsRootCertificateInstalled)
            {
                RootCertificate = null;
                var pfxFilePath = Interface.PfxFilePath;
                if (File.Exists(pfxFilePath)) File.Delete(pfxFilePath);
            }
        }
        catch (CryptographicException)
        {
            // 取消删除证书
            return false;
        }
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "DeleteRootCertificate Error");
            return false;
        }
        return true;
    }

    [SupportedOSPlatform("Linux")]
    void DeleteRootCertificateLinux()
    {
        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);
        var collection = store.Certificates.Find(X509FindType.FindByIssuerName, ICertificateManager.RootCertificateName, false);
        foreach (var item in collection)
        {
            if (item != null)
            {
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Remove(item);
                }
                catch
                {
                    platformService.RunShell($"rm -f \"/usr/local/share/ca-certificates/{ICertificateManager.CertificateName}.Certificate.pem\" && sudo update-ca-certificates", true);
                }
            }
        }
    }

    [SupportedOSPlatform("macOS")]
    async void DeleteRootCertificateMacOS()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);
        var collection = store.Certificates.Find(X509FindType.FindByIssuerName, ICertificateManager.RootCertificateName, false);
        foreach (var item in collection)
        {
            if (item != null)
            {
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Remove(item);
                }
                catch
                {
                    await platformService.RunShellAsync($"security delete-certificate -Z \\\"{item.GetCertHashString()}\\\"", true);
                }
            }
        }
    }

    /// <inheritdoc cref="ICertificateManager.IsRootCertificateInstalled"/>
    public bool IsRootCertificateInstalled
    {
        get
        {
            if (RootCertificate == null)
                if (GetCerFilePathGeneratedWhenNoFileExists() == null) return false;
            return IsCertificateInstalled(RootCertificate);
        }
    }

    /// <summary>
    /// 检查证书是否已安装并信任
    /// </summary>
    /// <param name="certificate2"></param>
    /// <returns></returns>
    bool IsCertificateInstalled(X509Certificate2? certificate2)
    {
        if (certificate2 == null)
            return false;
        if (certificate2.NotAfter <= DateTime.Now)
            return false;

        if (!OperatingSystem.IsAndroid() && OperatingSystem.IsLinux())
        {
            // Linux 目前没有实现检测
            return true;
        }
        else if (OperatingSystem.IsAndroid() || OperatingSystem.IsMacOS())
        {
            return platformService.IsCertificateInstalled(certificate2);
        }
        else
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.Contains(certificate2);
        }
    }
}
#endif