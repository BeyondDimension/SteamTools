using Avalonia.Layout;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameListPage : PageBase<GameListPageViewModel>
{
    public GameListPage()
    {
        InitializeComponent();

        DataContext = new GameListPageViewModel();

        Title = Strings.GameList;
        Subtitle = "插件作者: Steam++ 官方";
        Description = "管理库存游戏";
    }
}
