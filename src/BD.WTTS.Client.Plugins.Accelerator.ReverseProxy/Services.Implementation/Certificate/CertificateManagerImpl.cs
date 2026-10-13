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

    public X509CertificatePackable RootCertificatePackable { get; set; }

    readonly object lock_RootCertificatePackable = new();

    byte[]? ICertificateManager.RootCertificatePackable
    {
        get
        {
            lock (lock_RootCertificatePackable)
            {
                RootCertificate ??= LoadRootCertificate();
            }
            return RootCertificate == default ? default : Serializable.SMP2(RootCertificatePackable);
        }
    }

    /// <inheritdoc cref="ICertificateManager.PfxPassword"/>
    public byte[]? PfxPassword { get; set; }

    string? GetPfxPassword()
    {
        var pfxPassword_ = Interface.PfxPassword;
        var pfxPassword = pfxPassword_.Any_Nullable() ?
                 Encoding.UTF8.GetString(pfxPassword_!) :
                 null;
        return pfxPassword;
    }

    X509Certificate2? LoadRootCertificate()
    {
        try
        {
            ICertificateManager thiz = Interface;
            // NixOS 适配：证书加载源优先取打包证书源（STEAMTOOLS_BUNDLED_PFX，nix store 最新 PFX）。
            // store 中 cer/pfx 与系统 security.pki.certificateFiles 为同一构建产物（同源），
            // 直接加载 store PFX 可保证代理私钥与系统信任严格一致（rebuild 后新 store 路径自动跟随，
            // 不依赖 AppData 同步时机，也不受残留旧证书影响）。未设置该环境变量时回退原逻辑（AppData）。
            var bundledPfxFilePath = BundledCertificateHelper.GetBundledPfxFilePath();
            string pfxFilePath;
            if (bundledPfxFilePath != null)
            {
                pfxFilePath = bundledPfxFilePath;
            }
            else
            {
                BundledCertificateHelper.TrySyncBundledCertificate(thiz.PfxFilePath);
                pfxFilePath = thiz.PfxFilePath;
            }
            if (!File.Exists(pfxFilePath))
                return null;
            X509Certificate2 rootCert;
            try
            {
                RootCertificatePackable = X509CertificatePackable.CreateX509Certificate2(
                    pfxFilePath, GetPfxPassword(), X509KeyStorageFlags.Exportable);
                rootCert = RootCertificatePackable!;
                rootCert.ThrowIsNull();
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

    [Obsolete("use ICertificateManager.Constants.TrustRootCertificate")]
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
        RootCertificate = LoadRootCertificate();

        if (RootCertificate != null)
        {
            return true;
        }

        var validFrom = DateTime.Today.AddDays(-1);
        var validTo = DateTime.Today.AddDays(CertificateConstants.CertificateValidDays);

        //var rootCertificateName = CertificateConstants.RootCertificateName;

        // NixOS 适配：生成/续期证书必须写入可写位置（AppData），不能写 nix store（只读）。
        // 加载源（PfxFilePath）仍优先取打包证书源（store 最新，与系统信任同源）；
        // 仅当 store 源缺失/过期时兜底生成，此时写入 AppData。
        var generatedPfxFilePath = CertificateConstants.DefaultPfxFilePath;
        RootCertificate = CertGenerator.GenerateBySelfPfx(
            null,
            validFrom,
            validTo,
            generatedPfxFilePath,
            GetPfxPassword());
        RootCertificatePackable = X509CertificatePackable.CreateX509Certificate2(generatedPfxFilePath, GetPfxPassword(), X509KeyStorageFlags.Exportable);

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
            foreach (var item in x509Store.Certificates.Find(X509FindType.FindBySubjectName, CertificateConstants.RootCertificateName, false))
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
                if (!GenerateCertificateUnlock(filePath))
                    return null;
            }
            else if (RootCertificate == null)
            {
                RootCertificate = LoadRootCertificate();
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
            toast.Show(
                IPCToastService.ToastIcon.Error,
                IPCToastService.ToastText.CreateCertificateFaild);
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

    bool? ICertificateManager.GenerateCertificate()
    {
        return GenerateCertificate(null);
    }

    //    public void TrustRootCertificate()
    //    {
    //        try
    //        {
    //            if (OperatingSystem.IsWindows())
    //            {
    //                try
    //                {
    //                    SharedTrustRootCertificate();
    //                }
    //#if DEBUG
    //                catch (Exception e)
    //                {
    //                    Log.Error(TAG, e, "SharedTrustRootCertificate Error");
    //                }
    //#else
    //        catch { }
    //#endif
    //            }
    //            else if (OperatingSystem.IsMacOS())
    //            {
    //                TrustRootCertificateMacOS();
    //            }
    //            else if (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid())
    //            {
    //                TrustRootCertificateLinux();
    //            }
    //        }
    //#if DEBUG
    //        catch (Exception e)
    //        {
    //            e.LogAndShowT(TAG, msg: "PlatformTrustRootCertificateGuide Error");
    //        }
    //#else
    //        catch { }
    //#endif
    //    }

    public void TrustRootCertificate()
    {
        if (RootCertificate == null)
        {
            GenerateCertificate();
        }

        if (RootCertificate == null)
        {
            throw new ApplicationException(
                "Could not install certificate as it is null or empty.");
        }

        ICertificateManager.Constants.TrustRootCertificate(GetCerFilePathGeneratedWhenNoFileExists, platformService, RootCertificate);
    }

    //[Obsolete("use ICertificateManager.Constants.TrustRootCertificate")]
    //[SupportedOSPlatform("macOS")]
    //void TrustRootCertificateMacOS()
    //{
    //    var filePath = GetCerFilePathGeneratedWhenNoFileExists();
    //    if (filePath == null) return;
    //    var state = platformService.TrustRootCertificateAsync(filePath);
    //    //await platformService.RunShellAsync($"security add-trusted-cert -d -r trustRoot -k /Users/{Environment.UserName}/Library/Keychains/login.keychain-db \\\"{filePath}\\\"", true);
    //    if (state.HasValue && !state.Value)
    //        TrustRootCertificateMacOS();
    //}

    //[Obsolete("use ICertificateManager.Constants.TrustRootCertificate")]
    //[SupportedOSPlatform("Linux")]
    //void TrustRootCertificateLinux()
    //{
    //    var filePath = GetCerFilePathGeneratedWhenNoFileExists();
    //    if (filePath == null) return;
    //    var state = platformService.TrustRootCertificateAsync(filePath);
    //    //部分系统还是只能手动导入浏览器
    //    Browser2.Open(Constants.Urls.OfficialWebsite_LiunxSetupCer);
    //    if (state.HasValue && !state.Value)
    //        GetCerFilePathGeneratedWhenNoFileExists();
    //    //全部屏蔽 Linux 浏览器全部不信任系统证书 只能手动导入 如需导入请手动操作
    //    //var crtFile = $"{Path.Combine(IOPath.AppDataDirectory, $@"{ICertificateManager.CertificateName}.Certificate.crt")}";
    //    ////复制一份Crt导入系统用 ca-certificates 只识别Crt后缀 
    //    //platformService.RunShell($"cp -f \"{filePath}\" \"{crtFile}\"", false);
    //    //platformService.RunShell($"cp -f \"{crtFile}\" \"/usr/local/share/ca-certificates\" && sudo update-ca-certificates", true);
    //    //浏览器不信任系统证书列表
    //    //Browser2.Open(Constants.Urls.OfficialWebsite_LiunxSetupCer);
    //}

    /// <inheritdoc cref="ICertificateManager.SetupRootCertificate"/>
    public bool SetupRootCertificate()
    {
        if (!GenerateCertificate())
            return false;
        var isRootCertificateInstalled = IsRootCertificateInstalled;
        if (!isRootCertificateInstalled)
        {
            TrustRootCertificate();
            isRootCertificateInstalled = IsRootCertificateInstalled;
            return isRootCertificateInstalled;
        }
        return true;
    }

    /// <inheritdoc cref="ICertificateManager.DeleteRootCertificate"/>
    public bool DeleteRootCertificate()
    {
        //if (reverseProxyService.ProxyRunning)
        //    return false;
        if (RootCertificate == null)
        {
            return true;
        }
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var cer = Serializable.SMP2(RootCertificatePackable);
                platformService.RemoveCertificate(cer);
            }
            else if (OperatingSystem.IsLinux())
            {
                DeleteRootCertificateLinux();
            }
            else
            {
                SharedRemoveTrustedRootCertificate();
            }
            var isRootCertificateInstalled = IsRootCertificateInstalled;
            if (!isRootCertificateInstalled)
            {
                RootCertificate = default;
                RootCertificatePackable = default;
                var pfxFilePath = Interface.PfxFilePath;
                var cerFilePath = Interface.CerFilePath;
                IOPath.FileTryDelete(pfxFilePath);
                IOPath.FileTryDelete(cerFilePath);
                return true;
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
        return false;
    }

    [SupportedOSPlatform("Linux")]
    void DeleteRootCertificateLinux()
    {

        var cer = Serializable.SMP2(RootCertificatePackable);
        platformService.RemoveCertificate(cer);
    }

    //[SupportedOSPlatform("macOS")]
    //async void DeleteRootCertificateMacOS()
    //{
    //    //using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
    //    //store.Open(OpenFlags.ReadOnly);
    //    //var collection = store.Certificates.Find(X509FindType.FindByIssuerName, CertificateConstants.RootCertificateName, false);
    //    //foreach (var item in collection)
    //    //{
    //    //    if (item != null)
    //    //    {
    //    //        try
    //    //        {
    //    //            store.Open(OpenFlags.ReadWrite);
    //    //            store.Remove(item);
    //    //        }
    //    //        catch
    //    //        {
    //    //            await platformService.RunShellAsync($"security delete-certificate -Z \\\"{item.GetCertHashString()}\\\"", true);
    //    //        }
    //    //    }
    //    //}
    //}

    /// <inheritdoc cref="ICertificateManager.IsRootCertificateInstalled"/>
    public bool IsRootCertificateInstalled
    {
        get
        {
            var result = ICertificateManager.Constants.IsRootCertificateInstalled(this, platformService, RootCertificatePackable);
            return result;
        }
    }

    bool? ICertificateManager.IsRootCertificateInstalled2 => IsRootCertificateInstalled;

    /// <summary>
    /// 检查证书是否已安装并信任
    /// </summary>
    /// <param name="certificate2"></param>
    /// <returns></returns>
    bool IsCertificateInstalled(X509CertificatePackable packable)
    {
        var result = ICertificateManager.Constants.IsCertificateInstalled(platformService, packable);
        return result;
    }

    public string GetCertificateInfo()
    {
        var rootCert = RootCertificate;
        if (rootCert == null)
        {
            GenerateCertificate();
        }
        rootCert = RootCertificate;

        if (rootCert == null)
        {
            return "";
        }

        StringBuilder b = new();
        b.AppendLine("Subject：");
        b.AppendLine(rootCert.Subject);
        b.AppendLine("SerialNumber：");
        b.AppendLine(rootCert.SerialNumber);
        b.AppendLine("PeriodValidity：");
        b.Append(rootCert.GetEffectiveDateString());
        b.Append(" ~ ");
        b.Append(rootCert.GetExpirationDateString());
        b.AppendLine();
        b.AppendLine("SHA256：");
        b.AppendLine(rootCert.GetCertHashStringCompat(HashAlgorithmName.SHA256));
        b.AppendLine("SHA1：");
        b.AppendLine(rootCert.GetCertHashStringCompat(HashAlgorithmName.SHA1));

        return b.ToString();
    }
}