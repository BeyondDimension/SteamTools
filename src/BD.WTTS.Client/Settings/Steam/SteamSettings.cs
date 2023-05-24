#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1634 // File header should show copyright
// <console-tools-generated/>
#pragma warning restore SA1634 // File header should show copyright
#pragma warning restore IDE0079 // 请删除不必要的忽略
using static BD.WTTS.Settings.Abstractions.ISteamSettings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(SteamSettings_))]
internal partial class SteamSettingsContext : JsonSerializerContext
{
    static SteamSettingsContext? instance;

    public static SteamSettingsContext Instance
        => instance ??= new SteamSettingsContext(ISettings.GetDefaultOptions());
}

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class SteamSettings_ : ISteamSettings, ISettings, ISettings<SteamSettings_>
{
    public const string Name = nameof(SteamSettings);

    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => SteamSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => SteamSettingsContext.Instance.SteamSettings_;

    static JsonTypeInfo<SteamSettings_> ISettings<SteamSettings_>.JsonTypeInfo
        => SteamSettingsContext.Instance.SteamSettings_;

    /// <summary>
    /// Steam 启动参数
    /// </summary>
    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string? SteamStratParameter { get; set; }

    /// <summary>
    /// Steam 皮肤
    /// </summary>
    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string? SteamSkin { get; set; }

    /// <summary>
    /// Steam 默认程序路径
    /// </summary>
    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public string? SteamProgramPath { get; set; }

    /// <summary>
    /// 自动运行 Steam
    /// </summary>
    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public bool? IsAutoRunSteam { get; set; }

    /// <summary>
    /// Steam 启动时最小化到托盘
    /// </summary>
    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public bool? IsRunSteamMinimized { get; set; }

    /// <summary>
    /// Steam 启动时不检查更新
    /// </summary>
    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool? IsRunSteamNoCheckUpdate { get; set; }

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam 国服）启动
    /// </summary>
    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public bool? IsRunSteamChina { get; set; }

    /// <summary>
    /// Steam 登录时弹出消息通知
    /// </summary>
    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public bool? IsEnableSteamLaunchNotification { get; set; }

    /// <summary>
    /// Steam 下载完成执行任务
    /// </summary>
    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public OSExitMode? DownloadCompleteSystemEndMode { get; set; }

    /// <summary>
    /// Steam 以管理员权限运行
    /// </summary>
    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool? IsRunSteamAdministrator { get; set; }

}
public static partial class SteamSettings
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    /// <summary>
    /// Steam 启动参数
    /// </summary>
    public static SettingsProperty<string, SteamSettings_> SteamStratParameter { get; }
        = new(DefaultSteamStratParameter);

    /// <summary>
    /// Steam 皮肤
    /// </summary>
    public static SettingsProperty<string, SteamSettings_> SteamSkin { get; }
        = new(DefaultSteamSkin);

    /// <summary>
    /// Steam 默认程序路径
    /// </summary>
    public static SettingsProperty<string, SteamSettings_> SteamProgramPath { get; }
        = new(DefaultSteamProgramPath);

    /// <summary>
    /// 自动运行 Steam
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsAutoRunSteam { get; }
        = new(DefaultIsAutoRunSteam);

    /// <summary>
    /// Steam 启动时最小化到托盘
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamMinimized { get; }
        = new(DefaultIsRunSteamMinimized);

    /// <summary>
    /// Steam 启动时不检查更新
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamNoCheckUpdate { get; }
        = new(DefaultIsRunSteamNoCheckUpdate);

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam 国服）启动
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamChina { get; }
        = new(DefaultIsRunSteamChina);

    /// <summary>
    /// Steam 登录时弹出消息通知
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsEnableSteamLaunchNotification { get; }
        = new(DefaultIsEnableSteamLaunchNotification);

    /// <summary>
    /// Steam 下载完成执行任务
    /// </summary>
    public static SettingsStructProperty<OSExitMode, SteamSettings_> DownloadCompleteSystemEndMode { get; }
        = new(DefaultDownloadCompleteSystemEndMode);

    /// <summary>
    /// Steam 以管理员权限运行
    /// </summary>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamAdministrator { get; }
        = new(DefaultIsRunSteamAdministrator);
#endif
}
