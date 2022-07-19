#if MVVM_VM
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Application.Services;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
#endif
using System.Threading.Tasks;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    [MPObj]
    public partial class NoticeDTO
#if MVVM_VM
        : ReactiveObject
#endif
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public Guid Id { get; set; }

        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public Guid? ContentId { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public string Picture { get; set; } = string.Empty;

#if MVVM_VM
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public Task<string?>? PictureStream => !string.IsNullOrEmpty(Picture) ? IHttpService.Instance.GetImageAsync(Picture, "NoticePicture"/*ImageChannelType.NoticePicture*/) : null;
#endif

        /// <summary>
        /// 标题
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 作者
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 简介
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public string Introduction { get; set; } = string.Empty;

        /// <summary>
        /// 消息添加时间
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// 消息启用时间（预约添加）
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        public DateTimeOffset EnableTime { get; set; }

        /// <summary>
        /// 是否浏览器打开
        /// </summary>
        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public bool IsOpenBrowser { get; set; }

        /// <summary>
        /// 提醒过期时间 （默认3天）
        /// </summary>
        [MPKey(9)]
        [N_JsonProperty("9")]
        [S_JsonProperty("9")]
        public DateTimeOffset OverdueTime { get; set; }

#if MVVM_VM
        /// <summary>
        /// 是否过期
        /// </summary> 
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool Overdue => DateTimeOffset.Now > OverdueTime;
#endif

        /// <summary>
        /// 通知渠道
        /// </summary>
        [MPKey(10)]
        [N_JsonProperty("10")]
        [S_JsonProperty("10")]
        public NotificationType Type { get; set; } = NotificationType.Announcement;

        [MPKey(11)]
        [N_JsonProperty("11")]
        [S_JsonProperty("11")]
        public string Url { get; set; } = string.Empty;

        [MPKey(12)]
        [N_JsonProperty("12")]
        [S_JsonProperty("12")]
        public string? Context { get; set; } = string.Empty;
    }
}
