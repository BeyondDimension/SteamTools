using System.Collections.Generic;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public  class NoticeContentResponse
    {

        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public string Content  { get; set; } = string.Empty;

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public NoticeTypeEnum NoticeType { get; set; } = NoticeTypeEnum.Text;

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string Url { get; set; } = string.Empty;

        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public IList<NoticeExtendedPropeInfo>? ExtendedPropes { get; set; }
    }
}
