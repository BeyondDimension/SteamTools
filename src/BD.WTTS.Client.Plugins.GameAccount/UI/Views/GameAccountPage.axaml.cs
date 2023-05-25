using Avalonia.Controls;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views;
public partial class GameAccountPage : ReactiveUserControl<GameAccountPageViewModel>
{
    public GameAccountPage()
    {
        InitializeComponent();
        DataContext = new GameAccountPageViewModel();
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is PlatformAccount platform)
        {
            if (platform.FullName == "Steam")
            {
                Toast.Show(string.Format("不允许删除 {0} 平台", platform.FullName));
                return;
            }
            ViewModel?.AddGamePlatforms?.Add(platform);
            ViewModel?.GamePlatforms?.Remove(platform);
        }
    }
}
