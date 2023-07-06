using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class SettingsPage : PageBase<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = IViewModelManager.Instance.Get<SettingsPageViewModel>();

        SettingsScrollTab.SelectionChanged += SettingsScrollTab_SelectionChanged;
    }

    private void SettingsScrollTab_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.AddedItems)
        {
            if (item != null && item is Control c && c.Tag is string controlName)
            {
                var target = SettingsScrollViewer.FindControl<Control>(controlName);

                if (target == null)
                    continue;

                target.BringIntoView();
            }
        }
    }
}
