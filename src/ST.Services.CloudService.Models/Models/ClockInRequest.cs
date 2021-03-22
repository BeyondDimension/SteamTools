using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 签到请求
    /// </summary>
    [MPObj]
    public class ClockInRequest
    {
        /// <summary>
        /// 当前创建时间，仅用来验证时间差
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public DateTimeOffset CreationTime { get; set; }
    }
}