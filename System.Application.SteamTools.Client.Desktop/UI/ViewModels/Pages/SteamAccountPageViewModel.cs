using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.UserFastChange;
            protected set { throw new NotImplementedException(); }
        }

    }
}
