// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/ResponseConfig.cs

using System.Net;

// ReSharper disable once CheckNamespace
namespace System.Application.Models;

/// <inheritdoc cref="IResponseConfig"/>
public class ResponseConfig
{
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;

    public string ContentType { get; init; } = "text/plain;charset=utf-8";

    public string? ContentValue { get; init; }
}

/// <summary>
/// 响应配置
/// </summary>
public interface IResponseConfig
{
    /// <summary>
    /// HTTP 状态码
    /// </summary>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// 内容类型
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// 内容的值
    /// </summary>
    string? ContentValue { get; }
}

partial class AccelerateProjectDTO : IResponseConfig
{
    HttpStatusCode IResponseConfig.StatusCode => throw new NotImplementedException();

    string IResponseConfig.ContentType => throw new NotImplementedException();

    string? IResponseConfig.ContentValue => throw new NotImplementedException();
}