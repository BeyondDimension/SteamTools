namespace Mobius.Models;

public abstract record class XunYouBaseRequest
{
    /// <summary>
    /// 返回已排序的字典，用于生成签名
    /// </summary>
    /// <returns></returns>
    public virtual IReadOnlyDictionary<string, string> ToDictionary()
    {
        var dict = new SortedDictionary<string, string>();

        if (!string.IsNullOrEmpty(UserId))
            dict.Add("user_id", UserId);

        if (!string.IsNullOrEmpty(ChannelNo))
            dict.Add("channel_no", ChannelNo);

        if (!string.IsNullOrEmpty(ChannelType))
            dict.Add("channel_type", ChannelType);

        dict.Add("timestamp", Timestamp.ToString());

        if (!string.IsNullOrEmpty(SignType))
            dict.Add("sign_type", SignType);

        if (!string.IsNullOrEmpty(SignVersion))
            dict.Add("sign_ver", SignVersion);

        if (!string.IsNullOrEmpty(Brand))
            dict.Add("brand", Brand);

        return dict;
    }

    /// <summary>
    /// 用户账号
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 渠道编号
    /// </summary>
    [JsonPropertyName("channel_no")]
    public string? ChannelNo { get; set; } = "wattdistr";

    /// <summary>
    /// 渠道类型
    /// </summary>
    [JsonPropertyName("channel_type")]
    public string? ChannelType { get; set; } = "thirdparty";

    /// <summary>
    /// 签名
    /// </summary>
    [JsonPropertyName("sign")]
    public string? Sign { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTime.Now.ToUnixTimeMilliseconds();

    /// <summary>
    /// 签名类型
    /// </summary>
    [JsonPropertyName("sign_type")]
    public string? SignType { get; set; } = "md5";

    /// <summary>
    /// 签名版本号
    /// </summary>
    [JsonPropertyName("sign_ver")]
    public string? SignVersion { get; set; } = "1.0";

    /// <summary>
    /// 品牌
    /// </summary>
    [JsonPropertyName("brand")]
    public string? Brand { get; set; } = "watt";
}
