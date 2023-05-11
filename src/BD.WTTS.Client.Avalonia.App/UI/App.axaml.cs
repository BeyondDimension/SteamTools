namespace BD.WTTS.UI;

public sealed partial class App : Application
{
    const string TAG = "AvaApp";

    /// <summary>
    /// 获取当前主窗口
    /// </summary>
    public Window? MainWindow { get; internal set; }

    /// <summary>
    /// 获取任意一个窗口，优先返回主窗口
    /// </summary>
    /// <returns></returns>
    public Window? GetFirstOrDefaultWindow()
    {
        var window = MainWindow;
        if (window == null)
        {
            if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
            {
                window = classicDesktopStyleApplicationLifetime.Windows.FirstOrDefault(x => x != null);
            }
        }
        return window;
    }

    public override void Initialize()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Startup.GlobalExceptionHandler.Handler(ex, "load App.Xaml fail.");
        }
    }

    public override void RegisterServices()
    {
        //if (!Design.IsDesignMode)
        //{
        //    AvaloniaLocator.CurrentMutable
        //        .Bind<IFontManagerImpl>().ToConstant(Ioc.Get<IFontManagerImpl>());
        //}

        base.RegisterServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow ??= new MainWindow();
        }
        SetThemeNotChangeValue((AppTheme)UISettings.Theme.Value);
        SetThemeAccent(UISettings.GetUserThemeAccent.Value ? bool.TrueString : UISettings.ThemeAccent.Value);
        base.OnFrameworkInitializationCompleted();
    }

    //#if WINDOWS
    //    /// <inheritdoc cref="IPlatformService.SetDesktopBackgroundToWindow(nint, int, int)"/>
    //    public void SetDesktopBackgroundWindow()
    //    {
    //        //try
    //        //{
    //        //    if (MainWindow is MainWindow window)
    //        //    {
    //        //        IPlatformService.Instance.SetDesktopBackgroundToWindow(
    //        //            window.BackHandle,
    //        //            Convert.ToInt32(window.Width),
    //        //            Convert.ToInt32(window.Height));
    //        //    }
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    Log.Error(TAG, ex, "SetDesktopBackgroundToWindow fail.");
    //        //}
    //    }
    //#endif

    /// <summary>
    /// 设置当前打开窗口的 AvaloniaWindow 背景透明材质
    /// </summary>
    /// <param name="level"></param>
    public void SetAllWindowransparencyMateria(int level)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                window.TransparencyLevelHint = (WindowTransparencyLevel)level;

                //if (window.TransparencyLevelHint == WindowTransparencyLevel.Transparent ||
                //    window.TransparencyLevelHint == WindowTransparencyLevel.Blur)
                //{
                //    ((IPseudoClasses)window.Classes).Set(":transparent", true);
                //}
                //else
                //{
                //    ((IPseudoClasses)window.Classes).Set(":transparent", false);
                //}
            }
        }
    }

    public bool Shutdown(int exitCode = 0)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                desktop.Shutdown(exitCode);
            });
            return true;
        }
        return false;
    }

    internal void InitSettingSubscribe()
    {
        GeneralSettings.IsEnableTrayIcon.Subscribe(x => InitTrayIcon());
        UISettings.ThemeAccent.Subscribe(SetThemeAccent);
        UISettings.GetUserThemeAccent.Subscribe(x =>
            SetThemeAccent(x ? bool.TrueString : UISettings.ThemeAccent.Value));

        GeneralSettings.WindowsStartupAutoRun.Subscribe(IApplication.SetBootAutoStart);

        UISettings.WindowBackgroundMateria.Subscribe(SetAllWindowransparencyMateria, false);

        //#if WINDOWS
        //        UISettings.EnableDesktopBackground.Subscribe(x =>
        //        {
        //            if (x)
        //            {
        //                //var t = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;
        //                //if (t == WindowTransparencyLevel.None ||
        //                //    t == WindowTransparencyLevel.Mica)
        //                //{
        //                //    UISettings.EnableDesktopBackground.Value = false;
        //                //    Toast.Show(string.Format(AppResources.Settings_UI_EnableDesktopBackground_Error, t));
        //                //    return;
        //                //}
        //                SetDesktopBackgroundWindow();
        //            }
        //        }, false);
        //#endif
    }
}
