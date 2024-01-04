using Avalonia;

namespace BD.WTTS.UI.Views.Pages;

public partial class SettingsPage : PageBase<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        this.SetViewModel<SettingsPageViewModel>();

        //SettingsScrollTab.SelectionChanged += SettingsScrollTab_SelectionChanged;

        SettingsScrollTab.Tapped += SettingsScrollTab_Tapped;

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (this.PageScroller != null)
            this.PageScroller.ScrollChanged += SettingsScrollViewer_ScrollChanged;
    }

    private void SettingsScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            foreach (var child in SettingPanel.Children)
            {
                // 获取 ScrollViewer 的可视区域范围
                var viewportBounds = new Rect((Point)scrollViewer.Offset, new Size(scrollViewer.Viewport.Width, scrollViewer.Viewport.Height));

                if (viewportBounds.Intersects(child.Bounds))
                {
                    var nextItem = SettingsScrollTab.Items.FirstOrDefault(x => x is Control control && control.Tag is string tag && tag == child.Name);
                    if (SettingsScrollTab.SelectedItem == nextItem)
                        return;
                    SettingsScrollTab.SelectedItem = nextItem;
                }
            }
        }
    }

    private void SettingsScrollTab_Tapped(object? sender, TappedEventArgs e)
    {
        if (SettingsScrollTab.SelectedItem != null && SettingsScrollTab.SelectedItem is Control c && c.Tag is string controlName)
        {
            var target = this.FindControl<Control>(controlName);

            if (target == null)
                return;

            target.BringIntoView();
        }
    }

    //private void SettingsScrollTab_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    //{
    //    if (SettingsScrollTab.SelectedItem != null && SettingsScrollTab.SelectedItem is Control c && c.Tag is string controlName)
    //    {
    //        var target = this.FindControl<Control>(controlName);

    //        if (target == null)
    //            return;

    //        target.BringIntoView();
    //    }
    //}
}
