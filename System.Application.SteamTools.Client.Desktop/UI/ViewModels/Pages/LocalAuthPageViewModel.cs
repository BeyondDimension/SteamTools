using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class LocalAuthPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.SteamAuth;
            protected set { throw new NotImplementedException(); }
        }

    }
}
