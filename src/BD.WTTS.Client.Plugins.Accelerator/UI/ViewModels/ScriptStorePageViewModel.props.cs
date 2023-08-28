namespace BD.WTTS.UI.ViewModels;

public partial class ScriptStorePageViewModel : ViewModelBase
{
    readonly ReadOnlyObservableCollection<ScriptDTO> _Scripts;

    public ReadOnlyObservableCollection<ScriptDTO> Scripts => _Scripts;

    readonly SourceList<ScriptDTO> _ScriptsSourceList;

    string? _SearchText;

    public string? SearchText
    {
        get => _SearchText;
        set => this.RaiseAndSetIfChanged(ref _SearchText, value);
    }

    public bool IsScriptsEmpty => !Scripts.Any_Nullable();

    public ICommand? DownloadScriptItemCommand { get; }
}
