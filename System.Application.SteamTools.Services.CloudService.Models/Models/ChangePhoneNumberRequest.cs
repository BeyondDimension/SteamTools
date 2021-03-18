using System.Application.Columns;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 请求 - 换绑手机
    /// </summary>
    public static class ChangePhoneNumberRequest
    {
        [MPObj]
        public class Validation : IReadOnlySmsCode
        {
            [MPKey(0)]
            [N_JsonProperty("0")]
            [S_JsonProperty("0")]
            public string? SmsCode { get; set; }

            [MPKey(1)]
            [N_JsonProperty("1")]
            [S_JsonProperty("1")]
            public string? PhoneNumber { get; set; }
        }

        [MPObj]
        public class New : IReadOnlyPhoneNumber, IReadOnlySmsCode
        {
            [MPKey(0)]
            [N_JsonProperty("0")]
            [S_JsonProperty("0")]
            public string? PhoneNumber { get; set; }

            [MPKey(1)]
            [N_JsonProperty("1")]
            [S_JsonProperty("1")]
            public string? SmsCode { get; set; }

            /// <summary>
            /// 上一个接口返回的CODE
            /// </summary>
            [MPKey(2)]
            [N_JsonProperty("2")]
            [S_JsonProperty("2")]
            public string? Code { get; set; }
        }
    }
}
