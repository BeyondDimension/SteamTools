// https://bcssstorage.blob.core.chinacloudapi.cn/docs/CCS/%E8%93%9D%E4%BA%91%E7%94%A8%E6%88%B7%E8%BF%9E%E6%8E%A5%E6%9C%8D%E5%8A%A1%E6%8A%80%E6%9C%AF%E6%96%87%E6%A1%A3(%E7%9F%AD%E4%BF%A1%2B%E7%99%BB%E5%BD%95).pdf

using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models._21VianetBlueCloud
{
    public class SendSms21VianetBlueCloudResult : JsonModel<SendSms21VianetBlueCloudResult>
    {
        /// <summary>
        /// 短信发送的 ID，用于后续查询
        /// </summary>
        [N_JsonProperty("messageId")]
        [S_JsonProperty("messageId")]
        public string? MessageId { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [N_JsonProperty("sendTime")]
        [S_JsonProperty("sendTime")]
        public string? SendTime { get; set; }
    }
}