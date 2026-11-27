using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UI.ViewModels;

public sealed class ScriptAddAndInspectViewModel : ViewModelBase
{
    static readonly AvaloniaFilePickerFileTypeFilter ScriptFileTypes = new AvaloniaFilePickerFileTypeFilter.Item[]
    {
        new("JavaScript") { Patterns = ["*.js"], MimeTypes = [MediaTypeNames.JS] },
        new("Text") { Patterns = ["*.txt"], MimeTypes = [MediaTypeNames.TXT] },
    };

    readonly ProxyService service;
    readonly IScriptManager scriptManager;
    readonly CancellationTokenSource lifetime = new();
    string urlInput = string.Empty;
    bool enableAfterAdd = true;
    bool compileRequires = true;
    bool isBusy;
    string? errorMessage;
    ScriptInspectionResult? inspectionResult;

    public ScriptAddAndInspectViewModel(bool isAddMode = true, bool showUrlInputBar = true)
    {
        IsAddMode = isAddMode;
        ShowUrlInputBar = isAddMode && showUrlInputBar;
        service = ProxyService.Current;
        scriptManager = IScriptManager.Instance;

        var canInspect = this.ObserveValue(nameof(IsBusy), static vm => vm.IsBusy).Select(busy => !busy);
        PasteUrlCommand = ReactiveCommand.CreateFromTask(PasteUrlAsync, canInspect).AddTo(this);
        InspectUrlCommand = ReactiveCommand.CreateFromTask(() => InspectFromUrlAsync(UrlInput), canInspect).AddTo(this);
        PickLocalFileCommand = ReactiveCommand.CreateFromTask(
            () => PickScriptFileAsync(path => InspectFromFileAsync(path)), canInspect).AddTo(this);
        OpenScriptCatCommand = ReactiveCommand.Create(() => { Browser2.Open(ScriptExternalLinks.ScriptCatUrl); }).AddTo(this);
        OpenGreasyForkCommand = ReactiveCommand.Create(() => { Browser2.Open(ScriptExternalLinks.GreasyForkUrl); }).AddTo(this);
        OpenScriptStoreCommand = ReactiveCommand.Create(() => OpenScriptStoreRequested?.Invoke()).AddTo(this);
    }

    public ICommand PasteUrlCommand { get; }
    public ICommand InspectUrlCommand { get; }
    public ICommand PickLocalFileCommand { get; }
    public ICommand OpenScriptCatCommand { get; }
    public ICommand OpenGreasyForkCommand { get; }
    public ICommand OpenScriptStoreCommand { get; }

    public event Action? OpenScriptStoreRequested;

    public bool IsAddMode { get; }
    public bool ShowUrlInputBar { get; }
    public string DialogTitle => IsAddMode ? Strings.Script_Inspect_DialogTitleAdd : Strings.Script_Inspect_DialogTitleView;
    public string UrlInput
    {
        get => urlInput;
        set
        {
            var input = value ?? string.Empty;
            if (input == urlInput)
                return;

            this.RaiseAndSetIfChanged(ref urlInput, input);
            if (!IsBusy)
                ApplyInspection(null, null);
        }
    }

    public bool IsBusy => isBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage => errorMessage;
    public bool HasResult => InspectionResult != null;
    public bool ShowReadyHint => ShowUrlInputBar && !IsBusy && !HasError && !HasResult;
    public bool CanConfirmInstall => IsAddMode && !IsBusy && InspectionResult is { HasUserScriptHeader: true };
    public string ConfirmButtonText => IsUpgradeOrReplace ? Strings.Script_Inspect_ConfirmUpgrade : Strings.Script_Inspect_ConfirmAdd;
    public bool EnableAfterAdd
    {
        get => enableAfterAdd;
        set => this.RaiseAndSetIfChanged(ref enableAfterAdd, value);
    }
    public bool CompileRequires
    {
        get => compileRequires;
        set => this.RaiseAndSetIfChanged(ref compileRequires, value);
    }
    public ScriptInspectionResult? InspectionResult => inspectionResult;
    public string ScriptName => Display(InspectionResult?.Name);
    public string ScriptVersion => Display(InspectionResult?.Version);
    public string ScriptAuthor => Display(InspectionResult?.Author);
    public string ScriptDescription => InspectionResult?.Description ?? string.Empty;
    public bool HasDescription => !string.IsNullOrWhiteSpace(ScriptDescription);
    public string? ScriptIconUrl => InspectionResult?.IconUrl;
    public string SourceOriginLabel => InspectionResult?.SourceOriginLabel ?? string.Empty;
    public string RunAt => Display(InspectionResult?.RunAt);
    public string SizeDisplay => InspectionResult is null ? string.Empty : $"{InspectionResult.ContentByteLength:N0} B";
    public string HomepageUrl => InspectionResult?.HomepageUrl ?? string.Empty;
    public bool HasHomepageUrl => !string.IsNullOrWhiteSpace(HomepageUrl);
    public string DownloadUrl => InspectionResult?.DownloadUrl ?? string.Empty;
    public bool HasDownloadUrl => !string.IsNullOrWhiteSpace(DownloadUrl);
    public string UpdateUrl => InspectionResult?.UpdateUrl ?? string.Empty;
    public bool HasUpdateUrl => !string.IsNullOrWhiteSpace(UpdateUrl);
    public string InstallModeBadge => InspectionResult switch
    {
        { IsUpgrade: true } info => Strings.Script_Inspect_BadgeUpgradeFormat.Format(
            ScriptVersionComparer.TrimVersionPrefix(info.ExistingVersion), ScriptVersionComparer.TrimVersionPrefix(info.Version)),
        { ExistingScript: not null } info => Strings.Script_Inspect_BadgeReplaceFormat.Format(
            ScriptVersionComparer.TrimVersionPrefix(info.ExistingVersion)),
        _ => Strings.Script_Inspect_BadgeNew,
    };
    public bool IsUpgradeOrReplace => InspectionResult?.ExistingScript != null;
    public IReadOnlyList<ScriptRuleWarning> Warnings => InspectionResult?.Warnings ?? [];
    public bool HasWarnings => Warnings.Count != 0;
    public IReadOnlyList<string> MatchRules => InspectionResult?.EffectiveMatchRules ?? [];
    public bool HasMatchRules => MatchRulesCount != 0;
    public int MatchRulesCount => MatchRules.Count;
    public IReadOnlyList<string> ExcludeRules => InspectionResult?.ExcludeRules ?? [];
    public bool HasExcludeRules => ExcludeRulesCount != 0;
    public int ExcludeRulesCount => ExcludeRules.Count;
    public IReadOnlyList<string> Grants => InspectionResult?.Grants ?? [];
    public bool HasGrants => Grants.Count != 0;
    public IReadOnlyList<string> Connects => InspectionResult?.Connects ?? [];
    public bool HasConnects => Connects.Count != 0;
    public bool HasPermissions => HasGrants || HasConnects;
    public IReadOnlyList<string> Requires => InspectionResult?.Requires ?? [];
    public bool HasRequires => RequiresCount != 0;
    public int RequiresCount => Requires.Count;
    public string HeaderPreview => InspectionResult?.HeaderPreview ?? string.Empty;
    public bool HasHeaderPreview => !string.IsNullOrWhiteSpace(HeaderPreview);

