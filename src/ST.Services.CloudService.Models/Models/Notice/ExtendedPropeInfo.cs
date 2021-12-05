using System.Collections.Generic;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public class NoticeExtendedPropeInfo
    {
        /// <summary>
        /// 字体粗细
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public FontWeight FontWeight { get; set; }

        /// <summary>
        /// 字体颜色 
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public uint FontColor { get; set; }

        /// <summary>
        /// 对其方式
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public FontAlign FontAlign { get; set; } = FontAlign.Left;

        /// <summary>
        /// 下划线 or 删除线
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public FontDecoration FontDecoration { get; set; } = FontDecoration.None;
    }
}
