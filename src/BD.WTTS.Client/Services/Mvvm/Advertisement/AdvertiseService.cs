// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 广告服务
/// </summary>
public sealed class AdvertiseService : ReactiveObject
{
    static AdvertiseService? mCurrent;

    public static AdvertiseService Current => mCurrent ?? new();

    public SourceCache<AdvertisementDTO, Guid> AdvertisementsSource { get; }

    readonly ReadOnlyObservableCollection<AdvertisementDTO>? _HorizontalBannerAdvertisements;

    public ReadOnlyObservableCollection<AdvertisementDTO>? HorizontalBannerAdvertisements => _HorizontalBannerAdvertisements;

    readonly ReadOnlyObservableCollection<AdvertisementDTO>? _VerticalBannerAdvertisements;

    public ReadOnlyObservableCollection<AdvertisementDTO>? VerticalBannerAdvertisements => _VerticalBannerAdvertisements;

    [Reactive]
    public bool IsInitialized { get; set; }

    [Reactive]
    public bool IsShowAdvertise { get; set; } = true;

    AdvertiseService()
    {
        mCurrent = this;

        AdvertisementsSource = new SourceCache<AdvertisementDTO, Guid>(x => x.Id);

        AdvertisementsSource
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(x => x.Standard == AdvertisementStandard.Horizontal)
            .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
            .Bind(out _HorizontalBannerAdvertisements)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HorizontalBannerAdvertisements)));

        AdvertisementsSource
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(x => x.Standard == AdvertisementStandard.Vertical)
            .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
            .Bind(out _VerticalBannerAdvertisements)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(VerticalBannerAdvertisements)));

        UserService.Current.WhenValueChanged(x => x.User, false)
                .Subscribe(_ => CheckShow());

        AdvertisementsSource.CountChanged
                .Subscribe(_ => CheckShow());

        UISettings.IsShowAdvertise.Subscribe(_ => CheckShow());

        //this.WhenValueChanged(x => x.IsShowAdvertise, false)
        //    .Subscribe(async x =>
        //    {
        //        if (x)
        //        {
        //            await RefrshAdvertise();
        //        }
        //        else
        //        {
        //            AdvertisementsSource.Clear();
        //        }
        //    });
    }

    public async void InitAdvertise()
    {
        if (IsInitialized == false)
        {
            await RefrshAdvertiseAsync();
            IsInitialized = true;
            CheckShow();
        }
    }

    public async Task RefrshAdvertiseAsync()
    {
        var client = IMicroServiceClient.Instance.Advertisement;
        var result = await client.All();

        if (result.IsSuccess && result.Content != null)
        {
            AdvertisementsSource.Clear();
            AdvertisementsSource.AddOrUpdate(result.Content);
        }
        else
        {
            Log.Error(nameof(InitAdvertise), result.Message);
        }
    }

    public async void ClickAdvertisement(AdvertisementDTO dto)
    {
        if (dto != null)
        {
            await Browser2.OpenAsync(dto.Url);
        }
    }

    void CheckShow()
    {
        if (!IsInitialized)
        {
            IsShowAdvertise = false;
            return;
        }

        if (UserService.Current.User != null && UserService.Current.User.UserType == UserType.Sponsor)
        {
            if (!UISettings.IsShowAdvertise.Value)
            {
                IsShowAdvertise = false;
                return;
            }
        }

        //if (IsInitialized && !AdvertisementsSource.Items.Any_Nullable())
        //{
        //    IsShowAdvertise = false;
        //    return;
        //}

        IsShowAdvertise = true;
    }
}