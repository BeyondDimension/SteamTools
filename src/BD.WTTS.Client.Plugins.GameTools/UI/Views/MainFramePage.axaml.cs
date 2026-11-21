using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    public MainFramePage()
    {
        InitializeComponent();

        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Tabs.SelectedItem is null)
            Tabs.SelectedItem = BorderlessTab;
        else
            ShowSelectedPage();
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && e.Source == Tabs)
            ShowSelectedPage();
    }

    private void ShowSelectedPage()
    {
        if (ReferenceEquals(Tabs.SelectedItem, BorderlessTab) && InnerNavFrame.CurrentSourcePageType != typeof(BorderlessGamePage))
            InnerNavFrame.Navigate(typeof(BorderlessGamePage), null, FrameEffect.NoTransition);
    }
}
