using Avalonia.Input;
using ReactiveUI.Avalonia;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamLoginImportPage : ReactiveUserControl<SteamLoginImportPageViewModel>
{
    public SteamLoginImportPage()
    {
        InitializeComponent();
        DataContext ??= new SteamLoginImportPageViewModel();
        PhoneCodeText.KeyUp += PhoneCodeTextOnKeyUp;
        PhoneCodeText.TextInput += PhoneCodeTextOnTextInput;
    }

    void PhoneCodeTextOnTextInput(object? sender, TextInputEventArgs e)
    {
        e.Text = e.Text?.ToUpper();
    }

    void PhoneCodeTextOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Return)
        {
            if (DataContext is not SteamLoginImportPageViewModel vm) return;
            _ = vm.LoginSteamImport();
            e.Handled = true;
        }
    }
}
