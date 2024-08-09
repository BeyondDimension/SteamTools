namespace BD.WTTS.UI.Views.Pages;

public partial class BorderlessGamePage : PageBase<BorderlessGamePageViewModel>
{
    public BorderlessGamePage()
    {
        InitializeComponent();
        DataContext ??= new BorderlessGamePageViewModel();

        move.PointerPressed += (_, _) => move.IsVisible = false;
        move.PointerReleased += (_, _) => move.IsVisible = true;

        const int threeLineButtonWidth = 554;
        const int fourLineButtonWidth = 418;
        Observable.FromEventPattern<Avalonia.Controls.SizeChangedEventArgs>(ButtonGroup, nameof(ButtonGroup.SizeChanged))
                    .Select(eventPattern => eventPattern.EventArgs.NewSize.Width)
                    .Select(width => width < fourLineButtonWidth ? 200 : width < threeLineButtonWidth ? 150 : 100)
                    .DistinctUntilChanged()
                    .Subscribe(newHeight => ButtonGroup.Height = newHeight);
    }
}
