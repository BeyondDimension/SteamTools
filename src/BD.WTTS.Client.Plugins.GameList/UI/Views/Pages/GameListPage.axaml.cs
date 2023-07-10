using Avalonia.Layout;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameListPage : PageBase<GameListPageViewModel>
{
    public GameListPage()
    {
        InitializeComponent();

        DataContext = new GameListPageViewModel();
    }
}
