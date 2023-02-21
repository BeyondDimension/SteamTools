#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    public bool SetAsSystemProxy(bool state, IPAddress? ip, int port)
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
                    shellContent.AppendLine($"networksetup -setwebproxy '{item}' '{ip}' {port}");
                    shellContent.AppendLine($"networksetup -setwebproxystate '{item}' on");
                    shellContent.AppendLine($"networksetup -setsecurewebproxy '{item}' '{ip}' {port}");
                    shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' on");
                }
                else
                {
                    shellContent.AppendLine($"networksetup -setwebproxystate '{item}' off");
                    shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' off");
                }
            }
        }
        @this.RunShell(shellContent.ToString(), false);
        return true;
#else
        throw new PlatformNotSupportedException();
#endif
    }
}
#endif