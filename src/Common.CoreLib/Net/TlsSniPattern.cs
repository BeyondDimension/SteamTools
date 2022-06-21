// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/TlsSniPattern.cs

// ReSharper disable once CheckNamespace
namespace System.Net;

/// <summary>
/// SNI 自定义值表达式
/// <list type="bullet">
/// <item>@domain 变量表示取域名值</item>
/// <item>@ipadress 变量表示取 IP</item>
/// <item>@random 变量表示取随机值</item>
/// </list>
/// </summary>
public struct TlsSniPattern
{
    /// <summary>
    /// 获取表示式值
    /// </summary>
    public string? Value { get; }

    public const string DomainValue = "@domain";
    public const string IPAddressValue = "@ipaddress";
    public const string RandomValue = "@random";

    /// <summary>
    /// 无 SNI
    /// </summary>
    public static TlsSniPattern None { get; } = new TlsSniPattern(default);

    /// <summary>
    /// 域名 SNI
    /// </summary>
    public static TlsSniPattern Domain { get; } = new TlsSniPattern(DomainValue);

    /// <summary>
    /// IP 值的 SNI
    /// </summary>
    public static TlsSniPattern IPAddress { get; } = new TlsSniPattern(IPAddressValue);

    /// <summary>
    /// 随机值的 SNI
    /// </summary>
    public static TlsSniPattern Random { get; } = new TlsSniPattern(RandomValue);

    /// <summary>
    /// SNI 自定义值表达式
    /// </summary>
    /// <param name="value">表示式值</param>
    public TlsSniPattern(string? value) => Value = value;

    /// <summary>
    /// 更新域名
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public TlsSniPattern WithDomain(string domain)
    {
        if (string.IsNullOrEmpty(Value)) return None;
        var value = Value.Replace(DomainValue, domain,
            StringComparison.OrdinalIgnoreCase);
        return new TlsSniPattern(value);
    }

    /// <summary>
    /// 更新 IP 地址
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public TlsSniPattern WithIPAddress(IPAddress address)
    {
        if (string.IsNullOrEmpty(Value)) return None;
        var value = Value.Replace(IPAddressValue, address.ToString(),
            StringComparison.OrdinalIgnoreCase);
        return new TlsSniPattern(value);
    }

    /// <summary>
    /// 更新随机数
    /// </summary>
    /// <returns></returns>
    public TlsSniPattern WithRandom()
    {
        if (string.IsNullOrEmpty(Value)) return None;
        var value = Value.Replace(RandomValue,
#if NETCOREAPP3_0_OR_GREATER
            Environment.TickCount64.ToString(),
#else
            Environment.TickCount.ToString(),
#endif
            StringComparison.OrdinalIgnoreCase);
        return new TlsSniPattern(value);
    }

    public override string ToString() => Value ?? string.Empty;
}
