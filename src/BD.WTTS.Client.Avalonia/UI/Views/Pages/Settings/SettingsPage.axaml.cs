namespace BD.WTTS.UI.Views.Pages;

public partial class SettingsPage : PageBase<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        this.SetViewModel<SettingsPageViewModel>();
        SettingsScrollTab.Tapped += (_, _) => ScrollToSelectedSection();
        SettingsScrollTab.KeyUp += (_, e) =>
        {
            if (e.Key is Key.Left or Key.Right or Key.Home or Key.End or Key.Enter or Key.Space)
                ScrollToSelectedSection();
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (PageScroller != null)
            PageScroller.ScrollChanged -= SettingsScrollViewer_ScrollChanged;
        base.OnApplyTemplate(e);
        if (PageScroller != null)
            PageScroller.ScrollChanged += SettingsScrollViewer_ScrollChanged;
    }

    private void ScrollToSelectedSection()
    {
        var index = SettingsScrollTab.SelectedIndex;
        if (index >= 0 &&
            PageScroller is { Content: Visual content } scroller &&
            SettingsSections.ContainerFromIndex(index)?.TranslatePoint(default, content) is { } top)
            scroller.Offset = scroller.Offset.WithY(top.Y);
    }

    private void SettingsScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer { Content: Visual content } scroller)
            return;

        for (var index = 0; index < SettingsSections.ItemCount; index++)
        {
            if (SettingsSections.ContainerFromIndex(index) is { IsEffectivelyVisible: true } section &&
                section.TranslatePoint(new Point(0, section.Bounds.Height), content) is { } bottom &&
                bottom.Y > scroller.Offset.Y)
            {
                SettingsScrollTab.SelectedIndex = index;
                return;
            }
        }
    }
}
