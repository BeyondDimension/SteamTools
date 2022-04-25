using System.Collections.Generic;
#if MVVM_VM
using ReactiveUI;
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
    public class NoticeTypeDTO
#if MVVM_VM
        : ReactiveObject
#endif
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public int Order { get; set; }

#if MVVM_VM
        /// <summary>
        /// 当前组中所有的加速项目集合
        /// </summary> 
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public int Index
        {
            get => _Index;
            set => this.RaiseAndSetIfChanged(ref _Index, value);
        }

        int _Index = 1;

        /// <summary>
        /// 当前组中所有的加速项目集合
        /// </summary> 
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public PagedModel<NoticeDTO>? Items
        {
            get => mItems;
            set => this.RaiseAndSetIfChanged(ref mItems, value);
        }

        PagedModel<NoticeDTO>? mItems;

#endif
    }
}
