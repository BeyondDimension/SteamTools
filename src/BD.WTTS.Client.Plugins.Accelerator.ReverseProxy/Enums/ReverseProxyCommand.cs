namespace BD.WTTS.Enums;

/// <summary>
/// 反向代理 IPC 数据包命令
/// </summary>
public enum ReverseProxyCommand : byte
{
    /// <summary>
    /// 退出反向代理控制台子服务
    /// </summary>
    Exit = 1,

    /// <summary>
    /// 启动反向代理控制台子服务
    /// </summary>
    Start,

    /// <summary>
    /// 停止反向代理控制台子服务
    /// </summary>
    Stop,

    /// <summary>
    /// 获取当前流量统计
    /// </summary>
    GetFlowStatistics,

    /// <summary>
    /// 调用 UI 进程通知显示 DNS 错误
    /// </summary>
    NotifyDNSError,

    /// <summary>
    /// 热重载配置文件
    /// </summary>
    HotReloadConfig,

    /// <summary>
    /// 启动反向代理控制台结果
    /// </summary>
    StartResult,
}
