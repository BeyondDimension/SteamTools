namespace Mobius.Models;

public abstract record class XunYouBaseResponse<TData> where TData : notnull
{
    [JsonPropertyName("code")]
    public XunYouBaseResponseCode Code { get; set; }

    /// <summary>
    /// 错误提示，报错时返回
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 返回数据
    /// </summary>
    [JsonPropertyName("data")]
    public TData? Data { get; set; }
}