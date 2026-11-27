using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{
    string quickUrlText = string.Empty;
    string searchText = string.Empty;
    int selectedFilterIndex;
    bool isCheckingUpdates;
    bool isUpdatingAll;
    string? lastUpdateCheckSummary;

    public override string Name => Strings.ScriptConfig;
    public ReadOnlyObservableCollection<ScriptDTO>? ProxyScripts { get; }
    public ReadOnlyObservableCollection<ScriptItemViewModel> FilteredScripts { get; }
    public ScriptSettingsViewModel Settings { get; }

    public string QuickUrlText
    {
        get => quickUrlText;
        set => this.RaiseAndSetIfChanged(ref quickUrlText, value ?? string.Empty);
    }

    public string SearchText
    {
        get => searchText;
        set => this.RaiseAndSetIfChanged(ref searchText, value ?? string.Empty);
    }

    public int SelectedFilterIndex
    {
        get => selectedFilterIndex;
        set => this.RaiseAndSetIfChanged(ref selectedFilterIndex, value);
    }

    public bool IsCheckingUpdates
    {
        get => isCheckingUpdates;
        private set => this.RaiseAndSetIfChanged(ref isCheckingUpdates, value);
    }

    public bool IsUpdatingAll
    {
        get => isUpdatingAll;
        private set => this.RaiseAndSetIfChanged(ref isUpdatingAll, value);
    }

    public int TotalScriptCount => allScripts.Count;
    public int EnabledScriptCount => allScripts.Count(item => item.IsEnabled);
    public int AvailableUpdateCount => allScripts.Count(item => item.IsUpdate);
    public bool HasAvailableUpdates => AvailableUpdateCount > 0;
    public bool HasAnyScripts => TotalScriptCount > 0;
    public bool IsFilteredEmpty => HasAnyScripts && FilteredScripts.Count == 0;
    public bool IsCompletelyEmpty => !HasAnyScripts;
    public bool ShowRestartProxyHint => service.IsScriptConfigDirtyWhileProxyRunning;
    public string TotalCountText => Strings.Script_TotalCountFormat.Format(TotalScriptCount);
    public string EnabledCountText => Strings.Script_EnabledCountFormat.Format(EnabledScriptCount);
    public string UpdatesCountText => Strings.Script_UpdatesCountFormat.Format(AvailableUpdateCount);
    public string UpdateBannerMessage => Strings.Script_UpdateAvailableBanner.Format(AvailableUpdateCount);
    public string? LastUpdateCheckSummary
    {
        get => lastUpdateCheckSummary;
        private set
        {
            this.RaiseAndSetIfChanged(ref lastUpdateCheckSummary, value);
            this.RaisePropertyChanged(nameof(HasLastUpdateCheckSummary));
        }
    }
    public bool HasLastUpdateCheckSummary => !string.IsNullOrWhiteSpace(LastUpdateCheckSummary);
    public Func<ScriptAddAndInspectViewModel, Task<bool>>? ShowInspectDialogAsync { get; set; }
    public Func<ScriptStorePageViewModel, Task>? ShowScriptStoreDialogAsync { get; set; }

    public ICommand OpenAddFromUrlDialogCommand { get; }
    public ICommand PasteQuickUrlCommand { get; }
    public ICommand QuickInspectUrlCommand { get; }
    public ICommand AddNewScriptCommand { get; }
    public ICommand ScriptStoreCommand => Settings.OpenScriptStoreCommand;
    public ICommand OpenScriptCatCommand => Settings.OpenScriptCatCommand;
    public ICommand OpenGreasyForkCommand => Settings.OpenGreasyForkCommand;
    public ICommand CheckAllUpdatesCommand { get; }
    public ICommand UpdateAllScriptsCommand { get; }
    public ICommand RefreshALLScriptCommand { get; }
    public ICommand OpenScriptsDirectoryCommand => Settings.OpenScriptsDirectoryCommand;
    public ICommand ClearFilterCommand { get; }
    public ICommand InspectScriptRulesCommand { get; }
    public ICommand CheckSingleScriptUpdateCommand { get; }
    public ICommand UpdateScriptItemCommand { get; }
    public ICommand EditScriptItemCommand { get; }
    public ICommand RefreshScriptItemCommand { get; }
    public ICommand DeleteScriptItemCommand { get; }
    public ICommand CopyScriptLinkCommand { get; }
    public ICommand OpenScriptHomeCommand { get; }
}
