using System.Collections.Generic;
using System.Text;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public partial class RankingResponse
    {
        /// <summary>
        /// 用户昵称 
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public string? Name { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 赞助月份
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public int Month { get; set; }

        /// <summary>
        /// 捐助平台
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public ExternalTransactionType Type { get; set; }

        /// <summary>
        /// 赞助金额
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 货币类型
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public CurrencyCode CurrencyCode { get; set; }
    }

#if MVVM_VM
    partial class RankingResponse : ReactiveUI.ReactiveObject
    {

    }
#endif
}
