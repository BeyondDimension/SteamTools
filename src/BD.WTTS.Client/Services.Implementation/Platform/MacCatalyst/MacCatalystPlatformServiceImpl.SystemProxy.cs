#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    public Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip, int port)
    {
#if MACOS || MACCATALYST
        IPlatformService @this = this;
        var stringList = @this.GetMacOSNetworkSetup();
        var shellContent = new StringBuilder();
        var noProxyHostName = state ? $"{string.Join(" ", IPlatformService.GetNoProxyHostName)}" : "";
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
                        shellContent.AppendLine($"networksetup -setproxybypassdomains '{item}' '{noProxyHostName}'");
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