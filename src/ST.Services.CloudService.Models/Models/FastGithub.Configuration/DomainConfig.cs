// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/DomainConfig.cs

using MessagePack;
using System.Net;
using System.Runtime.Serialization.Formatters;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

// ReSharper disable once CheckNamespace
namespace System.Application.Models;

/// <inheritdoc cref="IDomainConfig"/>
[MPObj]
public sealed class DomainConfig : IDomainConfig
{
    [MPKey(0)]
    public bool TlsSni { get; init; }

    [MPKey(1)]
    public string? TlsSniPattern { get; init; }

    [MPKey(2)]
    public bool TlsIgnoreNameMismatch { get; init; }

    [MPKey(3)]
    [MessagePackFormatter(typeof(IPAddressFormatter))]
    public IPAddress? IPAddress { get; init; }

    [MPKey(4)]
    public TimeSpan? Timeout { get; init; }

    [MPKey(5)]
    public Uri? Destination { get; init; }

    [MPKey(6)]
    public ResponseConfig? Response { get; init; }

    [MPKey(7)]
    public string? ForwardDestination { get; init; }

    [MPKey(7)]
    public string? UserAgent { get; init; }

    IResponseConfig? IDomainConfig.Response => Response;
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
    /// 使用的域名
    /// </summary>
    string? ForwardDestination { get; }

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
    /// UserAgent
    /// </summary>
    string? UserAgent { get; }

    /// <summary>
    /// 自定义响应
    /// </summary>
    IResponseConfig? Response { get; }
}

partial class AccelerateProjectDTO : IDomainConfig
{
    bool IDomainConfig.TlsSni => !string.IsNullOrEmpty(ServerName);

    string? IDomainConfig.TlsSniPattern => ServerName;

    bool IDomainConfig.TlsIgnoreNameMismatch => true;

    IPAddress? IDomainConfig.IPAddress
    {
        get
        {
            if (ProxyType == ProxyType.Local && !ForwardDomainIsNameOrIP)
            {
                IPAddress2.TryParse(ForwardDomainIP, out var ip);
                return ip;
            }
            return null;
        }
    }

    string? IDomainConfig.ForwardDestination
    {
        get
        {
            if (ProxyType == ProxyType.Local && ForwardDomainIsNameOrIP)
            {
                return ForwardDomainName;
            }
            return null;
        }
    }

    TimeSpan? IDomainConfig.Timeout => null;

    Uri? IDomainConfig.Destination
    {
        get
        {
            if (ProxyType == ProxyType.Redirect)
            {
                var b = new UriBuilder
                {
                    Scheme = PortId == 443 ? "https" : "http",
                    Host = ForwardDomainName,
                    Port = PortId,
                };
                return b.Uri;
            }
            return null;
        }
    }

    string? IDomainConfig.UserAgent => UserAgent;

    IResponseConfig? IDomainConfig.Response => null; // or null
}

partial class ScriptDTO : IDomainConfig
{
    bool IDomainConfig.TlsSni => true;

    string? IDomainConfig.TlsSniPattern => null;

    bool IDomainConfig.TlsIgnoreNameMismatch => true;

    IPAddress? IDomainConfig.IPAddress => null;

    string? IDomainConfig.ForwardDestination => null;

    TimeSpan? IDomainConfig.Timeout => null;

    Uri? IDomainConfig.Destination => null;

    string? IDomainConfig.UserAgent => null;

    IResponseConfig? IDomainConfig.Response => null; // or null
}