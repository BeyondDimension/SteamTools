// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 广告服务
/// </summary>
public sealed class AdvertiseService : ReactiveObject
{
    static readonly Lazy<AdvertiseService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static AdvertiseService Current => mCurrent.Value;

    [Reactive]
    public ObservableCollection<AdvertisementDTO>? HorizontalBannerAdvertisements { get; set; }

    [Reactive]
    public ObservableCollection<AdvertisementDTO>? VerticalBannerAdvertisements { get; set; }

    [Reactive]
    public bool IsInitialized { get; set; }

    [Reactive]
    public bool IsShowAdvertise { get; set; } = true;

    public ICommand ClickAdvertisementCommand { get; }

    AdvertiseService()
    {
        //AdvertisementsSource
        //    .Connect()
        //    .ObserveOn(RxApp.MainThreadScheduler)
        //    .Filter(x => x.Standard == AdvertisementOrientation.Horizontal)
        //    .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
        //    .Bind(out _HorizontalBannerAdvertisements)
        //    .Subscribe(_ => this.RaisePropertyChanged(nameof(HorizontalBannerAdvertisements)));

        //AdvertisementsSource
        //    .Connect()
        //    .ObserveOn(RxApp.MainThreadScheduler)
        //    .Filter(x => x.Standard == AdvertisementOrientation.Vertical)
        //    .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
        //    .Bind(out _VerticalBannerAdvertisements)
        //    .Subscribe(_ => this.RaisePropertyChanged(nameof(VerticalBannerAdvertisements)));

        ClickAdvertisementCommand = ReactiveCommand.Create<AdvertisementDTO>(ClickAdvertisement);

        UserService.Current.WhenValueChanged(x => x.User, false)
                .Subscribe(_ => CheckShow());

        //AdvertisementsSource.CountChanged
        //        .Subscribe(_ => CheckShow());

        UISettings.IsShowAdvertisement.Subscribe(_ => CheckShow());

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
        var result = await client.All(AdvertisementType.Banner);

        if (result.IsSuccess && result.Content != null)
        {
            HorizontalBannerAdvertisements = null;
            HorizontalBannerAdvertisements = new ObservableCollection<AdvertisementDTO>(result.Content.Where(x => x.Standard == AdvertisementOrientation.Horizontal).OrderBy(x => x.Order));

            VerticalBannerAdvertisements = null;
            VerticalBannerAdvertisements = new ObservableCollection<AdvertisementDTO>(result.Content.Where(x => x.Standard == AdvertisementOrientation.Vertical).OrderBy(x => x.Order));
        }
        else
        {
            Log.Error(nameof(InitAdvertise), result.Message);
        }
    }

    static async void ClickAdvertisement(AdvertisementDTO? dto)
    {
        if (dto != null)
        {
            //IApplication.Instance.OpenBrowserCommandCore(dto.Url);
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
            if (!UISettings.IsShowAdvertisement.Value)
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