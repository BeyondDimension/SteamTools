using Avalonia.Controls;
using Avalonia.Threading;
using BD.SteamClient.Services;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI;

partial class App
{
    public Dictionary<string, TrayMenuItem> TrayMenus { get; } = new Dictionary<string, TrayMenuItem>();

    public TrayIcons TrayIcons { get; init; } = new TrayIcons();

    readonly Dictionary<string, ICommand> trayIconMenus = new();
    readonly CompositeDisposable trayMenuBindings = new();
    static readonly Uri trayIconAsset = new("avares://BD.WTTS.Client.Avalonia/UI/Assets/ApplicationIcon.ico");
    TrayPanelWindow? trayPanel;
#if WINDOWS
    WindowsTrayIcon? windowsTrayIcon;
#endif

    public IReadOnlyDictionary<string, ICommand> TrayIconMenus => trayIconMenus;

    void InitTrayIcon()
    {
        if (ApplicationLifetime is not
            IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
            return;

        var s = Startup.Instance;
        if (s.IsMainProcess)
        {
            s.HasTrayIcon = GeneralSettings.TrayIcon.Value;
            if (s.HasTrayIcon)
            {
                EnsureTrayIcon();
                classicDesktopStyleApplicationLifetime.Exit -= OnTrayLifetimeExit;
                classicDesktopStyleApplicationLifetime.Exit += OnTrayLifetimeExit;
            }
            else
            {
                classicDesktopStyleApplicationLifetime.Exit -= OnTrayLifetimeExit;
                RemoveTrayIcon();
            }

            classicDesktopStyleApplicationLifetime.ShutdownMode =
#if UI_DEMO
                        ShutdownMode.OnMainWindowClose;
#else
                s.HasTrayIcon ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnMainWindowClose;
#endif
#if MACOS
            if (Current!.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
            {
                activatableLifetime.Activated -= ActivatableLifetimeOnActivated;
                activatableLifetime.Activated += ActivatableLifetimeOnActivated;
            }
#endif
        }
    }

    void EnsureTrayIcon()
    {
        if (TrayIcons.Count > 0)
            return;
#if WINDOWS
        if (windowsTrayIcon != null)
            return;
        using (var iconStream = AssetLoader.Open(trayIconAsset))
            windowsTrayIcon = WindowsTrayIcon.TryCreate(iconStream, AssemblyInfo.Trademark, RestoreMainWindow,
                point => ShowTrayPanel(point));
        if (windowsTrayIcon != null)
            return;
#endif

        using var icon = AssetLoader.Open(trayIconAsset);
        TrayIcon.SetIcons(this, TrayIcons);
        TrayIcons.Add(new TrayIcon
        {
            Icon = new WindowIcon(icon),
            ToolTipText = AssemblyInfo.Trademark,
            Command = new Command(_ => RestoreMainWindow()),
            Menu = new NativeMenu(),
        });
        RebuildNativeTrayMenu();
    }

    void OnTrayLifetimeExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) => RemoveTrayIcon();

    void RemoveTrayIcon()
    {
        trayPanel?.Close();
        trayMenuBindings.Clear();
        TrayIcon.SetIcons(this, null);
        TrayIcons.Clear();
#if WINDOWS
        windowsTrayIcon?.Dispose();
        windowsTrayIcon = null;
#endif
    }

    void ShowTrayPanel(PixelPoint? anchor = null)
    {
        if (trayPanel is { } activePanel)
        {
            activePanel.ActivateWorkaround();
            return;
        }

        Action? restoreShellFocus = null;
#if WINDOWS
        if (windowsTrayIcon is { } icon)
            restoreShellFocus = icon.RestoreShellFocus;
#endif
        var model = new TrayPanelViewModel(CreateSteamTrayAction(), ObserveSteamRunning());
        var panel = new TrayPanelWindow(model, anchor, restoreShellFocus);
        trayPanel = panel;
        panel.Closed += (_, _) => trayPanel = null;
        UpdateTrayPanel(panel);
        panel.Show();
        panel.ActivateWorkaround();
    }

    void UpdateTrayPanel(TrayPanelWindow panel)
    {
        TrayMenus.TryGetValue(AssemblyInfo.Accelerator, out var accelerator);
        TrayMenus.TryGetValue(AssemblyInfo.GameAccount, out var accounts);
        panel.ViewModel.SetAccelerator(accelerator, TrayToggleState.Unavailable);
        panel.ViewModel.SetAccounts(accounts);
        panel.SetOtherActions(TrayMenus.Where(static pair => pair.Key is not (AssemblyInfo.Accelerator or AssemblyInfo.GameAccount))
            .Select(static pair => pair.Value));
    }

#if MACOS
    /// <summary>
    /// 从 macOS Dock 栏恢复窗口显示
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ActivatableLifetimeOnActivated(object? sender, ActivatedEventArgs e)
    {
        switch (e.Kind)
        {
            case ActivationKind.Background:
            case ActivationKind.Reopen:
                RestoreMainWindow();
                break;
        }
    }
#endif

    public void UpdateMenuItems() => MainThread2.BeginInvokeOnMainThread(RefreshTrayMenu);

    void RefreshTrayMenu()
    {
        if (trayPanel != null)
            UpdateTrayPanel(trayPanel);
        RebuildNativeTrayMenu();
    }

    void RebuildNativeTrayMenu()
    {
        if (TrayIcons.Count == 0 || TrayIcons[0].Menu is not { } menu)
            return;

        trayMenuBindings.Clear();
        menu.Items.Clear();
        menu.Add(new NativeMenuItem(Strings.Tray_QuickActions) { Command = new Command(_ => ShowTrayPanel()) });
        menu.Add(new NativeMenuItemSeparator());

        TrayMenus.TryGetValue(AssemblyInfo.Accelerator, out var accelerator);
        var toggle = new TraySwitchMenuItem
        {
            CommandParameter = accelerator?.CommandParameter,
            Command = accelerator?.Command,
        };
        void UpdateToggle(TrayToggleState state)
        {
            toggle.Header = $"{Strings.CommunityFix} · {TrayPanelViewModel.StatusText(state)}";
            toggle.SetConfirmedState(TrayPanelViewModel.IsOn(state));
            toggle.IsEnabled = TrayPanelViewModel.CanToggle(state) && toggle.Command?.CanExecute(toggle.CommandParameter) == true;
        }
        UpdateToggle(TrayToggleState.Unavailable);
        if (accelerator?.ToggleState is { } state)
            trayMenuBindings.Add(state.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(UpdateToggle));
        menu.Add(toggle);

        foreach (var (key, item) in TrayMenus)
            if (key != AssemblyInfo.Accelerator)
                menu.Add(CreateNativeTrayMenuItem(item));

        var steamItem = new NativeMenuItem { Command = CreateSteamTrayAction() };
        trayMenuBindings.Add(steamItem.Bind(NativeMenuItem.HeaderProperty,
            ObserveSteamRunning().Select(static running => running ? Strings.CloseSteam : Strings.StartSteam)));
        menu.Add(new NativeMenuItem(Strings.OpenMainWindow) { Command = new Command(_ => RestoreMainWindow()) });
        menu.Add(steamItem);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(new NativeMenuItem(Strings.Exit) { Command = new Command(_ => Shutdown()) });
    }

    static ICommand CreateSteamTrayAction() => ReactiveCommand.CreateFromTask(async () =>
    {
        try
        {
            var steam = ISteamService.Instance;
            if (steam.IsRunningSteamProcess)
                await steam.TryKillSteamProcess();
            else
                steam.StartSteamWithParameter();
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(App));
        }
    });

