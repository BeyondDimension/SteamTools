namespace System.Application.Models.Abstractions;

public interface ISmsResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// 返回的内容
    /// </summary>
    ISmsSubResult? Result { get; }

    /// <summary>
    /// 返回的HTTP状态码
    /// </summary>
    int HttpStatusCode { get; }
}