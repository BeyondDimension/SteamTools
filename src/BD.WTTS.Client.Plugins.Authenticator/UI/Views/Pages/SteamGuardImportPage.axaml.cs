using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Reactive;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamGuardImportPage : ReactiveUserControl<SteamGuardImportPageViewModel>
{
    public SteamGuardImportPage()
    {
        InitializeComponent();
        DataContext ??= new SteamGuardImportPageViewModel();
    }
}