namespace BD.WTTS.Enums;

/// <summary>
/// 应用程序 Web 代理模式
/// </summary>
public enum AppWebProxyMode : byte
{
    /// <summary>
    /// 跟随系统
    /// </summary>
    FollowSystem = 0,

    /// <summary>
    /// 无代理
    /// </summary>
    NoProxy,

    /// <summary>
    /// 自定义
    /// </summary>
    Custom,
}
