#if MVVM_VM
using ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// 加速项目组
    /// </summary>
    [MPObj]
    public class AccelerateProjectGroupDTO
#if MVVM_VM
        : ReactiveObject
#endif
    {

#if MVVM_VM
        public AccelerateProjectGroupDTO()
        {
            this.WhenAnyValue(x => x.Items)
                  .DistinctUntilChanged()
                  .Subscribe(s =>
                  {
                      if (s.Any_Nullable())
                          ObservableItems = new ObservableCollection<AccelerateProjectDTO>(s);
                  });
            this.WhenAnyValue(v => v.ObservableItems)
                  .Subscribe(items => items?
                        .ToObservableChangeSet()
                        .AutoRefresh(x => x.Enable)
                        .ToCollection()
                        .Select<IReadOnlyCollection<AccelerateProjectDTO>, bool?>(x =>
                        {
                            var count = x.Count(s => s.Enable);
                            if (x == null || count == 0)
                                return false;
                            if (count == x.Count)
                                return true;
                            return null;
                        })
                        .Subscribe(s =>
                        {
                            if (ThreeStateEnable != s)
                            {
                                mThreeStateEnable = s;
                                this.RaisePropertyChanged(nameof(ThreeStateEnable));
                            }
                        }));
        }

        private ObservableCollection<AccelerateProjectDTO>? _ObservableItems;
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public ObservableCollection<AccelerateProjectDTO>? ObservableItems
        {
            get => _ObservableItems;
            set => this.RaiseAndSetIfChanged(ref _ObservableItems, value);
        }

        /// <summary>
        /// 显示图片，使用 System.Application.ImageUrlHelper.GetImageApiUrlById(Guid) 转换为Url
        /// </summary>
        private Task<string?>? _ImageStream;
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public Task<string?>? ImageStream
        {
            get => _ImageStream;
            set => this.RaiseAndSetIfChanged(ref _ImageStream, value);
        }
#endif

        /// <summary>
        /// 显示名称
        /// </summary>
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        [NotNull, DisallowNull] // C# 8 not null
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// 当前组中所有的加速项目集合
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        [NotNull, DisallowNull] // C# 8 not null
        public List<AccelerateProjectDTO>? Items
#if MVVM_VM
        {
            get => mItems;
            set => this.RaiseAndSetIfChanged(ref mItems, value);
        }
        List<AccelerateProjectDTO>? mItems;
#else
        { get; set; } = new();
#endif

        /// <summary>
        /// 显示图片，使用 System.Application.ImageUrlHelper.GetImageApiUrlById(Guid) 转换为Url
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public Guid ImageId { get; set; }

        /// <summary>
        /// 是否默认启用
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public bool Enable
#if MVVM_VM
        {
            get => mEnable;
            set => this.RaiseAndSetIfChanged(ref mEnable, value);
        }
        bool mEnable;
#else
        { get; set; }
#endif

        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public int Order { get; set; }

#if MVVM_VM
        /// <summary>
        /// 是否有子项目选中的第三状态（仅客户端）
        /// </summary>
        [MPIgnore]
        [N_JsonIgnore]
        [S_JsonIgnore]
        public bool? ThreeStateEnable
        {
            get => mThreeStateEnable;
            set
            {
                mThreeStateEnable = value;
                Enable = mThreeStateEnable == true;
                foreach (var item in this.Items)
                {
                    item.Enable = Enable;
                }
                this.RaisePropertyChanged();
            }
        }
        bool? mThreeStateEnable;
#endif
    }
}