namespace BD.WTTS.UI.Styling;

/// <summary>
/// Includes the fluent theme in an application.
/// </summary>
public class CustomTheme : Styles, IResourceProvider
{
    public const string LightModeString = "Light";
    public const string DarkModeString = "Dark";
    public const string HighContrastModeString = "HighContrast";

    public static readonly ThemeVariant HighContrastTheme = new ThemeVariant(HighContrastModeString,
        ThemeVariant.Light);

    private readonly Uri? _baseUri;
    private bool _hasLoaded;

    public static readonly StyledProperty<string> ModeProperty =
        AvaloniaProperty.Register<CustomTheme, string>(nameof(Mode), DarkModeString);

    /// <summary>
    /// Gets or sets the mode of the fluent theme (light, dark).
    /// </summary>
    public string Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public CustomTheme(Uri baseUri)
    {
        _baseUri = baseUri;
        Init();
    }

    public CustomTheme(IServiceProvider serviceProvider)
    {
        _baseUri = ((IUriContext?)serviceProvider.GetService(typeof(IUriContext)))?.BaseUri;
        Init();
    }

    void Init()
    {
        AvaloniaXamlLoader.Load(this);
        // First load our base and theme resources

        // When initializing, UseSystemTheme overrides any setting of RequestedTheme, this must be
        // explicitly disabled to enable setting the theme manually
        //ResolveThemeAndInitializeSystemResources();

        _hasLoaded = true;
    }

    void ResolveThemeAndInitializeSystemResources()
    {
        ThemeVariant? theme = null;

        //var ps = AvaloniaLocator.Current.GetService<IPlatformSettings>();
        //ps.ColorValuesChanged += OnPlatformColorValuesChanged;

        theme = GetThemeFromIPlatformSettings();

        //AddOrUpdateSystemResource("ContentControlThemeFontFamily", FontFamily.Default);

        // The Resolve...Settings will return null if PreferSystemTheme is false
        if (theme != null && Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = theme;
        }
    }

    private ThemeVariant GetThemeFromIPlatformSettings()
    {
        return Mode switch
        {
            LightModeString => ThemeVariant.Light,
            DarkModeString => ThemeVariant.Dark,
            HighContrastModeString => HighContrastTheme,
            _ => ThemeVariant.Default,
        };
    }

    void AddOrUpdateSystemResource(object key, object value)
    {
        if (Resources.ContainsKey(key))
        {
            Resources[key] = value;
        }
        else
        {
            Resources.Add(key, value);
        }
    }

    bool IResourceNode.HasResources => true;

}
