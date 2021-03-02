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

    }
}
