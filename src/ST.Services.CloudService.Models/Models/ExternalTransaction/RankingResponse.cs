using System;
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
    public class RankingResponse
    {
        /// <summary>
        /// 用户nickName
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public string? Name { get; set; } = string.Empty;
        /// <summary>
        /// 头像
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public Guid Avatar { get; set; }
        /// <summary>
        /// 赞助月份
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public int Month { get; set; }
        /// <summary>
        /// 赞助金额
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public decimal Amount { get; set; }
    }
}
