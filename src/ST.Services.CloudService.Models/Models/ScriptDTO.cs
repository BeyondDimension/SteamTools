#if MVVM_VM
using ReactiveUI;
#endif
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
    /// 脚本
    /// </summary>
    [MPObj]
    public class ScriptDTO
#if MVVM_VM
        : ReactiveObject
#endif
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
        /// 脚本作者
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Author { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Version { get; set; } = string.Empty;

        /// <summary>
        /// 来源地址
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? SourceLink { get; set; } = string.Empty;

        /// <summary>
        /// 下载地址
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? DownloadLink { get; set; } = string.Empty;

        /// <summary>
        /// 更新地址
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? UpdateLink { get; set; } = string.Empty;

        /// <summary>
        /// 说明
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// 脚本匹配域名，分号分割多个
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? MatchDomainNames { get; set; } = string.Empty;

        /// <summary>
        /// 是否默认启用
        /// </summary>
        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public bool Enable
#if MVVM_VM
        {
            get => mEnable;
            set => this.RaiseAndSetIfChanged(ref mEnable, value);
        }
        bool mEnable;
#else
        { get; set; }
#endif

        string? mMatchDomainNames;
        string[]? mMatchDomainNamesArray;
        readonly object mMatchDomainNamesArrayLock = new();

        /// <inheritdoc cref="MatchDomainNames"/>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string[] MatchDomainNamesArray
            => GetSplitValues(mMatchDomainNamesArrayLock, MatchDomainNames, ref mMatchDomainNames, ref mMatchDomainNamesArray);
    }
}