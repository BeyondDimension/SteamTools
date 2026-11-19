using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

        Tabs_SelectionChanged(null, null);
    }

    private void Tabs_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs? e)
    {
        if (Tabs.SelectedItem is TabStripItem tab && tab.Tag is Type t)
        {
            if (InnerNavFrame.CurrentSourcePageType == t)
                return;

            InnerNavFrame.Navigate(t, null, FrameEffect.NoTransition);
        }
    }
}
