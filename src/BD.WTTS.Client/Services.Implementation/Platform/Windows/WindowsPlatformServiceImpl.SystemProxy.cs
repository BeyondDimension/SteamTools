#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    const string ProxyOverride = "\"ProxyOverride\"=\"localhost;127.*;10.*;172.16.*;172.17.*;172.18.*;172.19.*;172.20.*;172.21.*;172.22.*;172.23.*;172.24.*;172.25.*;172.26.*;172.27.*;172.28.*;172.29.*;172.30.*;172.31.*;192.168.*\"";

    /// <summary>
    /// 设置启用或关闭系统代理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool SetAsSystemProxy(bool state, IPAddress? ip, int port)
    {
        try
        {
            var proxyEnable = $"\"ProxyEnable\"=dword:{(state ? "00000001" : "00000000")}";
            var hasIpAndProt = ip != null && port >= 0;
            var proxyServer = hasIpAndProt ? $"\"proxyServer\"=\"{ip}:{port}\"" : "";
            var reg = $"Windows Registry Editor Version 5.00{Environment.NewLine}[HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Internet Settings]{Environment.NewLine}{proxyEnable}{Environment.NewLine}{(state ? $"{ProxyOverride}{Environment.NewLine}" : "")}{proxyServer}";
            var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchProxy", FileEx.Reg);
            File.WriteAllText(path, reg, Encoding.UTF8);
            var p = StartProcessRegedit($"/s \"{path}\"");
            IOPath.TryDeleteInDelay(p, path);
            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, TAG);
            return false;
        }
    }

    /// <summary>
    /// 设置启用或关闭 PAC 代理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public bool SetAsSystemPACProxy(bool state, string? url = null)
    {
        try
        {
            var regAutoConfigUrl = state ? $"\"AutoConfigURL\"=\"{url}\"" : "\"AutoConfigURL\"=\"\"";
            var reg = $"Windows Registry Editor Version 5.00{Environment.NewLine}[HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Internet Settings]{Environment.NewLine}{regAutoConfigUrl}";
            var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchPacProxy", FileEx.Reg);
            File.WriteAllText(path, reg, Encoding.UTF8);
            var p = StartProcessRegedit($"/s \"{path}\"");
            IOPath.TryDeleteInDelay(p, path);
            return true;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex, TAG);
            return false;
        }
    }
}
#endif