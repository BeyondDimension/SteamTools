using System.Diagnostics.CodeAnalysis;
using static System.Application.Services.CloudService.Constants;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 加速项目
    /// </summary>
    [MPObj]
    public class AccelerateProjectDTO
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// 端口号
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public ushort PortId { get; set; }

        /// <summary>
        /// 域名，分号分割多个
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? DomainNames { get; set; } = string.Empty;

        string? mDomainNames;
        string[]? mDomainNamesArray;
        readonly object mDomainNamesArrayLock = new();

        /// <inheritdoc cref="DomainNames"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[] DomainNamesArray
            => GetSplitValues(mDomainNamesArrayLock, DomainNames, ref mDomainNames, ref mDomainNamesArray);

        /// <summary>
        /// 转发域名
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? ForwardDomainName { get; set; } = string.Empty;

        /// <summary>
        /// 转发域名IP
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? ForwardDomainIP { get; set; } = string.Empty;

        /// <summary>
        /// 转发是域名(<see langword="true"/>)还是域名IP(<see langword="false"/>)
        /// </summary>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool ForwardDomainIsNameOrIP => string.IsNullOrEmpty(ForwardDomainIP);

        /// <summary>
        /// 服务器名
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? ServerName { get; set; } = string.Empty;

        /// <summary>
        /// 启用重定向
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public bool Redirect { get; set; }

        /// <summary>
        /// Host 域名集合
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Hosts { get; set; } = string.Empty;

        string? mHosts;
        string[]? mHostsArray;
        readonly object mHostsArrayLock = new();

        /// <inheritdoc cref="Hosts"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[] HostsArray
            => GetSplitValues(mHostsArrayLock, Hosts, ref mHosts, ref mHostsArray);
    }
}