namespace BD.WTTS.Enums;

/// <summary>
/// 应用服务级别，较低的级别注入较少的服务，适用于其他轻量进程业务启动
/// </summary>
[Flags]
public enum AppServicesLevel : uint
{
    /// <summary>
    /// 最小
    /// </summary>
    Min = 0,

    /// <summary>
    /// 服务端 API + Repositories + Storage + UserManager + ModelValidator
    /// </summary>
    ServerApiClient = 2,

    /// <summary>
    /// 图形界面
    /// </summary>
    UI = 4,

    /// <summary>
    /// <see cref="IHttpClientFactory"/> 服务
    /// </summary>
    HttpClientFactory = 8,

    /// <summary>
    /// Hosts 文件
    /// </summary>
    Hosts = 16,

    /// <summary>
    /// AppUpdate + 托盘图标(影响主窗口关闭与退出模式，仅在主进程中才会显示托盘)
    /// </summary>
    AppUpdateAndTrayIcon = 32,

    /// <summary>
    /// Steam 服务组
    /// </summary>
    Steam = 64,

    /// <summary>
    /// Http 代理
    /// </summary>
    HttpProxy = 128,

    /// <summary>
    /// IPC 进程管理员权限
    /// </summary>
    IPCRoot = 256,

    /// <summary>
    /// 仓储层
    /// </summary>
    Repositories = 512,

    //Placeholder = 1024,
    //Placeholder1 = 2048,
    //Placeholder2 = 4096,
    //Placeholder3 = 8192,
    //Placeholder4 = 16384,
    //Placeholder5 = 32768,
    //Placeholder6 = 65536,
    //Placeholder7 = 131072,
    //Placeholder8 = 262144,

    #region Group

    /// <summary>
    /// 主进程所需级别组，仅用于指定 DI 等级，当前进程不一定为主进程
    /// </summary>
    MainProcess =
        ServerApiClient |
        UI |
        HttpClientFactory |
        Hosts |
        AppUpdateAndTrayIcon |
        Steam |
        HttpProxy,

    #endregion
}

#if DEBUG
[Obsolete("use AppServicesLevel", true)]
public enum DILevel { }
#endif