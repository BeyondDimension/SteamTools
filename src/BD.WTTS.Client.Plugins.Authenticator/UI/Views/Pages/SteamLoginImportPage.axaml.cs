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
        PasswordText.KeyUp += PasswordTextOnKeyUp;
        EmailAuthText.KeyUp += PasswordTextOnKeyUp;
        //PhoneNumberText.KeyUp += PasswordTextOnKeyUp;
        PhoneCodeText.KeyUp += PasswordTextOnKeyUp;
        EmailAuthText.TextInput += EmailAuthText_TextInput;
        PhoneCodeText.TextInput += EmailAuthText_TextInput;
    }

    void EmailAuthText_TextInput(object? sender, TextInputEventArgs e)
    {
        e.Text = e.Text?.ToUpper();
    }

    void PasswordTextOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Return)
        {
            if (DataContext is not SteamLoginImportPageViewModel vm) return;
            _ = vm.LoginSteamImport();
            e.Handled = true;
        }
    }
}