using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class StartPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.Welcome;
            protected set { throw new NotImplementedException(); }
        }

        public StartPageViewModel()
        {
            IconKey = nameof(StartPageViewModel).Replace("ViewModel", "Svg");
        }
    }
}
