using static BD.WTTS.Settings.Abstractions.IProxySettings;

namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class ProxySettings_ : IProxySettings
{
    public const string Name = nameof(ProxySettings_);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public bool? IsAutoCheckScriptUpdate { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public bool? IsEnableScript { get; set; }

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public IReadOnlyCollection<string>? SupportProxyServicesStatus { get; set; }

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public IReadOnlyCollection<int>? ScriptsStatus { get; set; }

    #region 代理设置
    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public bool? ProgramStartupRunProxy { get; set; }

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public int? SystemProxyPortId { get; set; }

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public string? SystemProxyIp { get; set; }

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public bool? OnlyEnableProxyScript { get; set; }

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public string? ProxyMasterDns { get; set; }

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool? EnableHttpProxyToHttps { get; set; }
    #endregion

    #region 本地代理设置
    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public bool? Socks5ProxyEnable { get; set; }

    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public int? Socks5ProxyPortId { get; set; }
    #endregion

    #region 二级代理设置
    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public bool? TwoLevelAgentEnable { get; set; }

    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public short? TwoLevelAgentProxyType { get; set; }

    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public string? TwoLevelAgentIp { get; set; }

    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public int? TwoLevelAgentPortId { get; set; }

    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public string? TwoLevelAgentUserName { get; set; }

    [MPKey(17), MP2Key(17), JsonPropertyOrder(17)]
    public string? TwoLevelAgentPassword { get; set; }
    #endregion

    #region 代理模式设置
    [MPKey(18), MP2Key(18), JsonPropertyOrder(18)]
    public ProxyMode ProxyMode { get; set; }
    #endregion

    [MPKey(19), MP2Key(19), JsonPropertyOrder(19)]
    public bool? IsProxyGOG { get; set; }

    [MPKey(20), MP2Key(20), JsonPropertyOrder(20)]
    public bool? IsOnlyWorkSteamBrowser { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(ProxySettings_))]
internal partial class ProxySettingsContext : JsonSerializerContext
{
    static ProxySettingsContext? instance;

    public static ProxySettingsContext Instance
        => instance ??= new ProxySettingsContext(ISettings.GetDefaultOptions());
}

partial class ProxySettings_ : ISettings<ProxySettings_>
{
    public static JsonTypeInfo<ProxySettings_> JsonTypeInfo => ProxySettingsContext.Instance.ProxySettings_;

    public static JsonSerializerContext JsonSerializerContext => ProxySettingsContext.Instance;

    static string ISettings.Name => Name;

    static JsonTypeInfo ISettings.JsonTypeInfo => ProxySettingsContext.Instance.ProxySettings_;
}

[SettingsGeneration]
public static class ProxySettings
{
    /// <inheritdoc cref="IProxySettings.IsAutoCheckScriptUpdate"/>
    public static SettingsStructProperty<bool, ProxySettings_> IsAutoCheckScriptUpdate { get; } = new(DefaultIsAutoCheckScriptUpdate);

    /// <inheritdoc cref="IProxySettings.IsEnableScript"/>
    public static SettingsStructProperty<bool, ProxySettings_> IsEnableScript { get; } = new(DefaultIsEnableScript);

    /// <inheritdoc cref="IProxySettings.SupportProxyServicesStatus"/>
    public static SettingsProperty<IReadOnlyCollection<string>, ProxySettings_> SupportProxyServicesStatus { get; } = new(DefaultSupportProxyServicesStatus, false);

    /// <inheritdoc cref="IProxySettings.ScriptsStatus"/>
    public static SettingsProperty<IReadOnlyCollection<int>, ProxySettings_> ScriptsStatus { get; } = new(DefaultScriptsStatus);

    /// <inheritdoc cref="IProxySettings.ProgramStartupRunProxy"/>
    public static SettingsStructProperty<bool, ProxySettings_> ProgramStartupRunProxy { get; } = new(DefaultProgramStartupRunProxy);

    /// <inheritdoc cref="IProxySettings.SystemProxyPortId"/>
    public static SettingsStructProperty<int, ProxySettings_> SystemProxyPortId { get; } = new(DefaultSystemProxyPortId, false);

    /// <inheritdoc cref="IProxySettings.SystemProxyIp"/>
    public static SettingsProperty<string, ProxySettings_> SystemProxyIp { get; } = new(DefaultSystemProxyIp, false);

    /// <inheritdoc cref="IProxySettings.OnlyEnableProxyScript"/>
    public static SettingsStructProperty<bool, ProxySettings_> OnlyEnableProxyScript { get; } = new(DefaultOnlyEnableProxyScript, false);

    /// <inheritdoc cref="IProxySettings.ProxyMasterDns"/>
    public static SettingsProperty<string, ProxySettings_> ProxyMasterDns { get; } = new(DefaultProxyMasterDns, false);

    /// <inheritdoc cref="IProxySettings.EnableHttpProxyToHttps"/>
    public static SettingsStructProperty<bool, ProxySettings_> EnableHttpProxyToHttps { get; } = new(DefaultEnableHttpProxyToHttps);

    /// <inheritdoc cref="IProxySettings.Socks5ProxyEnable"/>
    public static SettingsStructProperty<bool, ProxySettings_> Socks5ProxyEnable { get; } = new(DefaultSocks5ProxyEnable, false);

    /// <inheritdoc cref="IProxySettings.Socks5ProxyPortId"/>
    public static SettingsStructProperty<int, ProxySettings_> Socks5ProxyPortId { get; } = new(DefaultSocks5ProxyPortId, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentEnable"/>
    public static SettingsStructProperty<bool, ProxySettings_> TwoLevelAgentEnable { get; } = new(DefaultTwoLevelAgentEnable, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentProxyType"/>
    public static SettingsStructProperty<short, ProxySettings_> TwoLevelAgentProxyType { get; } = new(DefaultTwoLevelAgentProxyType, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentIp"/>
    public static SettingsProperty<string, ProxySettings_> TwoLevelAgentIp { get; } = new(DefaultTwoLevelAgentIp, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentPortId"/>
    public static SettingsStructProperty<int, ProxySettings_> TwoLevelAgentPortId { get; } = new(DefaultTwoLevelAgentPortId, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentUserName"/>
    public static SettingsProperty<string, ProxySettings_> TwoLevelAgentUserName { get; } = new(null, false);

    /// <inheritdoc cref="IProxySettings.TwoLevelAgentPassword"/>
    public static SettingsProperty<string, ProxySettings_> TwoLevelAgentPassword { get; } = new(null, false);

    /// <inheritdoc cref="IProxySettings.ProxyMode"/>
    public static SettingsStructProperty<ProxyMode, ProxySettings_> ProxyMode { get; } = new(DefaultProxyMode);

    /// <inheritdoc cref="IProxySettings.IsProxyGOG"/>
    public static SettingsStructProperty<bool, ProxySettings_> IsProxyGOG { get; } = new(DefaultIsProxyGOG);

    /// <inheritdoc cref="IProxySettings.IsOnlyWorkSteamBrowser"/>
    public static SettingsStructProperty<bool, ProxySettings_> IsOnlyWorkSteamBrowser { get; } = new(DefaultIsOnlyWorkSteamBrowser);
}
