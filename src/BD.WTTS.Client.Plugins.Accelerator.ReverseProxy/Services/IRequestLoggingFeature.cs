// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/IRequestLoggingFeature.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 反向代理服务请求日志特性
/// </summary>
internal interface IRequestLoggingFeature
{
    /// <summary>
    /// 是否启用
    /// </summary>
    bool Enable { get; set; }
}