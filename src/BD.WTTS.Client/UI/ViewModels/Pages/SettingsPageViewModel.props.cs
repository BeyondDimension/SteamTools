using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class SettingsPageViewModel : TabItemViewModel
{
    protected override bool IsSingleInstance => true;

    public override string Name => AppResources.Settings;

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/settings.ico";

    public static string? SelectLanguageKey { get; private set; }

    KeyValuePair<string, string> _SelectLanguage;

    public KeyValuePair<string, string> SelectLanguage
    {
        get => _SelectLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
            SelectLanguageKey = value.Key;
        }
    }

    KeyValuePair<string, string> _SelectFont;

    public KeyValuePair<string, string> SelectFont
    {
        get => _SelectFont;
        set => this.RaiseAndSetIfChanged(ref _SelectFont, value);
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? SelectImage_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? ResetImage_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenFolder_Click { get; }

    public IReadOnlyCollection<UpdateChannelType> UpdateChannels { get; }

    const double clickInterval = 3d;
    readonly Dictionary<string, DateTime> clickTimeRecord = new();

    public static string[] GetThemes() => new[]
    {
        AppResources.Settings_UI_SystemDefault,
        AppResources.Settings_UI_Light,
        AppResources.Settings_UI_Dark,
    };

    [Reactive]
    public ObservableCollection<PluginResult<IPlugin>>? Plugins { get; set; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? CheckUpdate_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenPluginDirectory_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenPluginCacheDirectory_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? DeletePlugin_Click { get; }
}
