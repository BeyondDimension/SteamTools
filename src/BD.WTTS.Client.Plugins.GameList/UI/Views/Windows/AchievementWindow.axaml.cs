using Avalonia.Controls;
using Avalonia.Controls.Primitives;

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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ViewModel?.RefreshStats_Click();
    }

    void OnAchievementGridVerticalScroll(object? sender, ScrollEventArgs e)
    {
        // New wrapped rows can change the estimated extent without changing the viewport's desired size.
        // Re-measure the grid, not just the rows presenter, so the scrollbar tracks that estimate.
        AchievementGrid.InvalidateMeasure();
    }
}
