#if LINUX
using System.Text.RegularExpressions;

namespace BD.WTTS.Services.Implementation;

sealed class LinuxAcceleratorWebProxy : IWebProxy
{
    readonly IWebProxy fallback;
    IWebProxy? accelerator;

    public LinuxAcceleratorWebProxy(IWebProxy fallback) => this.fallback = fallback;

    IWebProxy Current => Volatile.Read(ref accelerator) ?? fallback;

    public ICredentials? Credentials
    {
        get => Current.Credentials;
        set => Current.Credentials = value;
    }

    public Uri? GetProxy(Uri destination) => Current.GetProxy(destination);

    public bool IsBypassed(Uri host) => Current.IsBypassed(host);

    public void SetAcceleratorProxy(IWebProxy? proxy) => Volatile.Write(ref accelerator, proxy);

    public static IWebProxy CreateAcceleratorProxy(IPAddress address, int port, IEnumerable<string> bypassHosts)
    {
        if (address.Equals(IPAddress.Any))
            address = IPAddress.Loopback;
        else if (address.Equals(IPAddress.IPv6Any))
            address = IPAddress.IPv6Loopback;

        var bypassList = bypassHosts.Select(host =>
            @"^https?://" + Regex.Escape(host).Replace(@"\*", ".*") + @"(?::\d+)?(?:/|$)").ToArray();
        return new WebProxy(new UriBuilder(Uri.UriSchemeHttp, address.ToString(), port).Uri, true, bypassList);
    }
}
#endif
