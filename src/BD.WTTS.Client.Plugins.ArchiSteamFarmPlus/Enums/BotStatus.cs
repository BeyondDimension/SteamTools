namespace BD.WTTS.Enums;

/// <summary>
/// <see cref="Bot"/> 状态
/// </summary>
public enum BotStatus : byte
{
    /// <summary>
    /// 禁用
    /// </summary>
    [Description("禁用")]
    Disabled = 0,

    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    OffLine = 1,

    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")]
    Online = 2,

    /// <summary>
    /// 正在挂卡
    /// </summary>
    [Description("正在挂卡")]
    Farming = 3
}
