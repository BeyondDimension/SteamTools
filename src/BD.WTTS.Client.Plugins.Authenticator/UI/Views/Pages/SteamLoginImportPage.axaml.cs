using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using BD.WTTS.UI.Views.Controls;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamLoginImportPage : ReactiveUserControl<SteamLoginImportViewModel>
{
    public SteamLoginImportPage()
    {
        InitializeComponent();
        DataContext ??= new SteamLoginImportViewModel();
        //PasswordText.KeyUp += PasswordTextOnKeyUp;
        //EmailAuthText.KeyUp += PasswordTextOnKeyUp;
        //PhoneNumberText.KeyUp += PasswordTextOnKeyUp;
        //PhoneCodeText.KeyUp += PasswordTextOnKeyUp;
    }

    void PasswordTextOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.Return) return;
        if (DataContext is not SteamLoginImportViewModel vm) return;
        _ = vm.LoginSteamImport();
        e.Handled = true;
    }

    // public SteamLoginImportPage(string? password)
    // {
    //     InitializeComponent();
    //     DataContext = new SteamLoginImportViewModel(password);
    // }

    // void Stepper_OnBacking(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnBacking(sender, args);
    // }
    //
    // void Stepper_OnNexting(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnNexting(sender, args);
    // }
    //
    // void Stepper_OnSkiping(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnSkiping(sender, args);
    // }
}
