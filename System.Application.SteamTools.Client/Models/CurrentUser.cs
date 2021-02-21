using System.Application.Columns;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 当前登录用户模型，如需增加字段，还需要在 <see cref="Clone"/> 中赋值新添加字段
    /// </summary>
    [MPObj]
    public sealed class CurrentUser : ILoginResponse, IExplicitHasValue, IPhoneNumber
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid UserId { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? AuthToken { get; set; }

        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string? PhoneNumber { get; set; }

        bool IExplicitHasValue.ExplicitHasValue()
        {
            if (string.IsNullOrWhiteSpace(AuthToken)) return false;
            return true;
        }

        public CurrentUser? Clone() => this.HasValue() ?
         new CurrentUser
         {
             UserId = UserId,
             AuthToken = AuthToken,
             PhoneNumber = PhoneNumber,
         } : null;
    }
}