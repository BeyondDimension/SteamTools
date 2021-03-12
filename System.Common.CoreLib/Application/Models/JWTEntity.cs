using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// JWT 值
    /// </summary>
    [MPObj]
    [Serializable]
    public sealed class JWTEntity : IExplicitHasValue
    {
        /// <summary>
        /// 凭证有效期
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public DateTimeOffset ExpiresIn { get; set; }

        /// <summary>
        /// 当前凭证
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新凭证
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string? RefreshToken { get; set; }

        bool IExplicitHasValue.ExplicitHasValue()
        {
            // 仅数据格式是否正确，不验证时间有效期等业务逻辑
            return !string.IsNullOrEmpty(AccessToken) &&
                !string.IsNullOrEmpty(RefreshToken);
        }
    }
}