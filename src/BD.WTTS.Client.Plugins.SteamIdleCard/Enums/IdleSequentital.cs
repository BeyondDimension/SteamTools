namespace BD.WTTS.Enums;

/// <summary>
/// 运行顺序
/// </summary>
public enum IdleSequentital : byte
{
    /// <summary>
    /// 默认（按字母顺序）
    /// </summary>
    [Description("L10nAuthIdle_Text127")]
    Default = 0,

    /// <summary>
    /// 按卡片数量最少优先
    /// </summary>
    [Description("L10nAuthIdle_Text128")]
    LeastCards = 1,

    /// <summary>
    /// 按卡片数量最多优先
    /// </summary>
    [Description("L10nAuthIdle_Text129")]
    Mostcards = 2,

    /// <summary>
    /// 按卡片价值最高优先
    /// </summary>
    [Description("L10nAuthIdle_Text130")]
    Mostvalue = 3,

}
