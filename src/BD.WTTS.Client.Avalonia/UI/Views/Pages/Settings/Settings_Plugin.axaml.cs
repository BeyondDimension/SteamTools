using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class Settings_Plugin : ReactiveUserControl<SettingsPageViewModel>
{
    public Settings_Plugin()
    {
        InitializeComponent();
        DataContext = new SettingsPageViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ViewModel?.LoadPlugins();
    }
}
