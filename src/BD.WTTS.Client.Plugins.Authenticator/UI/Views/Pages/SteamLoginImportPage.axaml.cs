using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using BD.WTTS.UI.Views.Controls;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamLoginImportPage : ReactiveUserControl<SteamLoginImportPageViewModel>
{
    public SteamLoginImportPage()
    {
        InitializeComponent();
        DataContext ??= new SteamLoginImportPageViewModel();
        //PasswordText.KeyUp += PasswordTextOnKeyUp;
        //EmailAuthText.KeyUp += PasswordTextOnKeyUp;
        //PhoneNumberText.KeyUp += PasswordTextOnKeyUp;
        //PhoneCodeText.KeyUp += PasswordTextOnKeyUp;
    }

    void PasswordTextOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.Return) return;
        if (DataContext is not SteamLoginImportPageViewModel vm) return;
        _ = vm.LoginSteamImport();
        e.Handled = true;
    }
}
