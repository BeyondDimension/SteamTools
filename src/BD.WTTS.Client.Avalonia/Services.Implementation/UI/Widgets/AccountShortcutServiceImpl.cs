#if WINDOWS || MACOS || LINUX
using BD.WTTS.Client.Resources;
using BD.WTTS.Models;

namespace BD.WTTS.Services.Implementation;

internal sealed class AccountShortcutServiceImpl : IAccountShortcutService
{
#if MACOS
    readonly MacDockAccountMenu dockMenu = new();
#endif

    public ValueTask UpdateSteamAccountsAsync(IReadOnlyList<SteamAccountShortcut> accounts)
    {
        try
        {
#if WINDOWS
            if (accounts.Count > 0 && Environment.ProcessPath is { } executable)
            {
                return IJumpListService.Instance.AddJumpItemsAsync(accounts.Select(account => (
                    account.Title, executable, executable, $"-clt steam -account {account.AccountName}",
                    Strings.UserChange_BtnTootlip, Strings.UserFastChange)));
            }
#elif MACOS
            if (Avalonia.Application.Current is { } application && Environment.ProcessPath is { } executable)
            {
                dockMenu.Apply(application, Strings.UserFastChange, accounts,
                    accountName => StartMacAccountSwitch(executable, accountName));
            }
#elif LINUX
            LinuxDesktopAccountActions.Apply(accounts);
#endif
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AccountShortcutServiceImpl), ex, "Could not update Steam account shortcuts.");
        }
        return ValueTask.CompletedTask;
    }

#if MACOS
    static void StartMacAccountSwitch(string executable, string accountName)
    {
        try
        {
            var startInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                ArgumentList = { "-clt", "steam", "-account", accountName },
            };
            using var process = Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AccountShortcutServiceImpl), ex, "Could not start the Steam account switch.");
        }
    }
#endif
}
#endif
