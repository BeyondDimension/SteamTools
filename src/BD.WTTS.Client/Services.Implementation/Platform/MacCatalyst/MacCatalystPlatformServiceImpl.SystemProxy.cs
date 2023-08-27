#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    const string ProxyOverride = "localhost 127.* 10.* 172.16.* 172.17.* 172.18.* 172.19.* 172.20.* 172.21.* 172.22.* 172.23.* 172.24.* 172.25.* 172.26.* 172.27.* 172.28.* 172.29.* 172.30.* 172.31.* 192.168.* *.steampp.net";

    public Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip, int port)
    {
#if MACOS || MACCATALYST
        IPlatformService @this = this;
        var stringList = @this.GetMacOSNetworkSetup();
        var shellContent = new StringBuilder();
        foreach (var item in stringList)
        {
            if (item.Trim().Length > 0)
            {
                if (state)
                {
                    if (ip != null)
                    {
                        var bindIP = ip?.ToString() == IPAddress.Any.ToString() ? IPAddress.Loopback : ip;
                        shellContent.AppendLine($"networksetup -setwebproxy '{item}' '{bindIP}' {port}");
                        shellContent.AppendLine($"networksetup -setwebproxystate '{item}' on");
                        shellContent.AppendLine($"networksetup -setsecurewebproxy '{item}' '{bindIP}' {port}");
                        shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' on");
                        shellContent.AppendLine($"networksetup -setproxybypassdomains '{item}' '{ProxyOverride}'");
                    }
                    else
                    {
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    shellContent.AppendLine($"networksetup -setwebproxystate '{item}' off");
                    shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' off");
                }
            }
        }
        @this.RunShell(shellContent.ToString(), false);
        return Task.FromResult(true);
#else
        throw new PlatformNotSupportedException();
#endif
    }
}
#endif