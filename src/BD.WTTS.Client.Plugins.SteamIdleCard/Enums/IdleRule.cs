namespace BD.WTTS.Enums;

/// <summary>
/// 运行规则
/// </summary>
public enum IdleRule : byte
{
    /// <summary>
    /// 快速掉卡模式
    /// </summary>
    [Description("L10nAuthIdle_Text123")]
    FastMode = 0,

    /// <summary>
    /// 顺序运行
    /// </summary>
    [Description("L10nAuthIdle_Text124")]
    OnlyOneGame = 1,

    /// <summary>
    /// 先挂卡已满（游戏最小运行时间）的游戏，然后再并行挂时长
    /// </summary>
    [Description("L10nAuthIdle_Text125")]
    OneThenMany = 2,

    /// <summary>
    /// 先并行挂时长满（游戏最小运行时间）后再挂卡
    /// </summary>
    [Description("L10nAuthIdle_Text126")]
    ManyThenOne = 3,
}
