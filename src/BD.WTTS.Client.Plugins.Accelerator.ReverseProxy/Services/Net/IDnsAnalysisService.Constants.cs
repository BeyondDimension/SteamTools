// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// DNS 解析服务
/// </summary>
public partial interface IDnsAnalysisService // 公共常量定义
{
    #region DNS 常量

    const string DNS_Ali_DohAddres = "https://dns.alidns.com/resolve";
    const string DNS_Ali_DohAddres2 = "https://223.6.6.6/resolve";
    const string DNS_Ali_DohAddres3 = "https://223.5.5.5/resolve";

    const string Dnspod_DohAddres = "https://1.12.12.12/resolve";
    const string Dnspod_DohAddres2 = "https://doh.pub/resolve";
    const string Dnspod_DohAddres3 = "https://120.53.53.53/resolve";

    const string Google_DohAddres = "https://dns.google/resolve";
    const string Cloudflare_DohAddres = "https://cloudflare-dns.com/resolve";
    const string DohAddres_360 = "https://doh.360.cn/resolve";
    const string TUNA_DohAddres = "https://101.6.6.6:8443/resolve";

    const string PrimaryDNS_IPV6_Ali = "2400:3200::1";

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

    static readonly Uri[] DoH_Alis = new[] { new Uri(DNS_Ali_DohAddres), new Uri(DNS_Ali_DohAddres2), new Uri(DNS_Ali_DohAddres3) };
    static readonly Uri[] DoH_Dnspods = new[] { new Uri(Dnspod_DohAddres), new Uri(Dnspod_DohAddres2), new Uri(Dnspod_DohAddres3) };

    private static class NameServer
    {
        public static readonly IPEndPoint GooglePublicDns = new(IPAddress.Parse(SecondaryDNS_Google), 53);

        public static readonly IPEndPoint GooglePublicDns2 = new(IPAddress.Parse(PrimaryDNS_Google), 53);

        public static readonly IPEndPoint Cloudflare = new(IPAddress.Parse("1.1.1.1"), 53);

        public static readonly IPEndPoint Cloudflare2 = new(IPAddress.Parse("1.0.0.1"), 53);
    }

    #endregion DNS 常量
}