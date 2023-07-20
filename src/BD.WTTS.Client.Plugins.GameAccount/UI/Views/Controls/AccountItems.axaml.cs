using Avalonia.Controls;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Controls;

public partial class AccountItems : ReactiveUserControl<PlatformAccount>
{
    public AccountItems()
    {
        InitializeComponent();

        //this.WhenActivated(disposable =>
        //{
        //    ViewModel?.LoadUsers();
        //});

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, new PlatformAccount(ThirdpartyPlatform.Steam)
            {
                FullName = "Steam",
                Icon = "Steam"
            });
#endif
    }

    private void SteamPersonaStateSwapMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is SteamAccount account && item.Tag is PersonaState state)
        {
            account.PersonaState = state;
            ViewModel?.SwapToAccountCommand.Execute(account);
        }
    }
}
