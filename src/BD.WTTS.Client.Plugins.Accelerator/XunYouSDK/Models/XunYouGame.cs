namespace Mobius.Models;

/// <summary>
/// 迅游游戏信息
/// <para>13.接口 xunyou_get_all_games</para>
/// <para>14.接口 xunyou_get_hot_games</para>
/// </summary>
[MP2Obj(MP2SerializeLayout.Explicit)]
public sealed partial record class XunYouGame
{
    /// <summary>
    /// 迅游库中的游戏 Id
    /// </summary>
    [JsonPropertyName("game_id")]
    [MP2Key(0)]
    public int Id { get; set; }

    /// <summary>
    /// 游戏名称
    /// </summary>
    [JsonPropertyName("game_name")]
    [MP2Key(1)]
    public string? Name { get; set; }

    /// <summary>
    /// 游戏图标地址
    /// </summary>
    [JsonPropertyName("game_icon_url")]
    [MP2Key(2)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 游戏图片地址
    /// </summary>
    [JsonPropertyName("game_pic_url")]
    [MP2Key(3)]
    public string? PicUrl { get; set; }

    /// <summary>
    /// 游戏图片的 <see cref="MD5"/>
    /// </summary>
    [JsonPropertyName("game_pic_md5_url")]
    [MP2Key(4)]
    public string? PicMD5 { get; set; }

    /// <summary>
    /// 游戏 Logo 图片地址
    /// </summary>
    [JsonPropertyName("game_logo_url")]
    [MP2Key(5)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// 游戏 Logo 图片的 <see cref="MD5"/>
    /// </summary>
    [JsonPropertyName("game_logo_md5_url")]
    [MP2Key(6)]
    public string? LogoMD5 { get; set; }
}
