namespace BD.WTTS.UI;

partial class App
{
    const AppTheme _DefaultActualTheme = AppTheme.Dark;

    AppTheme IApplication.DefaultActualTheme => _DefaultActualTheme;

    AppTheme mTheme = _DefaultActualTheme;

    public AppTheme Theme
    {
        get => mTheme;
        set
        {
            if (value == mTheme) return;
            AppTheme switch_value = value;

            if (value == AppTheme.FollowingSystem)
            {
                var dps = IPlatformService.Instance;
                var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                if (isLightOrDarkTheme.HasValue)
                {
                    switch_value = IApplication.GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                    dps.SetLightOrDarkThemeFollowingSystem(true);
                    if (switch_value == mTheme) goto setValue;
                }
            }
            else if (mTheme == AppTheme.FollowingSystem)
            {
                var dps = IPlatformService.Instance;
                dps.SetLightOrDarkThemeFollowingSystem(false);
                var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                if (isLightOrDarkTheme.HasValue)
                {
                    var mThemeFS = IApplication.GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                    if (mThemeFS == switch_value) goto setValue;
                }
            }

            SetThemeNotChangeValue(switch_value);

        setValue: mTheme = value;
        }
    }

    public void SetThemeNotChangeValue(AppTheme value)
    {
        string? themeName = null;
        //FluentThemeMode mode;

        //switch (value)
        //{
        //    case AppTheme.HighContrast:
        //        themeName = FluentAvaloniaTheme.HighContrastModeString;
        //        //mode = FluentThemeMode.Light;
        //        break;
        //    case AppTheme.Light:
        //        themeName = FluentAvaloniaTheme.LightModeString;
        //        //mode = FluentThemeMode.Light;
        //        //LiveCharts.CurrentSettings.AddLightTheme();
        //        break;
        //    case AppTheme.Dark:
        //    default:
        //        themeName = FluentAvaloniaTheme.DarkModeString;
        //        //mode = FluentThemeMode.Dark;
        //        //LiveCharts.CurrentSettings.AddDarkTheme();
        //        break;
        //}

        //var uri_0 = new Uri($"avares://Avalonia.Themes.Fluent/Fluent{themeName}.xaml");
        //var uri_1 = new Uri($"avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Styles/Theme{themeName}.xaml");

        //var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
        //if (faTheme != null)
        //    faTheme.RequestedTheme = themeName;

        //var csTheme = AvaloniaLocator.Current.GetService<CustomTheme>();
        //if (csTheme != null)
        //    csTheme.Mode = themeName;

        //if (Resources.Count > 1)
        //{
        //    //Styles[0] = new FluentTheme(uri_0)
        //    //{
        //    //    Mode = mode,
        //    //};
        //    Resources.MergedDictionaries[0] = (ResourceDictionary)AvaloniaXamlLoader.Load(uri_1);
        //}
    }

    public static void SetThemeAccent(string? colorHex)
    {
        if (colorHex == null)
        {
            return;
        }
        //var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()!;

        //if (Color.TryParse(colorHex, out var color))
        //{
        //    thm.CustomAccentColor = color;
        //}
        //else
        //{
        //    if (colorHex.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
        //    {
        //        thm.CustomAccentColor = null;
        //    }
        //}

#if WINDOWS
#endif
        //if (OperatingSystem.IsWindows())
        //{
        //    if (OperatingSystem.IsWindowsVersionAtLeast(6, 2))
        //        thm.PreferUserAccentColor = true;
        //    else
        //        thm.PreferUserAccentColor = false;
        //}
        //else
        //{
        //    thm.PreferUserAccentColor = true;
        //}
    }
}