using Avalonia.Controls;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views;

public partial class ImportControl : UserControl
{
    public ImportControl()
    {
        InitializeComponent();
        DataContext = new SteamImportAuthenticator();
    }

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
