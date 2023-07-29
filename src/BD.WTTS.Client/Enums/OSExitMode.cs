namespace BD.WTTS.Enums;

/// <summary>
/// 退出操作系统方式
/// </summary>
public enum OSExitMode : byte
{
    /// <summary>
    /// 睡眠
    /// </summary>
    Sleep,

    /// <summary>
    /// 休眠
    /// </summary>
    Hibernate,

    /// <summary>
    /// 关机
    /// </summary>
    Shutdown,

    /// <summary>
    /// 锁屏
    /// </summary>
    Lock,

    /// <summary>
    /// 注销
    /// </summary>
    Logout,
}

#if DEBUG
[Obsolete("use OSExitMode", true)]
public enum SystemEndMode { }
#endif