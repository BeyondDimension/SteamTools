// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

public sealed partial class ProxySettings : SettingsHost2<ProxySettings>
{
    /// <summary>
    /// 启用脚本自动检查更新
    /// </summary>
    public static SerializableProperty<bool> IsAutoCheckScriptUpdate { get; }
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 启用代理脚本
    /// </summary>
    public static SerializableProperty<bool> IsEnableScript { get; }
        = GetProperty(defaultValue: false);

    /// <summary>
    /// 代理服务启用状态
    /// </summary>
    public static SerializableProperty<IReadOnlyCollection<string>> SupportProxyServicesStatus { get; }
        = GetProperty(defaultValue: (IReadOnlyCollection<string>)Array.Empty<string>(), autoSave: false);

    /// <summary>
    /// 脚本启用状态
    /// </summary>
    public static SerializableProperty<IReadOnlyCollection<int>> ScriptsStatus { get; }
        = GetProperty(defaultValue: (IReadOnlyCollection<int>)Array.Empty<int>());

    #region 代理设置

    /// <summary>
    /// 程序启动时自动启动代理
    /// </summary>
    public static SerializableProperty<bool> ProgramStartupRunProxy { get; }
        = GetProperty(defaultValue: false);

    /// <summary>
    /// 系统代理模式端口
    /// </summary>
    public static SerializableProperty<int> SystemProxyPortId { get; }
        = GetProperty(defaultValue: 26501, autoSave: false);

    /// <summary>
    /// 系统代理模式IP
    /// </summary>
    public static SerializableProperty<string> SystemProxyIp { get; }
        = GetProperty(defaultValue: IPAddress.Any.ToString(), autoSave: false);

    /// <summary>
    /// 开启加速后仅代理脚本而不加速
    /// </summary>
    public static SerializableProperty<bool> OnlyEnableProxyScript { get; }
        = GetProperty(defaultValue: false, autoSave: false);

    /// <summary>
    /// 代理时使用的解析主DNS
    /// </summary>
    public static SerializableProperty<string?> ProxyMasterDns { get; }
        = GetProperty<string?>(defaultValue: "223.5.5.5", autoSave: false);

    /// <summary>
    /// 启用 Http 链接转发到 Https
    /// </summary>
    public static SerializableProperty<bool> EnableHttpProxyToHttps { get; }
        = GetProperty(defaultValue: true);

    #endregion

    #region 本地代理设置

    /// <summary>
    /// Socks5 Enable
    /// </summary>
    public static SerializableProperty<bool> Socks5ProxyEnable { get; }
        = GetProperty(defaultValue: false, autoSave: false);

    /// <summary>
    /// Socks5 监听端口
    /// </summary>
    public static SerializableProperty<int> Socks5ProxyPortId { get; }
        = GetProperty(defaultValue: DefaultSocks5ProxyPortId, autoSave: false);

    public const int DefaultSocks5ProxyPortId = 8868;

    #endregion

    #region 二级代理设置

    /// <summary>
    /// TwoLevelAgent Enable
    /// </summary>
    public static SerializableProperty<bool> TwoLevelAgentEnable { get; }
        = GetProperty(defaultValue: false, autoSave: false);

    /// <summary>
    /// TwoLevelAgent ProxyType
    /// </summary>
    public static SerializableProperty<short> TwoLevelAgentProxyType { get; }
        = GetProperty(defaultValue: DefaultTwoLevelAgentProxyType, autoSave: false);

    public const short DefaultTwoLevelAgentProxyType =
        (short)IReverseProxyService.DefaultTwoLevelAgentProxyType;

    /// <summary>
    /// 二级代理 IP
    /// </summary>
    public static SerializableProperty<string> TwoLevelAgentIp { get; }
        = GetProperty(defaultValue: IPAddress.Loopback.ToString(), autoSave: false);

    /// <summary>
    /// 二级代理 监听端口
    /// </summary>
    public static SerializableProperty<int> TwoLevelAgentPortId { get; }
        = GetProperty(defaultValue: DefaultTwoLevelAgentPortId, autoSave: false);

    public const int DefaultTwoLevelAgentPortId = 7890;

    /// <summary>
    /// TwoLevelAgent UserName
    /// </summary>
    public static SerializableProperty<string> TwoLevelAgentUserName { get; }
        = GetProperty(defaultValue: string.Empty, autoSave: false);

    /// <summary>
    /// TwoLevelAgent Password
    /// </summary>
    public static SerializableProperty<string> TwoLevelAgentPassword { get; }
        = GetProperty(defaultValue: string.Empty, autoSave: false);

    #endregion

    #region 代理模式设置

    static ProxyMode DefaultProxyMode => ProxyModes.FirstOrDefault();

    static IEnumerable<ProxyMode> GetProxyModes()
    {
        if (OperatingSystem2.IsWindows())
        {
            yield return ProxyMode.Hosts;
            yield return ProxyMode.DNSIntercept;
            yield return ProxyMode.PAC;
            yield return ProxyMode.System;
        }
        else if (OperatingSystem2.IsAndroid())
        {
            yield return ProxyMode.VPN;
            yield return ProxyMode.ProxyOnly;
        }
        else if (OperatingSystem2.IsLinux() || OperatingSystem2.IsMacOS())
        {
            yield return ProxyMode.Hosts;
            yield return ProxyMode.System;
        }
    }

    public static IReadOnlyList<ProxyMode> ProxyModes => mProxyModes.Value;

    static readonly Lazy<IReadOnlyList<ProxyMode>> mProxyModes = new(() => GetProxyModes().ToArray());

    /// <summary>
    /// 当前代理模式
    /// </summary>
    public static SerializableProperty<ProxyMode> ProxyMode { get; }
       = GetProperty(defaultValue: DefaultProxyMode);

    /// <inheritdoc cref="ProxyMode"/>
    public static ProxyMode ProxyModeValue
    {
        get
        {
            var value = ProxyMode.Value;
            if (ProxyModes.Contains(value)) return value;
            return DefaultProxyMode;
        }
        set => ProxyMode.Value = value;
    }

    public static string ToStringByProxyMode(ProxyMode mode) => mode switch
    {
        ProxyMode.DNSIntercept => AppResources.ProxyMode_DNSIntercept,
        ProxyMode.Hosts => AppResources.ProxyMode_Hosts,
        ProxyMode.System => AppResources.ProxyMode_System,
        ProxyMode.VPN => AppResources.ProxyMode_VPN,
        ProxyMode.ProxyOnly => AppResources.ProxyMode_ProxyOnly,
        _ => string.Empty,
    };

    public static string ProxyModeValueString => ToStringByProxyMode(ProxyModeValue);

    #endregion
}