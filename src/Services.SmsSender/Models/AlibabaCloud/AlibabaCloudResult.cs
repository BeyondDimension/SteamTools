namespace System.Application.Models.AlibabaCloud;

public class AlibabaCloudResult<T> : JsonModel<T> where T : AlibabaCloudResult<T>
{
    /// <summary>
    /// https://help.aliyun.com/document_detail/55323.html
    /// </summary>
    public string? Code { get; set; }

    public virtual bool IsOK() => Code?.Equals("OK", StringComparison.OrdinalIgnoreCase) ?? false;
}