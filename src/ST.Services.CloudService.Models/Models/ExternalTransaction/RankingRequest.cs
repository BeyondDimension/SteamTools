using System;
using System.Collections.Generic;
using System.Text;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
namespace System.Application.Models
{
    [MPObj]
    public class RankingRequest
    {
        /// <summary>
        /// 排序类型是否是金额
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public bool IsAmount { get; set; } = true;

        /// <summary>
        /// 时间范围
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public DateTimeOffset[]? TimeRange { get; set; }

        /// <summary>
        /// 捐助平台
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public ExternalTransactionType? Type { get; set; }

        /// <summary>
        /// 货币类型
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public CampaignCurrencyEnum CampaignCurrency { get; set; } = CampaignCurrencyEnum.RMB;
    }
}
