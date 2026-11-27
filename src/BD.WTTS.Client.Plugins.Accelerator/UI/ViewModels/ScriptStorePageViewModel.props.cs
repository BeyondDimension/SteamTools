namespace BD.WTTS.UI.ViewModels;

public partial class ScriptStorePageViewModel
{
    string searchText = string.Empty;
    int selectedFilterIndex;
    bool isLoading;
    bool hasError;
    string? errorMessage;

    public ReadOnlyObservableCollection<ScriptStoreItemViewModel> FilteredScripts { get; }

    public string SearchText
    {
        get => searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref searchText, value ?? string.Empty);
            this.RaisePropertyChanged(nameof(HasActiveFilter));
        }
    }

    public int SelectedFilterIndex
    {
        get => selectedFilterIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedFilterIndex, value);
            this.RaisePropertyChanged(nameof(HasActiveFilter));
        }
    }

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            this.RaiseAndSetIfChanged(ref isLoading, value);
            this.RaisePropertyChanged(nameof(ShowEmptyState));
        }
    }

    public bool HasError
    {
        get => hasError;
        private set
        {
            this.RaiseAndSetIfChanged(ref hasError, value);
            this.RaisePropertyChanged(nameof(ShowEmptyState));
        }
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        private set
        {
            this.RaiseAndSetIfChanged(ref errorMessage, value);
            HasError = value != null;
        }
    }

    public string CountSummary => Strings.Script_Store_CountFormat.Format(scriptRows.Count, scriptRows.Items.Count(item => item.IsInstalled));
    public bool HasActiveFilter => !string.IsNullOrWhiteSpace(SearchText) || SelectedFilterIndex != 0;
    public bool ShowEmptyState => !IsLoading && !HasError && FilteredScripts.Count == 0;
    public ICommand RefreshCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand DownloadScriptItemCommand { get; }
}
