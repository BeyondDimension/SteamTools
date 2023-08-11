using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    int lastIndex = -1;

    public MainFramePage()
    {
        InitializeComponent();

        //Tabs.Items.Add(new TabStripItem { Content = Strings.CommunityFix, Tag = typeof(AcceleratorPage) });
        //Tabs.Items.Add(new TabStripItem { Content = Strings.ScriptConfig, Tag = typeof(ScriptPage) });

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
            InnerNavFrame.Navigate(t, null, new SlideNavigationTransitionInfo
            {
                Effect = FrameEffect.GetEffect(lastIndex, Tabs.SelectedIndex),
                FromHorizontalOffset = 70,
            });

            lastIndex = Tabs.SelectedIndex;
        }
    }
}
