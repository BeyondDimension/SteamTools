namespace Mobius.Models;

/// <summary>
/// 迅游游戏信息详情
/// <para>15.接口 xunyou_get_gameinfo</para>
/// </summary>
public sealed record class XunYouGameInfo
{
    /// <summary>
    /// 迅游库中的游戏 Id
    /// </summary>
    [JsonPropertyName("game_id")]
    public int Id { get; set; }

    /// <inheritdoc cref="XunYouGameOperatorId"/>
    [JsonPropertyName("game_operator_id")]
    public XunYouGameOperatorId OperatorId { get; set; }

    /// <summary>
    /// 显示启动游戏按钮
    /// </summary>
    [JsonPropertyName("show_start")]
    public bool ShowStart { get; set; }

    /// <summary>
    /// 迅游游戏区服信息
    /// </summary>
    [JsonPropertyName("game_area")]
    public List<XunYouGameArea>? Areas { get; set; }
}