// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/DomainConfig.cs

using System.Net;

// ReSharper disable once CheckNamespace
namespace System.Application.Models;

/// <inheritdoc cref="IDomainConfig"/>
public sealed class DomainConfig : IDomainConfig
{
    public bool TlsSni { get; init; }

    public string? TlsSniPattern { get; init; }

    public bool TlsIgnoreNameMismatch { get; init; }

    public IPAddress? IPAddress { get; init; }

    public TimeSpan? Timeout { get; init; }

    public Uri? Destination { get; init; }

    public IResponseConfig? Response { get; init; }
}

/// <summary>
/// 域名配置
/// </summary>
public interface IDomainConfig
{
    /// <summary>
    /// 是否发送 SNI
    /// </summary>
    bool TlsSni { get; }

    /// <summary>
    /// 自定义 SNI 值的表达式
    /// </summary>
    string? TlsSniPattern { get; }

    /// <summary>
    /// 是否忽略服务器证书域名不匹配
    /// <para>当不发送 SNI 时服务器可能发回域名不匹配的证书</para>
    /// </summary>
    bool TlsIgnoreNameMismatch { get; }

    /// <summary>
    /// 使用的 IP 地址
    /// </summary>
    IPAddress? IPAddress { get; }

    /// <summary>
    /// 请求超时时长
    /// </summary>
    TimeSpan? Timeout { get; }

    /// <summary>
    /// 目的地
    /// <para>格式为相对或绝对 <see cref="Uri"/></para>
    /// </summary>
    Uri? Destination { get; }

    /// <summary>
    /// 自定义响应
    /// </summary>
    IResponseConfig? Response { get; }
}

partial class AccelerateProjectDTO : IDomainConfig
{
    bool IDomainConfig.TlsSni => ServerName != null;

    string? IDomainConfig.TlsSniPattern => ServerName;

    bool IDomainConfig.TlsIgnoreNameMismatch => false;

    IPAddress? IDomainConfig.IPAddress => IPAddress.Parse(ForwardDomainIP);

    TimeSpan? IDomainConfig.Timeout => null;

    Uri? IDomainConfig.Destination => new(ForwardDomainName);

    IResponseConfig? IDomainConfig.Response => null; // or null
}