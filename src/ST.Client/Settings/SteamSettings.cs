using System.Application.Models;
using System.Application.UI;
using System.Runtime.Versioning;

namespace System.Application.Settings;

[SupportedOSPlatform("Windows7.0")]
[SupportedOSPlatform("macOS")]
[SupportedOSPlatform("Linux")]
public sealed class SteamSettings : SettingsHost2<SteamSettings>
{
    static SteamSettings()
    {
        if (!IApplication.IsDesktopPlatform) return;
        IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
        IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        IsRunSteamChina.ValueChanged += IsRunSteamChina_ValueChanged;
    }

    static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -noverifyfiles";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-noverifyfiles", "").Trim();
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -silent";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-silent", "").Trim();
    }

    static void IsRunSteamChina_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -steamchina";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-steamchina", "").Trim();
    }

    static readonly SerializableProperty<string?>? _SteamStratParameter = IApplication.IsDesktopPlatform ?
        GetProperty<string?>(defaultValue: null) : null;

    /// <summary>
    /// Steam 启动参数
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<string?> SteamStratParameter => _SteamStratParameter ?? throw new PlatformNotSupportedException();

    //static readonly SerializableProperty<string>? _SteamSkin = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: string.Empty, autoSave: true) : null;
    ///// <summary>
    ///// Steam 皮肤
    ///// </summary>
    //[SupportedOSPlatform("Windows7.0")]
    //[SupportedOSPlatform("macOS")]
    //[SupportedOSPlatform("Linux")]
    //public static SerializableProperty<string> SteamSkin => _SteamSkin ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _IsAutoRunSteam = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 自动运行 Steam
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsAutoRunSteam => _IsAutoRunSteam ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _IsRunSteamMinimized = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// Steam 启动时最小化到托盘
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsRunSteamMinimized => _IsRunSteamMinimized ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _IsRunSteamNoCheckUpdate = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// Steam 启动时不检查更新
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsRunSteamNoCheckUpdate => _IsRunSteamNoCheckUpdate ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _IsRunSteamChina = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam国服）启动
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsRunSteamChina => _IsRunSteamChina ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _IsEnableSteamLaunchNotification = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: true) : null;

    /// <summary>
    /// 检测到 Steam 登录时弹出消息通知
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsEnableSteamLaunchNotification => _IsEnableSteamLaunchNotification ?? throw new PlatformNotSupportedException();

    /// <summary>
    /// Steam 下载完成执行任务
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<SystemEndMode>? DownloadCompleteSystemEndMode = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: SystemEndMode.Sleep) : null;

    static readonly SerializableProperty<bool>? _IsRunSteamAdministrator = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 以管理员权限运行 Steam
    /// </summary>
    [SupportedOSPlatform("Windows7.0")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> IsRunSteamAdministrator => _IsRunSteamAdministrator ?? throw new PlatformNotSupportedException();

}