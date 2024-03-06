namespace Mobius.Models;

/// <summary>
/// 迅游加速状态模型类
/// </summary>
[MP2Obj(MP2SerializeLayout.Explicit)]
public partial class XunYouAccelStateModel
{
    /// <inheritdoc cref="XunYouState"/>
    [MP2Key(0)]
    public XunYouState State { get; set; }

    /// <summary>
    /// 当前加速的游戏 id。
    /// </summary>
    [MP2Key(1)]
    public int GameId { get; set; }

    /// <summary>
    /// 当前加速的游戏区 id。
    /// </summary>
    [MP2Key(2)]
    public int AreaId { get; set; }

    /// <summary>
    /// 当前加速游戏的服 id，没有则为 0。
    /// </summary>
    [MP2Key(3)]
    public int ServerId { get; set; }

    /// <inheritdoc cref="XunYouAccelStateEx"/>
    [MP2Key(4)]
    public XunYouAccelStateEx? AccelState { get; set; }
}
