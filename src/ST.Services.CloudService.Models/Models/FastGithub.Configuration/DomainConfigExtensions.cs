// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/DomainConfig.cs#L52

using System.Net;
using System.Application.Models;

namespace System;

public static class DomainConfigExtensions
{
    /// <summary>
    /// 获取 <see cref="TlsSniPattern"/>
    /// </summary>
    /// <param name="domainConfig"></param>
    /// <returns></returns>
    public static TlsSniPattern GetTlsSniPattern(this IDomainConfig domainConfig)
    {
        if (domainConfig.TlsSni == false)
        {
            return TlsSniPattern.None;
        }
        if (string.IsNullOrEmpty(domainConfig.TlsSniPattern))
        {
            return TlsSniPattern.Domain;
        }
        return new TlsSniPattern(domainConfig.TlsSniPattern);
    }
}