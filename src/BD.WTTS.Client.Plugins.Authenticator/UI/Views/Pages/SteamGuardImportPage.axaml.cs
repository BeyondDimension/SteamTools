using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamGuardImportPage : ReactiveUserControl<SteamGuardImportPageViewModel>
{
    public SteamGuardImportPage()
    {
        InitializeComponent();
        DataContext ??= new SteamGuardImportPageViewModel();
    }
}