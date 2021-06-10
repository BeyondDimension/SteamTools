using ReactiveUI;
using System.Application.Services.Implementation;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.ArchiSteamFarmPlus;
            protected set { throw new NotImplementedException(); }
        }

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel).Replace("ViewModel", "Svg");

            WebUrl = IArchiSteamFarmService.Instance.GetArchiSteamFarmIPCUrl();
        }

        private string? _WebUrl;
        public string? WebUrl
        {
            get => _WebUrl;
            set => this.RaiseAndSetIfChanged(ref _WebUrl, value);
        }


    }
}
