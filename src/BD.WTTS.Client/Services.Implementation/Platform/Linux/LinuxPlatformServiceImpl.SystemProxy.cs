#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    const string ProxyOverride = "['localhost', '127.*', '10.*', '172.16.*', '172.17.*', '172.18.*', '172.19.*', '172.20.*', '172.21.*', '172.22.*', '172.23.*', '172.24.*', '172.25.*', '172.26.*', '172.27.*', '172.28.*', '172.29.*', '172.30.*', '172.31.*', '192.168.*', '*.steampp.net']";

    public Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip, int port)
    {
        var shellContent = new StringBuilder();
        if (state)
        {
            var hasIpAndProt = ip != null && port >= 0;
            shellContent.AppendLine("gsettings set org.gnome.system.proxy mode 'manual'");
            if (hasIpAndProt)
            {
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http host '{ip}'");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.http port {port}");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https host '{ip}'");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy.https port {port}");
                shellContent.AppendLine($"gsettings set org.gnome.system.proxy ignore-hosts \"{ProxyOverride}\"");
            }
        }
        else
        {
            shellContent.AppendLine($"gsettings set org.gnome.system.proxy mode 'none'");
        }
        IPlatformService @this = this;
        @this.RunShell(shellContent.ToString(), false);
        return Task.FromResult(true);
    }
}
#endif