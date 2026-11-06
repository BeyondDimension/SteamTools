namespace BD.WTTS.UI.Views.Windows;

/// <summary>Closes an idle main window after it has moved to the tray.</summary>
internal sealed class MainWindowTrayLifetime
{
    readonly MainWindow window;
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(2) };
    WindowState lastVisibleState = WindowState.Normal;

    internal MainWindowTrayLifetime(MainWindow window)
    {
        this.window = window;
        timer.Tick += OnTimerTick;
        window.PropertyChanged += OnWindowPropertyChanged;
        window.Closed += OnWindowClosed;
        if (Startup.Instance.IsMinimize && GeneralSettings.TrayIcon.Value)
            timer.Start();
    }

    void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Visual.IsVisibleProperty && e.Property != Window.WindowStateProperty)
            return;

        if (window.IsVisible && window.WindowState != WindowState.Minimized)
        {
            lastVisibleState = window.WindowState;
            timer.Stop();
        }
        else if (GeneralSettings.TrayIcon.Value)
        {
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Start();
        }
    }

    void OnTimerTick(object? sender, EventArgs e)
    {
        timer.Stop();
        if ((window.IsVisible && window.WindowState != WindowState.Minimized) ||
            !GeneralSettings.TrayIcon.Value || window.PlatformImpl == null)
            return;

        try
        {
            if (!Startup.Instance.HasTrayIcon ||
                App.Instance.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
                    { ShutdownMode: ShutdownMode.OnExplicitShutdown } ||
                !window.TryCloseForTray(lastVisibleState))
            {
                // Keep in-progress login, dialogs and startup alive; retry without a busy loop.
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Start();
            }
        }
        catch (Exception ex)
        {
            // Do not retry a failed snapshot until the window is shown and hidden again.
            Log.Error(nameof(MainWindowTrayLifetime), ex, "Could not release the tray window.");
        }
    }

    void OnWindowClosed(object? sender, EventArgs e)
    {
        timer.Stop();
        window.PropertyChanged -= OnWindowPropertyChanged;
        window.Closed -= OnWindowClosed;
    }
}
