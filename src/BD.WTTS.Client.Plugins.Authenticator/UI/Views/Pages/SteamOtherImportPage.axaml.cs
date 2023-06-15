using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamOtherImportPage : UserControl
{
    public SteamOtherImportPage()
    {
        InitializeComponent();
        DataContext = new SteamOtherImportViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}