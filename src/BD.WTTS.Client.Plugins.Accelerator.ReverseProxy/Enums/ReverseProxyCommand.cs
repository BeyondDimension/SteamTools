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
}
