// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/DomainConfig.cs

using MessagePack;
using System.Net;
using System.Runtime.Serialization.Formatters;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

// ReSharper disable once CheckNamespace
namespace System.Application.Models;

/// <summary>
/// 脚本配置
/// </summary>
public interface IScriptConfig
{
    /// <summary>
    /// 本地脚本Id
    /// </summary>
    int LocalId { get; }

    /// <summary>
    /// 排除匹配域名
    /// </summary>
    DomainPattern? ExcludeDomainPattern { get; }

}

partial class ScriptDTO : IScriptConfig
{
    int IScriptConfig.LocalId => LocalId;

    DomainPattern? IScriptConfig.ExcludeDomainPattern => string.IsNullOrWhiteSpace(ExcludeDomainNames) ? null : new DomainPattern(ExcludeDomainNames);
}