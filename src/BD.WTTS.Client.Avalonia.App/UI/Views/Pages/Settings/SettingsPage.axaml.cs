using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class SettingsPage : ReactiveUserControl<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsPageViewModel();
    }
}
