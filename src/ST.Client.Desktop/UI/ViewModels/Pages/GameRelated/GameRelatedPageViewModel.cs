using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class GameRelatedPageViewModel : TabItemViewModel, MainWindowViewModel.ITabItemViewModel
    {
        MainWindowViewModel.TabItemId MainWindowViewModel.ITabItemViewModel.Id
            => MainWindowViewModel.TabItemId.GameRelated;

        public override string Name
        {
            get => AppResources.GameRelated;
            protected set { throw new NotImplementedException(); }
        }

        public GameRelatedPageViewModel()
        {
            IconKey = nameof(GameRelatedPageViewModel).Replace("ViewModel", "Svg");
        }
    }
}
