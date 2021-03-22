using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class GameRelatedPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameRelated;
            protected set { throw new NotImplementedException(); }
        }

    }
}
