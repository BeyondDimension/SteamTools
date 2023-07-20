using BD.WTTS.Client.Resources;
using BD.WTTS.Extensions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;

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

            LiveCharts.Configure(config =>
            {
                config
                    // registers SkiaSharp as the library backend
                    // REQUIRED unless you build your own
                    .AddSkiaSharp();
                // adds the default supported types
                // OPTIONAL but highly recommend
                //.AddDefaultMappers()

                // select a theme, default is Light
                // OPTIONAL
                //.AddDarkTheme()

                // In case you need a non-Latin based font, you must register a typeface for SkiaSharp
                config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')); // <- Chinese // mark
                //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('أ'))  // <- Arabic // mark
                //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('あ')) // <- Japanese // mark
                //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('헬')) // <- Korean // mark
                //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('Ж'))  // <- Russian // mark

                if (Theme != AppTheme.FollowingSystem)
                {
                    if (Theme == AppTheme.Light)
                        config.AddLightTheme();
                    else
                        config.AddDarkTheme();
                }
                else
                {
                    var dps = IPlatformService.Instance;
                    dps.SetLightOrDarkThemeFollowingSystem(false);
                    var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                    if (isLightOrDarkTheme.HasValue)
                    {
                        var mThemeFS = IApplication.GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                        if (mThemeFS == AppTheme.Light)
                            config.AddLightTheme();
                        else
                            config.AddDarkTheme();
                    }
                }
            });
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
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView();
        }
        //SetThemeNotChangeValue(UISettings.Theme.Value);
        //SetThemeAccent(UISettings.UseSystemThemeAccent.Value ? bool.TrueString : UISettings.ThemeAccent.Value);
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
    public void SetAllWindowransparencyMateria(WindowBackgroundMaterial level)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                window.TransparencyLevelHint = new WindowTransparencyLevel[] { level.ToWindowTransparencyLevel() };

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

    public void InitSettingSubscribe()
    {
        GeneralSettings.TrayIcon.Subscribe(x => InitTrayIcon());
        UISettings.ThemeAccent.Subscribe(SetThemeAccent);
        UISettings.UseSystemThemeAccent.Subscribe(useSystemThemeAccent
            => SetThemeAccent(useSystemThemeAccent ? bool.TrueString : UISettings.ThemeAccent.Value));

        GeneralSettings.AutoRunOnStartup.Subscribe(IApplication.SetBootAutoStart);

        UISettings.WindowBackgroundMaterial.Subscribe(SetAllWindowransparencyMateria, false);

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
        //                //    Toast.Show(string.Format(AppResources.Settings_UI_EnableDesktopBackground_Error_, t));
        //                //    return;
        //                //}
        //                SetDesktopBackgroundWindow();
        //            }
        //        }, false);
        //#endif
    }

    static FontFamily GetDefaultFontFamily()
    {
        try
        {
            var fontFamily = IPlatformService.Instance.GetDefaultFontFamily();
            return FontFamily.Parse(fontFamily);
        }
        catch
        {
        }
        return FontFamily.Default;
    }

    static readonly Lazy<FontFamily> _DefaultFontFamily = new(GetDefaultFontFamily);

    public static FontFamily DefaultFontFamily => _DefaultFontFamily.Value;
}
