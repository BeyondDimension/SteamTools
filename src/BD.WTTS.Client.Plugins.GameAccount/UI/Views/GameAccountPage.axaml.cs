using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views;
public partial class GameAccountPage : ReactiveUserControl<GameAccountPageViewModel>
{
    public GameAccountPage()
    {
        InitializeComponent();
        DataContext = new GameAccountPageViewModel();
    }
}
