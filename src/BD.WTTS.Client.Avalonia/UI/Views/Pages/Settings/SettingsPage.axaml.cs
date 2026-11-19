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
        if (SettingsScrollTab.SelectedIndex >= 0)
            SettingsSections.ScrollIntoView(SettingsScrollTab.SelectedIndex);
    }

    private void SettingsScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scroller)
            return;

        for (var index = 0; index < SettingsSections.ItemCount; index++)
        {
            if (SettingsSections.ContainerFromIndex(index) is { IsVisible: true } section &&
                section.TranslatePoint(new Point(0, section.Bounds.Height), scroller) is { } bottom &&
                bottom.Y > scroller.Padding.Top)
            {
                SettingsScrollTab.SelectedIndex = index;
                return;
            }
        }
    }
}
