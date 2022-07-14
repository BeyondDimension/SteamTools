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

        private SourceList<AdvertisementDTO> AdvertisementsSource { get; }

        private readonly ReadOnlyObservableCollection<AdvertisementDTO> _Advertisements;

        public ReadOnlyObservableCollection<AdvertisementDTO> Advertisements => _Advertisements;

        [Reactive]
        public bool IsInitialized { get; set; }

        [Reactive]
        public bool IsShowAdvertise { get; set; } = true;

        private AdvertiseService()
        {
            mCurrent = this;

            AdvertisementsSource = new SourceList<AdvertisementDTO>();

            AdvertisementsSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<AdvertisementDTO>.Ascending(x => x.Order))
                .Bind(out _Advertisements)
                .Subscribe();

            InitAdvertise();

            UserService.Current.WhenValueChanged(x => x.User, false)
                    .Subscribe(_ => CheckShow());

            AdvertiseService.Current.WhenValueChanged(x => x.Advertisements, false)
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
            var result = await client.All(EAdvertisementType.Banner);

            if (result.IsSuccess && result.Content != null)
            {
                AdvertisementsSource.Clear();
                AdvertisementsSource.AddRange(result.Content);
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

            if (AdvertiseService.Current.IsInitialized && !AdvertiseService.Current.Advertisements.Any_Nullable())
            {
                IsShowAdvertise = false;
                return;
            }

            IsShowAdvertise = true;
        }
    }
}
