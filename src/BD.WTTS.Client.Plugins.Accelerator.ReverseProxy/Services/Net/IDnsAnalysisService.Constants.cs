// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// DNS 解析服务
/// </summary>
public partial interface IDnsAnalysisService // 公共常量定义
{
    #region DNS 常量

    const string DNS_Ali_DohAddres = "https://dns.alidns.com";
    const string Dnspod_DohAddres = "https://1.12.12.12";
    const string Google_DohAddres = "https://dns.google";
    const string Cloudflare_DohAddres = "https://cloudflare-dns.com";

    protected const string PrimaryDNS_IPV6_Ali = "2400:3200::1";

    const string PrimaryDNS_Ali = "223.5.5.5";
    const string SecondaryDNS_Ali = "223.6.6.6";

    const string PrimaryDNS_Dnspod = "119.29.29.29";
    const string SecondaryDNS_Dnspod = "182.254.116.116";

    const string PrimaryDNS_114 = "114.114.114.114";
    const string SecondaryDNS_114 = "114.114.115.115";

    const string PrimaryDNS_Google = "8.8.8.8";
    const string SecondaryDNS_Google = "8.8.4.4";

    const string PrimaryDNS_Cloudflare = "1.1.1.1";
    const string SecondaryDNS_Cloudflare = "1.0.0.1";

    const string PrimaryDNS_Baidu = "180.76.76.76";

    static readonly IPAddress[] DNS_Alis = new[] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) };
    static readonly IPAddress[] DNS_Dnspods = new[] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) };
    static readonly IPAddress[] DNS_114s = new[] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) };
    static readonly IPAddress[] DNS_Googles = new[] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address };
    static readonly IPAddress[] DNS_Cloudflares = new[] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address };

    private static class NameServer
    {
        public static readonly IPEndPoint GooglePublicDns = new(IPAddress.Parse(SecondaryDNS_Google), 53);

        public static readonly IPEndPoint GooglePublicDns2 = new(IPAddress.Parse(PrimaryDNS_Google), 53);

        public static readonly IPEndPoint Cloudflare = new(IPAddress.Parse("1.1.1.1"), 53);

        public static readonly IPEndPoint Cloudflare2 = new(IPAddress.Parse("1.0.0.1"), 53);
    }

    #endregion
}