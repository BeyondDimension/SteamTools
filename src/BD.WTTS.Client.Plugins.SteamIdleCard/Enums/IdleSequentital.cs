namespace BD.WTTS.Enums;

/// <summary>
/// 运行顺序
/// </summary>
public enum IdleSequentital : byte
{
    /// <summary>
    /// 默认（按字母顺序）
    /// </summary>
    [Description("默认（按字母顺序）")]
    Default = 0,

    /// <summary>
    /// 按卡片数量最少优先
    /// </summary>
    [Description("按卡片数量最少优先")]
    LeastCards = 1,

    /// <summary>
    /// 按卡片数量最多优先
    /// </summary>
    [Description("按卡片数量最多优先")]
    Mostcards = 2,

    /// <summary>
    /// 按卡片价值最高优先
    /// </summary>
    [Description("按卡片价值最高优先")]
    Mostvalue = 3,

}
