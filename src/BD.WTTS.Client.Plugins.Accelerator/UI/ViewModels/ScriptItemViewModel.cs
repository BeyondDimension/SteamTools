using System.ComponentModel;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UI.ViewModels;

public sealed class ScriptItemViewModel : ReactiveObject, IDisposable
{
    readonly ProxyService service;
    bool isCheckingUpdate;
    string updateStatusText = string.Empty;

    internal ScriptItemViewModel(ScriptDTO model, ProxyService service)
    {
        Model = model;
        this.service = service;
        Model.PropertyChanged += OnModelPropertyChanged;
        service.ScriptUpdateResultChanged += ApplyUpdateResult;
        if (service.GetScriptUpdateResult(model) is { } result)
            ApplyUpdateResult(result);
    }

    public ScriptDTO Model { get; }
    public int LocalId => Model.LocalId;
    public string Name => Model.Name;
    public string AuthorName => Model.AuthorName ?? string.Empty;
    public string VersionDisplay => ScriptVersionComparer.FormatVersionDisplay(Model.Version);
    public string Describe => Model.Describe ?? string.Empty;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Describe);
    public string? IconUrl => Model.IconUrl;
    public bool IsBasics => SourceKind == ScriptSourceKind.Basics;
    public bool IsEnabled
    {
        get => !Model.Disable;
        set => Model.Disable = !value;
    }

    public bool IsLoading => Model.IsLoading;
    public bool IsCheckingUpdate
    {
        get => isCheckingUpdate;
        set
        {
            if (isCheckingUpdate == value)
                return;
            this.RaiseAndSetIfChanged(ref isCheckingUpdate, value);
            this.RaisePropertyChanged(nameof(IsBusy));
        }
    }
    public bool IsBusy => IsLoading || IsCheckingUpdate;
    public bool IsUpdate => Model.IsUpdate;
    public string? NewVersion => Model.NewVersion;
    public string NewVersionBadge => Strings.Script_NewVersionBadgeFormat.Format(ScriptVersionComparer.TrimVersionPrefix(NewVersion));
    public string UpdateButtonTooltip => string.IsNullOrWhiteSpace(NewVersion) ? Strings.Script_UpdateNow
        : Strings.Script_UpdateToVersionFormat.Format(ScriptVersionComparer.TrimVersionPrefix(NewVersion));
    public ScriptSourceKind SourceKind => ScriptUrlResolver.Classify(Model);
    public string SourceLabel => ScriptExternalLinks.GetSourceLabel(SourceKind);
    public bool IsWorkshopSource => SourceKind == ScriptSourceKind.Workshop;
    public bool IsExternalMarketSource => SourceKind is ScriptSourceKind.ScriptCat or ScriptSourceKind.GreasyFork;
    public int MatchRuleCount => Model.MatchDomainNamesArray.Length;
    public string MatchSummary => MatchRuleCount == 0 ? Strings.Script_NoMatchRules
        : Strings.Script_MatchRulesSummaryFormat.Format(MatchRuleCount);
    public string MatchTooltip => MatchRuleCount == 0 ? MatchSummary : string.Join(Environment.NewLine, Model.MatchDomainNamesArray);
    public bool HasSourceLink => ScriptUrlResolver.TryGetHttpUri(SourceLink, out _);
    public string SourceLink => Model.SourceLink ?? string.Empty;
    public bool HasRemoteUpdateLink => Model.Id.HasValue || ScriptUrlResolver.TryGetHttpUri(Model.UpdateLink, out _)
        || ScriptUrlResolver.TryGetHttpUri(Model.DownloadLink, out _) || ScriptUrlResolver.CanResolveScriptLink(SourceLink);
    public string CopyableLink => new[] { Model.UpdateLink, Model.DownloadLink, Model.SourceLink }
        .FirstOrDefault(url => ScriptUrlResolver.TryGetHttpUri(url, out _)) ?? string.Empty;
    public bool HasCopyableLink => CopyableLink.Length != 0;
    public string UpdateStatusText
    {
        get => updateStatusText;
        set
        {
            this.RaiseAndSetIfChanged(ref updateStatusText, value);
            this.RaisePropertyChanged(nameof(HasUpdateStatusText));
        }
    }
    public bool HasUpdateStatusText => !string.IsNullOrWhiteSpace(UpdateStatusText);

    void ApplyUpdateResult(ScriptUpdateCheckResult result)
    {
        if (!ReferenceEquals(result.Script, Model))
            return;

        UpdateStatusText = result.Status switch
        {
            ScriptUpdateCheckStatus.UpdateAvailable => Strings.Script_UpdateStatus_Updatable.Format(
                ScriptVersionComparer.TrimVersionPrefix(result.RemoteVersion)),
            ScriptUpdateCheckStatus.UpToDate => Strings.Script_UpdateStatus_Latest,
            ScriptUpdateCheckStatus.NoUpdateUrl => Strings.Script_UpdateStatus_NoRemoteUrl,
            _ => Strings.Script_UpdateStatus_Failed,
        };
        NotifyVersionChanged();
    }

    void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ScriptDTO.Disable):
                this.RaisePropertyChanged(nameof(IsEnabled));
                break;
            case nameof(ScriptDTO.IsLoading):
                this.RaisePropertyChanged(nameof(IsLoading));
                this.RaisePropertyChanged(nameof(IsBusy));
                break;
            case nameof(ScriptDTO.IsUpdate):
                NotifyVersionChanged();
                break;
        }
    }

    void NotifyVersionChanged()
    {
        this.RaisePropertyChanged(nameof(IsUpdate));
        this.RaisePropertyChanged(nameof(NewVersion));
        this.RaisePropertyChanged(nameof(NewVersionBadge));
        this.RaisePropertyChanged(nameof(UpdateButtonTooltip));
    }

    public void Dispose()
    {
        Model.PropertyChanged -= OnModelPropertyChanged;
        service.ScriptUpdateResultChanged -= ApplyUpdateResult;
    }
}
