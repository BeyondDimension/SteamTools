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
        /// <summary>
        /// 换绑手机 - 安全验证
        /// <list type="bullet">
        /// <item>【Lable 当前用户的手机号 中间四位隐藏】</item>
        /// <item>【TextBox 输入短信验证码】【Button 获取验证码】</item>
        /// <item>【TextBox 输入新的手机号码】</item>
        /// <item>【Button 提交】</item>
        /// </list>
        /// </summary>
        [MPObj]
        public class Validation : IReadOnlySmsCode
        {
            /// <summary>
            /// 当前手机号的短信验证码
            /// </summary>
            [MPKey(0)]
            [N_JsonProperty("0")]
            [S_JsonProperty("0")]
            public string? SmsCode { get; set; }

            /// <summary>
            /// 要绑定的新手机号码
            /// </summary>
            [MPKey(1)]
            [N_JsonProperty("1")]
            [S_JsonProperty("1")]
            public string? PhoneNumber { get; set; }
        }

        /// <summary>
        /// 换绑手机 - 绑定新手机号
        /// <list type="bullet">
        /// <item>【TextBox 输入新手机号码的短信验证码】</item>
        /// <item>【Button 完成】</item>
        /// </list>
        /// </summary>
        [MPObj]
        public class New : IReadOnlyPhoneNumber, IReadOnlySmsCode
        {
            /// <summary>
            /// 要绑定的新手机号码
            /// </summary>
            [MPKey(0)]
            [N_JsonProperty("0")]
            [S_JsonProperty("0")]
            public string? PhoneNumber { get; set; }

            /// <summary>
            /// 新手机号码的短信验证码
            /// </summary>
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