    static IObservable<bool> ObserveSteamRunning() => Observable.Defer(() =>
    {
        var steam = SteamConnectService.Current;
        return steam.Changed.Where(static change => change.PropertyName == nameof(steam.IsRunningSteamProcess))
            .Select(_ => steam.IsRunningSteamProcess).StartWith(steam.IsRunningSteamProcess).DistinctUntilChanged();
    });

    NativeMenuItem CreateNativeTrayMenuItem(TrayMenuItem item)
    {
        var result = new NativeMenuItem(GetTrayItemName(item))
        {
            CommandParameter = item.CommandParameter,
            Command = item.Command,
        };
        if (item.IsVisible is BindingBase visible)
            trayMenuBindings.Add(result.Bind(NativeMenuItem.IsVisibleProperty, visible));
        if (item.IsEnabled is BindingBase enabled)
            trayMenuBindings.Add(result.Bind(NativeMenuItem.IsEnabledProperty, enabled));

        if (item.Accounts is { } accountRows)
        {
            var submenu = new NativeMenu();
            var manage = new NativeMenuItem(Strings.Tray_ManageAccounts)
            {
                CommandParameter = item.CommandParameter,
                Command = item.Command,
            };
            // Native status menus cannot host full account cards. Keep direct switching and management available.
            result.Command = null;
            result.Menu = submenu;
            submenu.Add(manage);
            trayMenuBindings.Add(accountRows.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(snapshot =>
            {
                submenu.Items.Clear();
                submenu.Add(manage);
                submenu.Add(new NativeMenuItemSeparator());
                if (snapshot.Rows.Count == 0)
                {
                    submenu.Add(new NativeMenuItem(TrayPanelViewModel.AccountsStatusText(snapshot.Status)) { IsEnabled = false });
                    if (snapshot.Status == TrayAccountsStatus.Failed && item.RefreshAccountsCommand is { } refresh)
                        submenu.Add(new NativeMenuItem(Strings.Retry) { Command = refresh });
                }
                foreach (var row in snapshot.Rows)
                {
                    var header = row.HasDetail ? $"{row.Name} · {row.Detail}" : row.Name;
                    if (row.IsCurrent)
                        header += $" ({Strings.Tray_CurrentAccount})";
                    else if (row.IsRecent)
                        header += $" ({Strings.UserChange_RecentLogin})";
                    submenu.Add(new NativeMenuItem(header)
                    {
                        CommandParameter = row.CommandParameter,
                        Command = row.IsCurrent ? null : row.Command,
                        IsEnabled = !row.IsCurrent && row.Command.CanExecute(row.CommandParameter),
                    });
                }
            }));
        }
        else if (item.Items is { Count: > 0 } children)
        {
            var submenu = new NativeMenu();
            foreach (var child in children)
                submenu.Add(CreateNativeTrayMenuItem(child));
            result.Menu = submenu;
        }

        return result;
    }

    sealed class TraySwitchMenuItem : NativeMenuItem
    {
        bool confirmedState;

        static TraySwitchMenuItem()
        {
            IsCheckedProperty.OverrideMetadata<TraySwitchMenuItem>(new(coerce: static (item, _) =>
                ((TraySwitchMenuItem)item).confirmedState));
        }

        public TraySwitchMenuItem() => ToggleType = MenuItemToggleType.CheckBox;

        public void SetConfirmedState(bool value)
        {
            // Ignore the presenter's optimistic write-back; the service reports the result independently.
            confirmedState = value;
            CoerceValue(IsCheckedProperty);
        }
    }

    internal static string GetTrayItemName(TrayMenuItem item) => item.NameResourceKey is { } key
        ? Strings.ResourceManager.GetString(key, Strings.Culture) ?? item.Name
        : item.Name;

    public void UpdateMenuItems(string menuKey, TrayMenuItem trayMenuItem) => MainThread2.BeginInvokeOnMainThread(() =>
    {
        TrayMenus[menuKey] = trayMenuItem;
        RefreshTrayMenu();
    });
}
