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

namespace System.Application.Services
{
    public class AdvertiseService : ReactiveObject
    {
        static AdvertiseService? mCurrent;

        public static AdvertiseService Current => mCurrent ?? new();

        private SourceList<AdvertisementDTO> AdvertisementsSource { get; }

        private readonly ReadOnlyObservableCollection<AdvertisementDTO> _Advertisements;

        public ReadOnlyObservableCollection<AdvertisementDTO> Advertisements => _Advertisements;

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
        }

        public async void InitAdvertise()
        {
            var client = ICloudServiceClient.Instance.Advertisement;
            var result = await client.All();

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
    }
}
