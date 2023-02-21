#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static (string productName, int revision, string releaseIdOrDisplayVersion) GetWinNTCurrentVersion()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        using var ndpKey = Registry.LocalMachine.OpenSubKey(subkey);
        var productName = ndpKey?.GetValue("ProductName")?.ToString() ?? string.Empty;
        var revision = ndpKey?.GetValue("UBR")?.ToString();
        if (!int.TryParse(revision, out var revisionInt32))
        {
            var path_kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                "kernel32.dll");
            if (File.Exists(path_kernel32))
            {
                revisionInt32 = FileVersionInfo.GetVersionInfo(path_kernel32).ProductPrivatePart;
            }
        }
        var releaseId = ndpKey?.GetValue("ReleaseId")?.ToString();
        if (int.TryParse(releaseId, out var releaseIdInt32) && releaseIdInt32 >= 2009)
        {
            var displayVersion = ndpKey?.GetValue("DisplayVersion")?.ToString();
            if (!string.IsNullOrWhiteSpace(displayVersion))
                releaseId = displayVersion;
        }
        return (productName, revisionInt32, releaseId ?? string.Empty);
    }

    static readonly Lazy<(string productName, int revision, string releaseIdOrDisplayVersion)> _WinNTCurrentVersion = new(GetWinNTCurrentVersion);

    public string WindowsProductName => _WinNTCurrentVersion.Value.productName;

    public int WindowsVersionRevision => _WinNTCurrentVersion.Value.revision;

    public string WindowsReleaseIdOrDisplayVersion => _WinNTCurrentVersion.Value.releaseIdOrDisplayVersion;
}
#endif
