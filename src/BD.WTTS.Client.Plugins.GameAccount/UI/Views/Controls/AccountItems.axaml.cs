using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BD.SteamClient.Helpers;
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
            account.WantsOfflineMode = false;
            ViewModel?.SwapToAccountCommand.Execute(account);
        }
    }

    private void SteamOfflineModeStartMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is SteamAccount account)
        {
            account.WantsOfflineMode = true;
            account.SkipOfflineModeWarning = true;
            ViewModel?.SwapToAccountCommand.Execute(account);
        }
    }

    private void CopySteamIdMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is SteamAccount account && item.Tag is int idType)
        {
            var idConvert = new SteamIdConvert(account.AccountId);
            var id = idType switch
            {
                1 => idConvert.Id,
                2 => idConvert.Id3,
                3 => idConvert.Id32,
                _ => idConvert.Id64,
            };
            App.Instance.CopyToClipboardCommandCore(id);
        }
    }

    private void OpenUserDataFolderMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is SteamAccount account)
        {
            if (ISteamService.Instance.SteamDirPath != null)
                IPlatformService.Instance.OpenFolder(Path.Combine(ISteamService.Instance.SteamDirPath, account.SteamUser.UserdataPath));
            else
                Toast.Show(ToastIcon.Error, Strings.Error_CurrentSteamPathIncorrect);
        }
    }
}
