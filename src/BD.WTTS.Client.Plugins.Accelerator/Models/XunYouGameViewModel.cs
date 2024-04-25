namespace BD.WTTS.Models;

[MP2Obj(MP2SerializeLayout.Explicit)]
public partial class XunYouGameViewModel : ReactiveSerializationObject, IReactiveObject
{
    /// <inheritdoc cref="XunYouGame"/>
    [MP2Key(0), JsonPropertyOrder(0)]
    public XunYouGame Model { get; set; } = model;

    /// <summary>
    /// PicInfo
    /// </summary>
    [Reactive]
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public XunYouPicInfo? PicInfo { get; set; }

    /// <summary>
    /// 加速延迟
    /// </summary>
    [Reactive]
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public uint PingValue { get; set; }

    /// <summary>
    /// 加速延迟是否无效
    /// </summary>
    [Reactive]
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public bool PingValueIsInvalid { get; set; }

    /// <summary>
    /// 设置加速延迟
    /// </summary>
    /// <param name="value"></param>
    public void SetPingValue(uint value)
    {
        PingValueIsInvalid = value >= 1000;
        if (PingValueIsInvalid) // 延迟大于等于1000，就显示--
            PingValue = default;
        else
            PingValue = value;
    }

    /// <summary>
    /// 加速丢包率
    /// </summary>
    [Reactive]
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public float PingSpeedLoss { get; set; }

    XunYouGameInfo? _GameInfo;

    /// <summary>
    /// 游戏信息
    /// </summary>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public XunYouGameInfo? GameInfo
    {
        get => _GameInfo;
        set
        {
            _GameInfo = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(ShowStart));
        }
    }

    /// <inheritdoc cref="XunYouGameInfo.ShowStart"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public bool ShowStart => IsAccelerated && (GameInfo != null && GameInfo.ShowStart);

    /// <summary>
    /// 是否正在加速等待
    /// </summary>
    [Reactive]
    [MP2Ignore, SystemTextJsonIgnore]
    public bool IsAccelerating { get; set; }

    /// <summary>
    /// 加速进度
    /// </summary>
    [Reactive]
    [MP2Ignore, SystemTextJsonIgnore]
    public int AcceleratingProgress { get; set; }

    bool _IsAccelerated;

    /// <summary>
    /// 是否已加速
    /// </summary>
    [MP2Ignore, SystemTextJsonIgnore]
    public bool IsAccelerated
    {
        get => _IsAccelerated;
        set
        {
            if (_IsAccelerated != value)
            {
                _IsAccelerated = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ShowStart));
            }
        }
    }

    /// <summary>
    /// 是否正在停止加速等待
    /// </summary>
    [Reactive]
    [MP2Ignore, SystemTextJsonIgnore]
    public bool IsStopAccelerating { get; set; }

    /// <summary>
    /// 最后加速时间
    /// </summary>
    [Reactive]
    [MP2Key(1), JsonPropertyOrder(1)]

    public DateTimeOffset? LastAccelerateTime { get; set; }

    /// <summary>
    /// 加速的区服
    /// </summary>
    [Reactive]
    [MP2Key(2), JsonPropertyOrder(2)]
    public XunYouGameArea? SelectedArea { get; set; }

    /// <summary>
    /// 加速的区服服务器
    /// </summary>
    [Reactive]
    [MP2Key(3), JsonPropertyOrder(3)]
    public XunYouGameServer? SelectedServer { get; set; }
}

/// <summary>
/// <see cref="XunYouGame"/> 的视图模型
/// </summary>
sealed partial class XunYouGameViewModel(XunYouGame model) : ReactiveSerializationObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XunYouGameViewModel"/> class.
    /// </summary>
    [MP2Constructor, SystemTextJsonConstructor]
    public XunYouGameViewModel() : this(new())
    {
    }

    /// <summary>
    /// ViewModel => Model
    /// </summary>
    /// <param name="vm"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator XunYouGame(XunYouGameViewModel vm) => vm.Model;

    /// <summary>
    /// Model => ViewModel
    /// </summary>
    /// <param name="model"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator XunYouGameViewModel(XunYouGame model) => new(model);

    /// <inheritdoc cref="XunYouGame.Id"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public int Id { get => Model.Id; set => Model.Id = value; }

    /// <inheritdoc cref="XunYouGame.Name"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? Name { get => Model.Name; set => Model.Name = value; }

    /// <inheritdoc cref="XunYouGame.IconUrl"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? IconUrl { get => Model.IconUrl; set => Model.IconUrl = value; }

    /// <inheritdoc cref="XunYouGame.PicUrl"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? PicUrl { get => Model.PicUrl; set => Model.PicUrl = value; }

    /// <inheritdoc cref="XunYouGame.PicMD5"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? PicMD5 { get => Model.PicMD5; set => Model.PicMD5 = value; }

    /// <inheritdoc cref="XunYouGame.LogoUrl"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? LogoUrl { get => Model.LogoUrl; set => Model.LogoUrl = value; }

    /// <inheritdoc cref="XunYouGame.LogoMD5"/>
    [XmlIgnore, IgnoreDataMember, SystemTextJsonIgnore, NewtonsoftJsonIgnore, MPIgnore, MP2Ignore]
    public string? LogoMD5 { get => Model.LogoMD5; set => Model.LogoMD5 = value; }

    /// <inheritdoc />
    public override string ToString() => Model.ToString()!;
}

sealed class LastAccelerateTimeComparer : IComparer<XunYouGameViewModel>
{
    /// <inheritdoc/>
    public int Compare(XunYouGameViewModel? x, XunYouGameViewModel? y)
    {
        if (x == null || y == null)
            return 0;
        if (x.IsAccelerating)
        {
            return -1;
        }
        else if (y.IsAccelerating)
        {
            return 1;
        }
        if (x.IsAccelerated)
        {
            return -1;
        }
        else if (y.IsAccelerated)
        {
            return 1;
        }
        else if (x.LastAccelerateTime.HasValue && y.LastAccelerateTime.HasValue)
        {
            return y.LastAccelerateTime.Value.CompareTo(x.LastAccelerateTime.Value);
        }
        else if (x.LastAccelerateTime.HasValue)
        {
            return -1;
        }
        else if (y.LastAccelerateTime.HasValue)
        {
            return 1;
        }
        else
        {
            var mygamekeys = GameAcceleratorSettings.MyGames.Value?.Keys;
            if (mygamekeys != null)
            {
                var x2 = mygamekeys.IndexOf(x.Id);
                var y2 = mygamekeys.IndexOf(y.Id);
                if (x2 < y2)
                {
                    return -1;
                }
                else if (x2 > y2)
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}