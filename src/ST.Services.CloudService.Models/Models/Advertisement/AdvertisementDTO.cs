#if MVVM_VM
using ReactiveUI;
using System.Collections.Generic;
using static System.Application.Constants.Urls;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
#endif
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public class AdvertisementDTO
#if MVVM_VM
        : ReactiveObject
#endif
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public int Order { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string? Remark { get; set; }
#if MVVM_VM

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string ImageUrl => API_Advertisement_ImageUrl(Id);

        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public string Url => API_Advertisement_JumpUrl(Id);

#endif
    }
}
