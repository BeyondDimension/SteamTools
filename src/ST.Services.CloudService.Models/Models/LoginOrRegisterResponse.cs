using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 登录或注册接口响应模型
    /// </summary>
    [MPObj]
    public class LoginOrRegisterResponse : ILoginResponse, IExplicitHasValue
    {
        UserInfoDTO ILoginResponse.User => User ?? throw new ArgumentNullException(nameof(User));

        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public JWTEntity? AuthToken { get; set; }

        /// <summary>
        /// 当前登录的用户信息
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public UserInfoDTO? User { get; set; }

        /// <summary>
        /// 当前操作是登录(<see langword="true"/>)还是注册(<see langword="false"/>)
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public bool IsLoginOrRegister { get; set; }

        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string? PhoneNumber { get; set; }

        bool IExplicitHasValue.ExplicitHasValue()
        {
            var hasToken = AuthToken != null;
            if (IsLoginOrRegister)
            {
                return hasToken;
            }
            else
            {
                return hasToken && User != null;
            }
        }
    }
}