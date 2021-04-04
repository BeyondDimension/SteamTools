using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }


    }
}
