using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class OtherPlatformPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.OtherGamePlaform;
            protected set { throw new NotImplementedException(); }
        }

        public OtherPlatformPageViewModel() 
        {
            IconKey = nameof(OtherPlatformPageViewModel);
        }
    }
}
