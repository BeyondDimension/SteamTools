#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public async Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip, int port)
    {
        var appProxy = HttpClient.DefaultProxy as LinuxAcceleratorWebProxy;
        if (state)
        {
            ArgumentNullException.ThrowIfNull(ip);
            if (port is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(port));

            var acceleratorProxy = LinuxAcceleratorWebProxy.CreateAcceleratorProxy(ip, port, IPlatformService.GetNoProxyHostName);
            var proxyAddress = ip.Equals(IPAddress.Any) ? IPAddress.Loopback :
                ip.Equals(IPAddress.IPv6Any) ? IPAddress.IPv6Loopback : ip;
            var noProxyHostName = $"[{string.Join(",", IPlatformService.GetNoProxyHostName.Select(x => $"'{x}'"))}]";
            await SetGSettingsAsync("org.gnome.system.proxy.http", "host", proxyAddress.ToString());
            await SetGSettingsAsync("org.gnome.system.proxy.http", "port", port.ToString(CultureInfo.InvariantCulture));
            await SetGSettingsAsync("org.gnome.system.proxy.https", "host", proxyAddress.ToString());
            await SetGSettingsAsync("org.gnome.system.proxy.https", "port", port.ToString(CultureInfo.InvariantCulture));
            await SetGSettingsAsync("org.gnome.system.proxy", "ignore-hosts", noProxyHostName);
            await SetGSettingsAsync("org.gnome.system.proxy", "mode", "manual");
            appProxy?.SetAcceleratorProxy(acceleratorProxy);
        }
        else
        {
            await SetGSettingsAsync("org.gnome.system.proxy", "mode", "none");
            appProxy?.SetAcceleratorProxy(null);
        }
        return true;
    }

    static async Task SetGSettingsAsync(string schema, string key, string value)
    {
        var startInfo = new ProcessStartInfo("gsettings")
        {
            UseShellExecute = false,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("set");
        startInfo.ArgumentList.Add(schema);
        startInfo.ArgumentList.Add(key);
        startInfo.ArgumentList.Add(value);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start gsettings.");
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"gsettings set {schema} {key} failed ({process.ExitCode}): {error}");
    }
}
#endif