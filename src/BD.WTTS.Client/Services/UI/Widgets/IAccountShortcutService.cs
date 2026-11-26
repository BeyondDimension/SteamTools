using BD.WTTS.Models;

namespace BD.WTTS.Services;

/// <summary>Updates the desktop launcher with the current saved Steam accounts.</summary>
public interface IAccountShortcutService
{
    ValueTask UpdateSteamAccountsAsync(IReadOnlyList<SteamAccountShortcut> accounts);
}
