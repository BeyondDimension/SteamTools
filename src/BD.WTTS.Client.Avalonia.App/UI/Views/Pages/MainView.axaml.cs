using FluentAvalonia.Core;

namespace BD.WTTS.UI.Views.Pages;

public sealed partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    public MainView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, MainWindow.GetMainWinodwViewModel());
#endif

        NavView.SelectionChanged += (_, e) =>
        {
            if (e.SelectedItem is MenuTabItemViewModel menu && menu.PageType != null)
            {
                FrameView?.Navigate(menu.PageType);
                return;
            }
            FrameView?.Navigate(typeof(ErrorPage));
        };
    }
}
