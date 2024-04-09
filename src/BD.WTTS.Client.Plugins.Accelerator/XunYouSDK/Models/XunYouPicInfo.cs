namespace Mobius.Models;

/// <summary>
/// 同步获取对应游戏 id 的图片信息
/// <para>24.接口 xunyou_get_picinfo</para>
/// </summary>
public sealed record class XunYouPicInfo
{
    /// <summary>
    /// 图标
    /// </summary>
    [JsonPropertyName("icon")]
    public XunYouPicInfoUrl? Icon { get; set; }

    /// <summary>
    /// 大图
    /// </summary>
    [JsonPropertyName("large")]
    public XunYouPicInfoUrl? Large { get; set; }

    /// <summary>
    /// Logo
    /// </summary>
    [JsonPropertyName("logo")]
    public XunYouPicInfoUrl? Logo { get; set; }

    /// <summary>
    /// 小图
    /// </summary>
    [JsonPropertyName("small")]
    public XunYouPicInfoUrl? Small { get; set; }
}