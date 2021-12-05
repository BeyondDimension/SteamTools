using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public class NoticeDTO
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public Guid? ContentId { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string Picture { get; set; } = string.Empty;
        /// <summary>
        /// 标题
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 作者
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 标题
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public string Introduction { get; set; } = string.Empty;

        /// <summary>
        /// 消息添加时间
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 消息启用时间（预约添加）
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        public DateTimeOffset EnableTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 是否浏览器打开
        /// </summary>
        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public bool IsOpenBrowser { get; set; }
    }
}
