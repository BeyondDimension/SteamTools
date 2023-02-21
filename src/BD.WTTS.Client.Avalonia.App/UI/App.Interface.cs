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

        if (OperatingSystem2.IsWindows())
        {
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
        }
    }

    void IApplication.PlatformInitSettingSubscribe() => PlatformInitSettingSubscribe();

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

    void IApplication.Shutdown() => Shutdown();

    object IApplication.CurrentPlatformUIHost => MainWindow!;

    DeploymentMode IApplication.DeploymentMode => Program.Host.Instance.DeploymentMode;
}