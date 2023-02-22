namespace BD.WTTS.UI;

partial class App : IApplication
{
    public static App Instance => Program.Host.Instance.App;

    IApplication.IProgramHost IApplication.ProgramHost => Program.Host.Instance;

    /// <summary>
    /// 初始化设置项变更时监听
    /// </summary>
    void IApplication.InitSettingSubscribe()
    {
        UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
        UISettings.Language.Subscribe(ResourceService.ChangeLanguage);

        if (IApplication.IsDesktop())
        {
            GeneralSettings.IsEnableTrayIcon.Subscribe(x => InitTrayIcon(ApplicationLifetime as IClassicDesktopStyleApplicationLifetime));
        }
    }

    /// <inheritdoc cref="IApplication.InitSettingSubscribe"/>
    void PlatformInitSettingSubscribe()
    {
        IApplication @this = this;
        @this.InitSettingSubscribe();
        UISettings.ThemeAccent.Subscribe(SetThemeAccent);
        UISettings.GetUserThemeAccent.Subscribe(x => SetThemeAccent(x ? bool.TrueString : UISettings.ThemeAccent.Value));

        GeneralSettings.WindowsStartupAutoRun.Subscribe(IApplication.SetBootAutoStart);

        UISettings.WindowBackgroundMateria.Subscribe(SetAllWindowransparencyMateria, false);

#if WINDOWS
        UISettings.EnableDesktopBackground.Subscribe(x =>
        {
            if (x)
            {
                //var t = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;
                //if (t == WindowTransparencyLevel.None ||
                //    t == WindowTransparencyLevel.Mica)
                //{
                //    UISettings.EnableDesktopBackground.Value = false;
                //    Toast.Show(string.Format(AppResources.Settings_UI_EnableDesktopBackground_Error, t));
                //    return;
                //}
                SetDesktopBackgroundWindow();
            }
        }, false);
#endif
    }

    void IApplication.PlatformInitSettingSubscribe() => PlatformInitSettingSubscribe();

    /// <summary>
    /// Restores the app's main window by setting its <c>WindowState</c> to
    /// <c>WindowState.Normal</c> and showing the window.
    /// </summary>
    public void RestoreMainWindow()
    {
        Window? mainWindow = null;

    ReTry:
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindow = desktop.MainWindow;
            if (mainWindow == null)
            {
                //mainWindow = MainWindow;
                //desktop.MainWindow = MainWindow;
                mainWindow = MainWindow = desktop.MainWindow = new MainWindow();
                mainWindow.DataContext = IViewModelManager.Instance.MainWindow;
            }

        }

        if (mainWindow == null)
        {
            throw new ArgumentNullException(nameof(mainWindow));
        }

        try
        {
            mainWindow.Show();
        }
        catch (InvalidOperationException)
        {
            mainWindow = null;
            goto ReTry;
        }

        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Topmost = true;
        mainWindow.Topmost = false;
        mainWindow.BringIntoView();
        mainWindow.ActivateWorkaround(); // Extension method hack because of https://github.com/AvaloniaUI/Avalonia/issues/2975
        mainWindow.Focus();

        //// Again, ugly hack because of https://github.com/AvaloniaUI/Avalonia/issues/2994
        //mainWindow.Width += 0.1;
        //mainWindow.Width -= 0.1;
    }

    public void SetTopmostOneTime()
    {
        if (MainWindow != null && MainWindow.WindowState != WindowState.Minimized)
        {
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
        }
    }

    public bool HasActiveWindow()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.Windows.Any_Nullable(x => x.IsActive))
            {
                return true;
            }
        }
        return false;
    }

    public Window GetActiveWindow()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var activeWindow = desktop.Windows.FirstOrDefault(x => x.IsActive);
            if (activeWindow != null)
            {
                return activeWindow;
            }
        }
        return MainWindow!;
    }

    /// <summary>
    /// 打开子窗口
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public async Task ShowDialogWindow(Window window)
    {
        var owner = GetActiveWindow();
        if (owner != null)
        {
            try
            {
                await window.ShowDialog(owner);
                return;
            }
            catch (InvalidOperationException)
            {
            }
        }
        window.Show();
    }

    public void ShowWindow(Window window)
    {
        var owner = GetActiveWindow();
        if (owner != null)
        {
            try
            {
                window.Show(owner);
                return;
            }
            catch (InvalidOperationException)
            {
            }

        }
        window.Show();
    }

    public void ShowWindowNoParent(Window window)
    {
        window.Show();
    }

    void IApplication.Shutdown() => Shutdown();

    object IApplication.CurrentPlatformUIHost => MainWindow!;

    DeploymentMode IApplication.DeploymentMode => Program.Host.Instance.DeploymentMode;
}