namespace Mobius.Models;

public sealed record class XunYouVipEndTimeResponse : XunYouBaseResponse<XunYouVipEndTimeResponseData>
{
}

public sealed record class XunYouVipEndTimeResponseData
{
    [JsonPropertyName("server_time")]
    public long ServerTime { get; set; }

    [JsonPropertyName("svip")]
    public XunYouVipEndTimeResponseDataSVIP? SVIP { get; set; }
}

public sealed record class XunYouVipEndTimeResponseDataSVIP
{
    /// <summary>
    /// SVIP 到期时间
    /// </summary>
    [JsonPropertyName("etime")]
    public long ETime { get; set; }

    [JsonPropertyName("expire_time")]
    public long ExpireTime { get; set; }

    [JsonPropertyName("gift_begin_time")]
    public long GiftBeginTime { get; set; }

    [JsonPropertyName("rtime")]
    public long RTime { get; set; }

    [JsonPropertyName("service_type")]
    public string? ServiceType { get; set; }
}