namespace BD.WTTS.Enums;

/// <summary>
/// 运行规则
/// </summary>
public enum IdleRule : byte
{
    /// <summary>
    /// 顺序运行
    /// </summary>
    [Description("按顺序运行")]
    OnlyOneGame = 0,

    /// <summary>
    /// 优先单独运行已满2小时的游戏，然后再同时挂卡
    /// </summary>
    [Description("优先单独运行已满2小时的游戏，然后再同时挂卡")]
    OneThenMany = 1,

    /// <summary>
    /// 将所有游戏运行满2小时后再按顺序运行
    /// </summary>
    [Description("将所有游戏运行满2小时后再按顺序运行")]
    ManyThenOne = 2,
}
