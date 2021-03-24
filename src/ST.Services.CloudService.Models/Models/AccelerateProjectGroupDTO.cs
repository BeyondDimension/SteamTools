using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 加速项目组
    /// </summary>
    [MPObj]
    public class AccelerateProjectGroupDTO
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
        /// 当前组中所有的加速项目集合
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        [NotNull, DisallowNull] // C# 8 not null
        public List<AccelerateProjectDTO>? Items = new();

        /// <summary>
        /// 显示图片，使用 System.Application.ImageUrlHelper.GetImageApiUrlById(Guid) 转换为Url
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public Guid ImageId { get; set; }
    }
}