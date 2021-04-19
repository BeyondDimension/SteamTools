using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class AboutPageViewModel : TabItemViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public override string Name
        {
            get => AppResources.About;
            protected set { throw new NotImplementedException(); }
        }

        public AboutPageViewModel()
        {
            IconKey = nameof(AboutPageViewModel).Replace("ViewModel", "Svg");
        }
    }
}
