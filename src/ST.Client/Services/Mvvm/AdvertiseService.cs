using System;
using System.Collections.Generic;
using System.Text;
using System.Application.Services;
using ReactiveUI;
using System.Application.Models;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;
using System.Application.Settings;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public class AdvertiseService : ReactiveObject
    {
        static AdvertiseService? mCurrent;

        public static AdvertiseService Current => mCurrent ?? new();

        public SourceCache<AdvertisementDTO, Guid> AdvertisementsSource { get; }

        private readonly ReadOnlyObservableCollection<AdvertisementDTO> _HorizontalBannerAdvertisements;

        public ReadOnlyObservableCollection<AdvertisementDTO> HorizontalBannerAdvertisements => _HorizontalBannerAdvertisements;

        private readonly ReadOnlyObservableCollection<AdvertisementDTO> _VerticalBannerAdvertisements;

        public ReadOnlyObservableCollection<AdvertisementDTO> VerticalBannerAdvertisements => _VerticalBannerAdvertisements;

        [Reactive]
        public bool IsInitialized { get; set; }

        [Reactive]
        public bool IsShowAdvertise { get; set; } = true;

        private AdvertiseService()
        {
            mCurrent = this;

            AdvertisementsSource = new SourceCache<AdvertisementDTO, Guid>(x => x.Id);

            AdvertisementsSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Filter(x => x.Standard == EAdvertisementStandard.Horizontal)
                .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
                .Bind(out _HorizontalBannerAdvertisements)
                .Subscribe();

            AdvertisementsSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Filter(x => x.Standard == EAdvertisementStandard.Vertical)
                .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
                .Bind(out _VerticalBannerAdvertisements)
                .Subscribe();

            InitAdvertise();

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
                await RefrshAdvertise();
                IsInitialized = true;
            }
        }

        public async Task RefrshAdvertise()
        {
            var client = ICloudServiceClient.Instance.Advertisement;
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

        public async void ClickAdvertisement(AdvertisementDTO ad)
        {
            if (ad != null)
            {
                await Browser2.OpenAsync(ad.Url);
            }
        }

        private void CheckShow()
        {
            if (UserService.Current.User != null && UserService.Current.User.UserType == UserType.Sponsor)
            {
                if (!UISettings.IsShowAdvertise.Value)
                {
                    IsShowAdvertise = false;
                    return;
                }
            }

            if (AdvertiseService.Current.IsInitialized && !AdvertiseService.Current.AdvertisementsSource.Items.Any_Nullable())
            {
                IsShowAdvertise = false;
                return;
            }

            IsShowAdvertise = true;
        }
    }
}
