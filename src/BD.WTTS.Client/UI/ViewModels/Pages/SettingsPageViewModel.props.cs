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

    MenuTabItemViewModel _SelectDefaultPage =
        IViewModelManager.Instance.MainWindow2.TabItems.OfType<MenuTabItemViewModel>().FirstOrDefault(
            s =>
            {
                if (string.IsNullOrEmpty(UISettings.StartDefaultPageName.Value))
                {
                    return s.Id == "Watt Toolkit-Welcome";
                }
                return s.Id == UISettings.StartDefaultPageName.Value;
            });

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public MenuTabItemViewModel SelectDefaultPage
    {
        get => _SelectDefaultPage;
        set
        {
            this.RaiseAndSetIfChanged(ref _SelectDefaultPage, value);
            if (!string.IsNullOrEmpty(value?.Id))
            {
                UISettings.StartDefaultPageName.Value = value.Id;
            }
        }
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? SelectImage_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? ResetImage_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenFolder_Click { get; }

    /// <summary>
    /// 点击间隔时间（秒）Windows 11 上资源管理器启动缓慢，设置一个间隔时间避免频繁点击
    /// </summary>
    const double clickOpenFolderIntervalSeconds = 7.5d;
    readonly Dictionary<string, DateTime> clickOpenFolderTimeRecord = new();

    public static string[] GetThemes() => new[]
    {
        AppResources.Settings_UI_SystemDefault,
        AppResources.Settings_UI_Light,
        AppResources.Settings_UI_Dark,
    };

    [Reactive]
    public ObservableCollection<PluginResult<IPlugin>>? Plugins { get; set; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? DeletePlugin_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? CheckUpdate_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenPluginDirectory_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OpenPluginCacheDirectory_Click { get; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? SwitchEnablePlugin_Click { get; }

}
