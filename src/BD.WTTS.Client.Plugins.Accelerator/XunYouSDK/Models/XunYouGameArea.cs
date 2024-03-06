namespace Mobius.Models;

/// <summary>
/// 迅游游戏区服信息
/// </summary>
[MP2Obj(MP2SerializeLayout.Explicit)]
public sealed partial record class XunYouGameArea
{
    /// <summary>
    /// 区服 Id
    /// </summary>
    [JsonPropertyName("area_id")]
    [MP2Key(0)]
    public int Id { get; set; }

    /// <summary>
    /// 区服名称
    /// </summary>
    [JsonPropertyName("area_name")]
    [MP2Key(1)]
    public string? Name { get; set; }

    /// <summary>
    /// 游戏服务器信息
    /// </summary>
    [JsonPropertyName("game_server")]
    [MP2Key(2)]
    public List<XunYouGameServer>? Servers { get; set; }
}
