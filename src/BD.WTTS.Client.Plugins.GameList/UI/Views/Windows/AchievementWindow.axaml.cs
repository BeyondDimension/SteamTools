using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Windows;

public partial class AchievementWindow : ReactiveAppWindow<AchievementAppPageViewModel>
{
    public AchievementWindow()
    {
        InitializeComponent();
    }

    public AchievementWindow(int appid) : this()
    {
        DataContext ??= new AchievementAppPageViewModel(appid);
    }
}
