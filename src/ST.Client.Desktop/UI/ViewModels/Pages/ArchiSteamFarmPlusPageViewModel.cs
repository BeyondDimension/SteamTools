using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        readonly IArchiSteamFarmService asfSerivce = DI.Get<IArchiSteamFarmService>();

        public override string Name
        {
            get => AppResources.ArchiSteamFarmPlus;
            protected set { throw new NotImplementedException(); }
        }

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel).Replace("ViewModel", "Svg");


            ASFService.Current.SteamBotsSourceList
                      .Connect()
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Sort(SortExpressionComparer<Bot>.Descending(x => x.BotName))
                      .Bind(out _SteamBots)
                      .Subscribe();
        }

        public string? WebUrl => asfSerivce.GetIPCUrl();

        /// <summary>
        /// ASF bots
        /// </summary>
        private ReadOnlyObservableCollection<Bot> _SteamBots;
        public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

        public void RunOrStopASF()
        {
            IArchiSteamFarmService.Instance.Start();
        }

    }
}
