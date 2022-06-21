using System.Application.Models;
using System.Application.UI.Resx;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Titanium.Web.Proxy.Network;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

abstract class ReverseProxyServiceImpl
{
    protected readonly IPlatformService platformService;

    public ReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalysis)
    {
        this.platformService = platformService;
        DnsAnalysis = dnsAnalysis;
    }

    public IDnsAnalysisService DnsAnalysis { get; }

    public abstract CertificateManager CertificateManager { get; }

    protected virtual void InitCertificateManager()
    {
        // 可选地设置证书引擎
        CertificateManager.CertificateEngine = (CertificateEngine)CertificateEngine;
        //CertificateManager.PfxPassword = $"{CertificateName}";
        CertificateManager.PfxFilePath = ((IReverseProxyService)this).PfxFilePath;
        CertificateManager.RootCertificateIssuerName = RootCertificateIssuerName;
        CertificateManager.RootCertificateName = RootCertificateName;
        //mac和ios的证书信任时间不能超过300天
        CertificateManager.CertificateValidDays = 300;
        //CertificateManager.SaveFakeCertificates = true;

        CertificateManager.RootCertificate = CertificateManager.LoadRootCertificate();
    }

    public X509Certificate2? RootCertificate
    {
        get => CertificateManager.RootCertificate;
        set => CertificateManager.RootCertificate = value;
    }

    public abstract bool ProxyRunning { get; }

    public bool IsCertificate => RootCertificate == null;

    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    public IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    public bool IsEnableScript { get; set; }

    public bool IsOnlyWorkSteamBrowser { get; set; }

    public ECertificateEngine CertificateEngine { get; set; } = ECertificateEngine.BouncyCastle;

    public int ProxyPort { get; set; } = 26501;

    public IPAddress ProxyIp { get; set; } = IPAddress.Any;

    public bool IsSystemProxy { get; set; }

    public bool IsProxyGOG { get; set; }

    public bool OnlyEnableProxyScript { get; set; }

    public bool Socks5ProxyEnable { get; set; }

    public bool EnableHttpProxyToHttps { get; set; }

    public int Socks5ProxyPortId { get; set; }

    public bool TwoLevelAgentEnable { get; set; }

    public EExternalProxyType TwoLevelAgentProxyType { get; set; } = DefaultTwoLevelAgentProxyType;

    public string? TwoLevelAgentIp { get; set; }

    public int TwoLevelAgentPortId { get; set; }

    public string? TwoLevelAgentUserName { get; set; }

    public string? TwoLevelAgentPassword { get; set; }

    public IPAddress? ProxyDNS { get; set; }

    public int GetRandomUnusedPort() => SocketHelper.GetRandomUnusedPort(ProxyIp);

    public bool PortInUse(int port) => SocketHelper.IsUsePort(ProxyIp, port);

    static bool WirtePemCertificateToGoGSteamPlugins(Func<string> getPemCertificateString)
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gogPlugins = Path.Combine(local, "GOG.com", "Galaxy", "plugins", "installed");
        if (Directory.Exists(gogPlugins))
        {
            foreach (var dir in Directory.GetDirectories(gogPlugins))
            {
                if (dir.Contains("steam"))
                {
                    var pem = getPemCertificateString();
                    var certifi = Path.Combine(local, dir, "certifi", "cacert.pem");
                    if (File.Exists(certifi))
                    {
                        var file = File.ReadAllText(certifi);
                        var s = file.Substring(Constants.CERTIFICATE_TAG, Constants.CERTIFICATE_TAG, true);
                        if (string.IsNullOrEmpty(s))
                        {
                            File.AppendAllText(certifi, Environment.NewLine + pem);
                        }
                        else if (s.Trim() != pem.Trim())
                        {
                            var index = file.IndexOf(Constants.CERTIFICATE_TAG);
                            File.WriteAllText(certifi, file.Remove(index, s.Length) + pem);
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool WirtePemCertificateToGoGSteamPlugins()
        => WirtePemCertificateToGoGSteamPlugins(() =>
            RootCertificate!.GetPublicPemCertificateString());

    protected virtual void OnException(Exception exception)
    {
        Log.Error(TAG, exception, "ProxyServer ExceptionFunc");
    }

    static readonly object lockGenerateCertificate = new();

    public string? GetCerFilePathGeneratedWhenNoFileExists()
    {
        var filePath = ((IReverseProxyService)this).CerFilePath;
        lock (lockGenerateCertificate)
        {
            if (!File.Exists(filePath))
            {
                if (!GenerateCertificateUnlock(filePath)) return null;
            }
            return filePath;
        }
    }

    protected bool GenerateCertificateUnlock(string filePath)
    {
        var result = CertificateManager.CreateRootCertificate(true);
        if (!result || RootCertificate == null)
        {
            Log.Error(TAG, AppResources.CreateCertificateFaild);
            Toast.Show(AppResources.CreateCertificateFaild);
            return false;
        }

        RootCertificate.SaveCerCertificateFile(filePath);

        return true;
    }

    public bool GenerateCertificate(string? filePath = null)
    {
        filePath ??= ((IReverseProxyService)this).CerFilePath;
        lock (lockGenerateCertificate)
        {
            return GenerateCertificateUnlock(filePath);
        }
    }

    public void TrustCer()
    {
        var filePath = GetCerFilePathGeneratedWhenNoFileExists();
        if (filePath != null)
            IPlatformService.Instance.RunShell($"security add-trusted-cert -d -r trustRoot -k /Users/{Environment.UserName}/Library/Keychains/login.keychain-db \\\"{filePath}\\\"", true);
    }

    public bool SetupCertificate()
    {
        // 此代理使用的本地信任根证书
        //proxyServer.CertificateManager.TrustRootCertificate(true);
        //proxyServer.CertificateManager
        //    .CreateServerCertificate($"{Assembly.GetCallingAssembly().GetName().Name} Certificate")
        //    .ContinueWith(c => proxyServer.CertificateManager.RootCertificate = c.Result);

        if (!GenerateCertificate()) return false;

        try
        {
            CertificateManager.TrustRootCertificate();
        }
#if DEBUG
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "TrustRootCertificate Error");
        }
#else
            catch { }
#endif
        try
        {
            CertificateManager.EnsureRootCertificate();
        }

#if DEBUG
        catch (Exception e)
        {
            e.LogAndShowT(TAG, msg: "EnsureRootCertificate Error");
        }
#else
            catch { }
#endif
        if (OperatingSystem2.IsMacOS())
        {
            TrustCer();
        }
        if (OperatingSystem2.IsLinux() && !OperatingSystem2.IsAndroid())
        {
            //IPlatformService.Instance.AdminShell($"sudo cp -f \"{filePath}\" \"{Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.pem")}\"", false);
            Browser2.Open(UrlConstants.OfficialWebsite_LiunxSetupCer);
            return true;
        }
        return IsCertificateInstalled(CertificateManager.RootCertificate);
    }

    /// <summary>
    /// 删除全部 Watt Toolkit 证书 如失败尝试 命令删除
    /// </summary>
    public async void DeleteCer()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByIssuerName, RootCertificateName, false);
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
                    await IPlatformService.Instance.RunShellAsync($"security delete-certificate -Z \\\"{item.GetCertHashString()}\\\"", true);
                }
            }
        }

    }

    public bool DeleteCertificate()
    {
        if (ProxyRunning)
            return false;
        if (CertificateManager.RootCertificate == null)
            return true;
        try
        {
            //using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            //{
            //    store.Open(OpenFlags.MaxAllowed);
            //    var test = store.Certificates.Find(X509FindType.FindByIssuerName, CertificateName, true);
            //    foreach (var item in test)
            //    {
            //        store.Remove(item);
            //    }
            //}
            //proxyServer.CertificateManager.ClearRootCertificate();

            if (OperatingSystem2.IsMacOS())
            {
                DeleteCer();
            }
            else
            {
                CertificateManager.RemoveTrustedRootCertificate();
            }
            if (IsCertificateInstalled(CertificateManager.RootCertificate) == false)
            {
                CertificateManager.RootCertificate = null;
                if (File.Exists(CertificateManager.PfxFilePath))
                    File.Delete(CertificateManager.PfxFilePath);
            }
            //CertificateManager.RemoveTrustedRootCertificateAsAdmin();
            //CertificateManager.CertificateStorage.Clear();
        }
        catch (CryptographicException)
        {
            //取消删除证书
        }
        catch (Exception)
        {
            throw;
        }
        return true;
    }

    public bool IsCurrentCertificateInstalled
    {
        get
        {
            if (CertificateManager.RootCertificate == null)
                if (GetCerFilePathGeneratedWhenNoFileExists() == null) return false;
            return IsCertificateInstalled(CertificateManager.RootCertificate, usePlatformCheck: true);
        }
    }

    public bool IsCertificateInstalled(X509Certificate2? certificate2) => IsCertificateInstalled(certificate2, false);

    public bool IsCertificateInstalled(X509Certificate2? certificate2, bool usePlatformCheck)
    {
        if (certificate2 == null)
            return false;
        if (certificate2.NotAfter <= DateTime.Now)
            return false;

        if (!OperatingSystem2.IsAndroid() && OperatingSystem2.IsLinux())
        {
            return true;
        }

        bool result;
        //|| OperatingSystem2.IsMacOS() 
        if ((usePlatformCheck && OperatingSystem2.IsAndroid()) || OperatingSystem2.IsMacOS())
        {
            result = platformService.IsCertificateInstalled(certificate2);
        }
        else
        {
            using var store = new X509Store(OperatingSystem2.IsMacOS() ? StoreName.My : StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            result = store.Certificates.Contains(certificate2);
        }
        return result;
    }

    public abstract EReverseProxyEngine ReverseProxyEngine { get; }

    #region IDisposable

    protected abstract void DisposeCore();

    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                DisposeCore();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
