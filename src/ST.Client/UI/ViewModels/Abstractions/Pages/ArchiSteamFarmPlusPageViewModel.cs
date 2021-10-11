using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    public partial class ArchiSteamFarmPlusPageViewModel
    {
        protected readonly IArchiSteamFarmService asfSerivce = IArchiSteamFarmService.Instance;

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel);

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
            if (!ASFService.Current.IsASFRuning)
            {
                ASFService.Current.InitASF();
            }
            else
            {
                ASFService.Current.StopASF();
            }
        }

        public void ShowAddBotWindow()
        {
            IWindowManager.Instance.Show(CustomWindow.ASF_AddBot, resizeMode: ResizeMode.CanResize);
        }
    }
}
