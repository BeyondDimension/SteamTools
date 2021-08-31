using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class SteamIdlePageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.IdleCard;
            protected set { throw new NotImplementedException(); }
        }

        public SteamIdlePageViewModel()
        {
            IconKey = nameof(SteamIdlePageViewModel).Replace("ViewModel", "Svg");
        }
    }
}
