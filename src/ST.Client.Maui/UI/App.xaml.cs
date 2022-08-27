using System.Application.Mvvm;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Styles;
using System.Application.UI.ViewModels;
using System.Reactive.Disposables;
using System.Windows.Input;
using EAppTheme = System.Application.Models.AppTheme;
using MauiApplication = Microsoft.Maui.Controls.Application;
using OSAppTheme = Microsoft.Maui.ApplicationModel.AppTheme;

namespace System.Application.UI;

public partial class App : MauiApplication, IDisposableHolder, IApplication, IMauiApplication
{
    public static App Instance => Current is App app ? app : throw new ArgumentNullException(nameof(app));

    #region IApplication.IDesktopProgramHost

    public IApplication.IDesktopProgramHost ProgramHost { get; }

    IApplication.IProgramHost IApplication.ProgramHost => ProgramHost;

    #endregion

    public App(IApplication.IDesktopProgramHost host)
    {
        ProgramHost = host;

        InitializeComponent();

        AppTheme = PlatformAppTheme;
        RequestedThemeChanged += (_, e) => AppTheme = e.RequestedTheme;

        Initialize();
    }

    public Window? MainWindow => Windows.Count >= 1 ? Windows[0] : null;

    MauiApplication IMauiApplication.Current => this;

    void Initialize()
    {
        const bool isTrace =
#if StartWatchTrace
    true;
#else
    false;
#endif
        ProgramHost.OnCreateAppExecuted(handlerViewModelManager: HandlerViewModelManager, isTrace: isTrace);
    }

    public Window GetActiveWindow()
    {
        var activeWindow = Windows.FirstOrDefault(x => x.IsActivated());
        if (activeWindow != null)
        {
            return activeWindow;
        }
        return MainWindow!;
    }

    #region IDisposable members

    readonly CompositeDisposable compositeDisposable = new();

    CompositeDisposable IApplication.CompositeDisposable => compositeDisposable;

    ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

    void IDisposable.Dispose()
    {
        compositeDisposable.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region IApplication

    readonly Dictionary<string, ICommand> mNotifyIconMenus = new();

    public IReadOnlyDictionary<string, ICommand> NotifyIconMenus => mNotifyIconMenus;

    #region Theme

    OSAppTheme _AppTheme = OSAppTheme.Light;

    public OSAppTheme AppTheme
    {
        get => _AppTheme;
        set
        {
            if (value == OSAppTheme.Unspecified) value = OSAppTheme.Light;
            if (_AppTheme == value) return;

            _AppTheme = value;

            SetThemeNotChangeValue(value.Convert());
        }
    }

    EAppTheme IApplication.Theme
    {
        get => AppTheme.Convert();
        set => AppTheme = value.Convert();
    }

    const EAppTheme _DefaultActualTheme = EAppTheme.Light;

    EAppTheme IApplication.DefaultActualTheme => _DefaultActualTheme;

    public void SetThemeNotChangeValue(EAppTheme value)
    {
        var isDark = value == EAppTheme.Dark;
        var themeRes = Resources.MergedDictionaries.First();
        if (isDark)
        {
            if (themeRes is ThemeDark) return;
        }
        else
        {
            if (themeRes is ThemeLight) return;
        }

        var resources = Resources.MergedDictionaries.Skip(1).ToArray();
        Resources.MergedDictionaries.Clear();

        themeRes = isDark ? new ThemeDark() : new ThemeLight();
        Resources.MergedDictionaries.Add(themeRes);

        Array.ForEach(resources, Resources.MergedDictionaries.Add);
    }

    #endregion

    object IApplication.CurrentPlatformUIHost
    {
        get
        {
            foreach (var window in Windows)
            {
                if (window.IsActivated()) return window;
            }
            throw new ArgumentNullException(nameof(IApplication.CurrentPlatformUIHost));
        }
    }

    bool IApplication.HasActiveWindow()
    {
        foreach (var window in Windows)
        {
            if (window.IsActivated()) return true;
        }
        return false;
    }

    #endregion;

    #region Compat

    public void Shutdown() => Quit();

    #endregion
}