    static string Display(string? value) => string.IsNullOrWhiteSpace(value) ? Strings.Script_Inspect_None : value;

    internal static Task PickScriptFileAsync(Func<string, Task> inspectAsync)
        => FilePicker2.PickAsync(inspectAsync, ScriptFileTypes, Strings.Script_Inspect_SwitchLocalButton);

    async Task PasteUrlAsync()
    {
        try
        {
            var text = (await Clipboard2.GetTextAsync())?.Trim();
            if (!string.IsNullOrEmpty(text))
                UrlInput = text;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }

    public Task InspectFromUrlAsync(string url, CancellationToken cancellationToken = default)
        => RunInspectionAsync(token => scriptManager.InspectScriptFromUrlAsync(url.Trim(), service.ProxyScripts.Items.ToArray(), token),
            cancellationToken, url.Trim());

    public Task InspectFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        => RunInspectionAsync(token => scriptManager.InspectScriptFromFileAsync(filePath, service.ProxyScripts.Items.ToArray(), token),
            cancellationToken, string.Empty);

    public Task LoadInstalledScriptAsync(ScriptDTO script, CancellationToken cancellationToken = default)
        => RunInspectionAsync(token => scriptManager.InspectInstalledScriptAsync(script, token), cancellationToken);

    Task RunInspectionAsync(Func<CancellationToken, Task<IApiRsp<ScriptInspectionResult?>>> inspect,
        CancellationToken cancellationToken, string? input = null)
        => MainThread2.InvokeOnMainThreadAsync(async () =>
        {
            if (Disposed || IsBusy)
                return;

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken);
            isBusy = true;
            if (input != null)
                UrlInput = input;
            ApplyInspection(null, null);
            try
            {
                var result = await inspect(cancellation.Token);
                if (!cancellation.IsCancellationRequested)
                    ApplyInspection(result.Content, result.IsSuccess ? null : result.Message);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                if (!Disposed)
                    ApplyInspection(null, ex.Message);
            }
            finally
            {
                isBusy = false;
                if (!Disposed)
                    NotifyInspectionChanged();
            }
        });

    void ApplyInspection(ScriptInspectionResult? inspection, string? error)
    {
        inspectionResult = inspection;
        errorMessage = error;
        if (IsAddMode && inspection != null)
        {
            enableAfterAdd = inspection.ExistingScript?.Disable != true;
            compileRequires = inspection.ExistingScript?.IsCompile ?? true;
        }

        NotifyInspectionChanged();
    }

    void NotifyInspectionChanged()
    {
        this.RaisePropertyChanged(string.Empty);
        this.RaisePropertyChanged(nameof(IsBusy));
        this.RaisePropertyChanged(nameof(CanConfirmInstall));
        this.RaisePropertyChanged(nameof(ConfirmButtonText));
    }

    public Task<bool> ConfirmInstallAsync(CancellationToken cancellationToken = default)
        => MainThread2.InvokeOnMainThreadAsync(async () =>
        {
            if (Disposed || !CanConfirmInstall || InspectionResult is not { } inspection)
                return false;

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken);
            isBusy = true;
            NotifyInspectionChanged();
            try
            {
                var result = await service.InstallInspectedScriptAsync(inspection, EnableAfterAdd, CompileRequires, cancellation.Token);
                Toast.Show(result.IsSuccess ? ToastIcon.Success : ToastIcon.Error, result.Message);
                return result.IsSuccess;
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                return false;
            }
            finally
            {
                isBusy = false;
                if (!Disposed)
                    NotifyInspectionChanged();
            }
        });

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}
