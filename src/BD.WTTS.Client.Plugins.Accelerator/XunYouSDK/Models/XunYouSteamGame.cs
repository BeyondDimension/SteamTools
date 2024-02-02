namespace Mobius.Models;

/// <summary>
/// 迅游 Steam 游戏信息
/// <para>9.接口 xunyou_get_steam_games</para>
/// </summary>
public sealed record class XunYouSteamGame
{
    /// <summary>
    /// Steam 中的游戏 Id
    /// </summary>
    [JsonPropertyName("steamid")]
    public int SteamAppId { get; set; }

    /// <summary>
    /// 迅游库中的游戏 Id
    /// </summary>
    [JsonPropertyName("xunyou_gameid")]
    public int Id { get; set; }

    /// <summary>
    /// 游戏名称
    /// </summary>
    [JsonPropertyName("game_name")]
    public string? Name { get; set; }

    /// <summary>
    /// 游戏图标地址
    /// </summary>
    [JsonPropertyName("game_icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 游戏图片地址
    /// </summary>
    [JsonPropertyName("game_pic_url")]
    public string? PicUrl { get; set; }
}