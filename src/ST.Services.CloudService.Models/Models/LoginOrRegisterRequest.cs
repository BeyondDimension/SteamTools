using System.Application.Columns;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 登录或注册接口请求模型
    /// </summary>
    [MPObj]
    public class LoginOrRegisterRequest : IReadOnlyPhoneNumber, IReadOnlySmsCode
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public string? PhoneNumber { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? SmsCode { get; set; }
    }
}