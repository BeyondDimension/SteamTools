// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class LazyCertificateManager : ICertificateManager
{
    private LazyCertificateManager() { }

    public static ICertificateManager Instance = new LazyCertificateManager();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE1006 // 命名样式
    static ICertificateManager impl() => Ioc.Get<ICertificateManager>();
#pragma warning restore IDE1006 // 命名样式

    public byte[]? PfxPassword => impl().PfxPassword;

    public X509CertificatePackable RootCertificatePackable { get => impl().RootCertificatePackable; set => impl().RootCertificatePackable = value; }

    public bool IsRootCertificateInstalled => impl().IsRootCertificateInstalled;

    public string? GetCerFilePathGeneratedWhenNoFileExists()
    {
        return impl().GetCerFilePathGeneratedWhenNoFileExists();
    }

    public ValueTask PlatformTrustRootCertificateGuideAsync()
    {
        return impl().PlatformTrustRootCertificateGuideAsync();
    }

    public ValueTask<bool> SetupRootCertificateAsync()
    {
        return impl().SetupRootCertificateAsync();
    }

    public bool DeleteRootCertificate()
    {
        return impl().DeleteRootCertificate();
    }
}
