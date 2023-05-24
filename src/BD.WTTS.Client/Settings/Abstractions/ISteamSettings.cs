// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface ISteamSettings
{
    static ISteamSettings? Instance
        => Ioc.Get_Nullable<IOptionsMonitor<ISteamSettings>>()?.CurrentValue;

    /// <summary>
    /// Steam 启动参数
    /// </summary>
    string? SteamStratParameter { get; set; }

    /// <summary>
    /// Steam 启动参数的默认值
    /// </summary>
    const string DefaultSteamStratParameter = null;

    /// <summary>
    /// Steam 皮肤
    /// </summary>
    string? SteamSkin { get; set; }

    /// <summary>
    /// Steam 皮肤的默认值
    /// </summary>
    const string DefaultSteamSkin = null;

    /// <summary>
    /// Steam 默认程序路径
    /// </summary>
    string? SteamProgramPath { get; set; }

    /// <summary>
    /// Steam 默认程序路径的默认值
    /// </summary>
    const string DefaultSteamProgramPath = null;

    /// <summary>
    /// 自动运行 Steam
    /// </summary>
    bool? IsAutoRunSteam { get; set; }

    /// <summary>
    /// 自动运行 Steam的默认值
    /// </summary>
    const bool DefaultIsAutoRunSteam = false;

    /// <summary>
    /// Steam 启动时最小化到托盘
    /// </summary>
    bool? IsRunSteamMinimized { get; set; }

    /// <summary>
    /// Steam 启动时最小化到托盘的默认值
    /// </summary>
    const bool DefaultIsRunSteamMinimized = false;

    /// <summary>
    /// Steam 启动时不检查更新
    /// </summary>
    bool? IsRunSteamNoCheckUpdate { get; set; }

    /// <summary>
    /// Steam 启动时不检查更新的默认值
    /// </summary>
    const bool DefaultIsRunSteamNoCheckUpdate = false;

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam 国服）启动
    /// </summary>
    bool? IsRunSteamChina { get; set; }

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam 国服）启动的默认值
    /// </summary>
    const bool DefaultIsRunSteamChina = false;

    /// <summary>
    /// Steam 登录时弹出消息通知
    /// </summary>
    bool? IsEnableSteamLaunchNotification { get; set; }

    /// <summary>
    /// Steam 登录时弹出消息通知的默认值
    /// </summary>
    const bool DefaultIsEnableSteamLaunchNotification = true;

    /// <summary>
    /// Steam 下载完成执行任务
    /// </summary>
    OSExitMode? DownloadCompleteSystemEndMode { get; set; }

    /// <summary>
    /// Steam 下载完成执行任务的默认值
    /// </summary>
    const OSExitMode DefaultDownloadCompleteSystemEndMode = OSExitMode.Sleep;

    /// <summary>
    /// Steam 以管理员权限运行
    /// </summary>
    bool? IsRunSteamAdministrator { get; set; }

    /// <summary>
    /// Steam 以管理员权限运行的默认值
    /// </summary>
    const bool DefaultIsRunSteamAdministrator = false;

}
