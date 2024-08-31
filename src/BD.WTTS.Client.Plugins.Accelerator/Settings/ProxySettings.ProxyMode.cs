using AppResources = BD.WTTS.Client.Resources.Strings;
using EProxyMode = BD.WTTS.Enums.ProxyMode;

namespace BD.WTTS.Settings;

public static partial class ProxySettings
{
    //static EProxyMode DefaultProxyMode => ProxyModes[0];

    static IEnumerable<EProxyMode> GetProxyModes()
    {
#if WINDOWS
        yield return EProxyMode.Hosts;
#if !REMOVE_DNS_INTERCEPT
        yield return EProxyMode.DNSIntercept;
#endif
        yield return EProxyMode.PAC;
        yield return EProxyMode.System;
#elif ANDROID
            yield return EProxyMode.VPN;
            yield return EProxyMode.ProxyOnly;
#elif LINUX || MACOS || MACCATALYST
#if MACCATALYST
            if (OperatingSystem.IsMacOS())
#endif
        {
            yield return EProxyMode.Hosts;
            yield return EProxyMode.System;
        }
#else
            return Array.Empty<EProxyMode>();
#endif
    }

    public static IReadOnlyList<EProxyMode> ProxyModes => mProxyModes.Value;

    static readonly Lazy<IReadOnlyList<EProxyMode>> mProxyModes = new(() => GetProxyModes().ToArray());

    /// <inheritdoc cref="ProxyMode"/>
    public static EProxyMode ProxyModeValue
    {
        get
        {
            var value = ProxyMode?.Value ?? ProxyModes[0];
            if (ProxyModes.Contains(value)) return value;
            return ProxyModes[0];
        }
        set => ProxyMode.Value = value;
    }

    public static string ToStringByProxyMode(EProxyMode mode) => mode switch
    {
        EProxyMode.DNSIntercept => AppResources.ProxyMode_DNSIntercept,
        EProxyMode.Hosts => AppResources.ProxyMode_Hosts,
        EProxyMode.System => AppResources.ProxyMode_System,
        EProxyMode.VPN => AppResources.ProxyMode_VPN,
        EProxyMode.ProxyOnly => AppResources.ProxyMode_ProxyOnly,
        _ => string.Empty,
    };

    public static string ProxyModeValueString => ToStringByProxyMode(ProxyModeValue);
}
