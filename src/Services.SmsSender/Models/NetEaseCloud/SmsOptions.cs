namespace System.Application.Models.NetEaseCloud;

public class SmsOptions : IExplicitHasValue
{
    /// <summary>
    /// 开发者平台分配的appkey
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// 开发者平台分配的appSecret
    /// </summary>
    public string? AppSecret { get; set; }

    /// <summary>
    /// 开发者平台分配的模板标志
    /// </summary>
    public SmsOptionsTemplateId<int>[]? Templates { get; set; }

    public int? DefaultTemplate { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(AppKey) &&
            !string.IsNullOrWhiteSpace(AppSecret) &&
            (Templates.Any_Nullable() || DefaultTemplate != default);
    }

    bool IExplicitHasValue.ExplicitHasValue() => IsValid();
}