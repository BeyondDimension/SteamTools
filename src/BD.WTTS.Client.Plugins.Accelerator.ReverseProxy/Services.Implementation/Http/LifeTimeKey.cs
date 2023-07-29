// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/LifeTimeKey.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 生命周期的 Key
/// </summary>
record LifeTimeKey
{
    /// <summary>
    /// 域名
    /// </summary>
    public string Domain { get; }

    /// <summary>
    /// 域名配置
    /// </summary>
    public IDomainConfig DomainConfig { get; }

    public LifeTimeKey(string domain, IDomainConfig domainConfig)
    {
        Domain = domain;
        DomainConfig = domainConfig;
    }
}