using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class SettingsPage : PageBase<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = IViewModelManager.Instance.Get<SettingsPageViewModel>();

        Title = Strings.Settings;
    }
}
