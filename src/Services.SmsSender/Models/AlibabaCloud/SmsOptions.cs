namespace System.Application.Models.AlibabaCloud;

public class SmsOptions : IExplicitHasValue
{
    /// <summary>
    /// 平台分配的appkey
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// 平台分配的appSecret
    /// </summary>
    public string? AccessKeySecret { get; set; }

    /// <summary>
    /// 短信内容签名，正式环境后申请APP名称签名，此处填写APP名称
    /// </summary>
    public string? SignName { get; set; }

    public SmsOptionsTemplateId<string>[]? Templates { get; set; }

    /// <summary>
    /// (默认)短信模板ID，发送国际/港澳台消息时，请使用国际/港澳台短信模版
    /// </summary>
    public string? DefaultTemplate { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(AccessKeyId) &&
            !string.IsNullOrWhiteSpace(AccessKeySecret) &&
            !string.IsNullOrWhiteSpace(SignName) &&
            (Templates.Any_Nullable() || DefaultTemplate != default);
    }

    bool IExplicitHasValue.ExplicitHasValue() => IsValid();
}