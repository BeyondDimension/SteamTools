#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class MacCatalystPlatformServiceImpl : IPlatformService
{
    const string TAG = "MacCatalystPlatformS";

    public MacCatalystPlatformServiceImpl()
    {
        // 平台服务依赖关系过于复杂，在构造函数中不得注入任何服务，由函数中延时加载调用服务
    }

#if MACCATALYST || MACOS
    string[] IPlatformService.GetMacOSNetworkSetup()
    {
        using var p = new Process();
        p.StartInfo.FileName = "networksetup";
        p.StartInfo.Arguments = "-listallnetworkservices";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();
        var ret = p.StandardOutput.ReadToEnd().Replace(
            "An asterisk (*) denotes that a network service is disabled.", "");
        p.Kill();
        return ret.Split('\n');
    }
#endif
}
#endif
