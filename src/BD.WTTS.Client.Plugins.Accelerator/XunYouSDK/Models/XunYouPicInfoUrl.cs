namespace Mobius.Models;

/// <summary>
/// 同步获取对应游戏 id 的图片信息 Url
/// <para>24.接口 xunyou_get_picinfo</para>
/// </summary>
public sealed record class XunYouPicInfoUrl
{
    /// <summary>
    /// 图片资源的 crc 哈希值
    /// </summary>
    [JsonPropertyName("crc")]
    public string? Crc { get; set; }

    /// <summary>
    /// 图片资源的地址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}