using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    int lastIndex = -1;

    public MainFramePage()
    {
        InitializeComponent();

        Tabs.Items.Add(new TabStripItem { Content = "网络加速", Tag = typeof(AcceleratorPage) });
        Tabs.Items.Add(new TabStripItem { Content = "脚本配置", Tag = typeof(ScriptPage) });

        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        Tabs_SelectionChanged(null, null);
    }

    private void Tabs_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs? e)
    {
        if (Tabs.SelectedItem is TabStripItem tab && tab.Tag is Type t)
        {
            InnerNavFrame.Navigate(t, null, new SlideNavigationTransitionInfo
            {
                Effect = GetEffect(lastIndex, Tabs.SelectedIndex),
                FromHorizontalOffset = 70,
            });

            lastIndex = Tabs.SelectedIndex;
        }
    }

    private SlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
    {
        if (oldIndex < 0)
            return SlideNavigationTransitionEffect.FromBottom;

        if (oldIndex > index)
            return SlideNavigationTransitionEffect.FromRight;
        else
            return SlideNavigationTransitionEffect.FromLeft;
    }
}
