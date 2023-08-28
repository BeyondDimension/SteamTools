#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    internal static bool SetAsSystemProxyStatus;

    public async Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip, int port)
    {
        try
        {
            SetAsSystemProxyStatus = state;
            var proxyEnable = $"\"ProxyEnable\"=dword:{(state ? "00000001" : "00000000")}";
            var hasIpAndProt = ip != null && port >= 0;
            var proxyServer = hasIpAndProt ? $"\"proxyServer\"=\"{ip}:{port}\"" : "";
            var noProxyHostName = state ? $"\"ProxyOverride\"=\"{string.Join(";", IPlatformService.GetNoProxyHostName)}\"" : "";
            string contents =
$"""
Windows Registry Editor Version 5.00
; {AssemblyInfo.Trademark} BD.WTTS.Services.Implementation.WindowsPlatformServiceImpl.SetAsSystemProxy
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings]
{proxyEnable}
{(state ? $"{noProxyHostName}{Environment.NewLine}" : "")}{proxyServer}
""";
            var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchProxy", FileEx.Reg);
            var r = await StartProcessRegeditAsync(path, contents, markKey: nameof(SetAsSystemProxyAsync), markValue: state.ToString());
            return r == 200;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, TAG);
            return false;
        }
    }

    internal static bool SetAsSystemPACProxyStatus;

    public async Task<bool> SetAsSystemPACProxyAsync(bool state, string? url = null)
    {
        try
        {
            SetAsSystemPACProxyStatus = state;
            var regAutoConfigUrl = state ? $"\"AutoConfigURL\"=\"{url}\"" : "\"AutoConfigURL\"=\"\"";
            string contents =
$"""
Windows Registry Editor Version 5.00
; {AssemblyInfo.Trademark} BD.WTTS.Services.Implementation.WindowsPlatformServiceImpl.SetAsSystemPACProxy
[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings]
{regAutoConfigUrl}
""";
            var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchPacProxy", FileEx.Reg);
            var r = await StartProcessRegeditAsync(path, contents, markKey: nameof(SetAsSystemPACProxyAsync), markValue: state.ToString());
            return r == 200;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, TAG);
            return false;
        }
    }
}
#endif