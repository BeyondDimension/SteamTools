using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class GameRelatedPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.GameRelated;

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
