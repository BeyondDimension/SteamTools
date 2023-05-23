namespace BD.WTTS.Settings.Abstractions;

/// <summary>
/// Steam 设置
/// </summary>
public interface ISteamSettings
{
    static ISteamSettings? Instance => Ioc.Get_Nullable<IOptionsMonitor<ISteamSettings>>()?.CurrentValue;

    /// <summary>
    /// Steam 启动参数
    /// </summary>
    string? SteamStratParameter { get; set; }

    /// <summary>
    /// Steam 皮肤
    /// </summary>
    string? SteamSkin { get; set; }

    /// <summary>
    /// Steam 默认程序路径
    /// </summary>
    string? SteamProgramPath { get; set; }

    /// <summary>
    /// 用户自定义 Steam 程序路径
    /// </summary>
    string? CustomSteamPath { get; set; }

    /// <summary>
    /// 自动运行 Steam
    /// </summary>
    bool? IsAutoRunSteam { get; set; }

    const bool DefaultIsAutoRunSteam = false;

    /// <summary>
    /// Steam 启动时最小化到托盘
    /// </summary>
    bool? IsRunSteamMinimized { get; set; }

    const bool DefaultIsRunSteamMinimized = false;

    /// <summary>
    /// Steam 启动时不检查更新
    /// </summary>
    bool? IsRunSteamNoCheckUpdate { get; set; }

    const bool DefaultIsRunSteamNoCheckUpdate = false;

    /// <summary>
    /// Steam 启动时模拟为蒸汽平台（Steam 国服）启动
    /// </summary>
    bool? IsRunSteamChina { get; set; }

    const bool DefaultIsRunSteamChina = false;

    /// <summary>
    /// 检测到 Steam 登录时弹出消息通知
    /// </summary>
    bool? IsEnableSteamLaunchNotification { get; set; }

    const bool DefaultIsEnableSteamLaunchNotification = true;

    /// <summary>
    /// Steam 下载完成执行任务
    /// </summary>
    OSExitMode? DownloadCompleteSystemEndMode { get; set; }

    const OSExitMode DefaultDownloadCompleteSystemEndMode = OSExitMode.Sleep;

    /// <summary>
    /// 以管理员权限运行 Steam
    /// </summary>
    bool? IsRunSteamAdministrator { get; set; }

    const bool DefaultIsRunSteamAdministrator = false;
}
