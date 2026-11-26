#if MACOS
using Avalonia;
using Avalonia.Controls;
using BD.WTTS.Models;

namespace BD.WTTS.Services.Implementation;

internal sealed class MacDockAccountMenu
{
    NativeMenuItem? accountSection;

    internal void Apply(Application application, string category, IReadOnlyList<SteamAccountShortcut> entries,
        Action<string> switchAccount)
    {
        var dockMenu = NativeDock.GetMenu(application);
        if (accountSection is { } current && (entries.Count == 0 || current.Parent != dockMenu))
        {
            accountSection = null;
            current.Parent?.Items.Remove(current);
        }

        if (entries.Count == 0) return;

        var accountMenu = new NativeMenu();
        foreach (var entry in entries)
        {
            var item = new NativeMenuItem(entry.Title);
            item.Click += (_, _) =>
            {
                if (ReferenceEquals(accountSection?.Menu, accountMenu))
                    switchAccount(entry.AccountName);
            };
            accountMenu.Add(item);
        }

        var index = accountSection is null || dockMenu is null ? -1 : dockMenu.Items.IndexOf(accountSection);
        var section = new NativeMenuItem(category) { Menu = accountMenu };
        accountSection = section;
        if (dockMenu is null)
        {
            dockMenu = new NativeMenu();
            dockMenu.Add(section);
            NativeDock.SetMenu(application, dockMenu);
        }
        else if (index < 0)
        {
            dockMenu.Add(section);
        }
        else
        {
            // Avalonia's macOS exporter refreshes the native menu when the items collection changes.
            dockMenu.Items[index] = section;
        }
    }
}
#endif
