using System.ComponentModel;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UI.ViewModels;

public sealed class ScriptStoreItemViewModel : ReactiveObject, IDisposable
{
    readonly string catalogVersion;

    public ScriptStoreItemViewModel(ScriptDTO model)
    {
        Model = model;
        catalogVersion = model.Version;
        Model.PropertyChanged += OnModelPropertyChanged;
    }

    public ScriptDTO Model { get; }
    public string Name => Model.Name;
    public string AuthorName => string.IsNullOrWhiteSpace(Model.AuthorName) ? Strings.Script_Inspect_None : Model.AuthorName;
    public string VersionDisplay => ScriptVersionComparer.FormatVersionDisplay(Model.Version);
    public string Describe => Model.Describe ?? string.Empty;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Describe);
    public string? IconUrl => Model.IconUrl;
    public bool IsInstalled => Model.IsExist;
    public bool IsUpdate => Model.IsUpdate;
    public bool IsDownloading => Model.IsLoading;
    public string NewVersionBadge => Strings.Script_NewVersionBadgeFormat.Format(ScriptVersionComparer.TrimVersionPrefix(Model.NewVersion));
    public string UpdateButtonTooltip => string.IsNullOrWhiteSpace(Model.NewVersion) ? Strings.Script_UpdateNow
        : Strings.Script_UpdateToVersionFormat.Format(ScriptVersionComparer.TrimVersionPrefix(Model.NewVersion));
    public bool ShowInstalledBadge => IsInstalled && !IsUpdate && !IsDownloading;
    public bool ShowGetButton => !IsInstalled && !IsDownloading;
    public bool ShowUpdateButton => IsInstalled && IsUpdate && !IsDownloading;
    public int MatchRuleCount => Model.MatchDomainNamesArray.Length;
    public bool HasMatchSummary => MatchRuleCount > 0;
    public string MatchSummary => HasMatchSummary ? Strings.Script_MatchRulesSummaryFormat.Format(MatchRuleCount)
        : Strings.Script_NoMatchRules;
    public string MatchTooltip => HasMatchSummary ? string.Join(Environment.NewLine, Model.MatchDomainNamesArray) : MatchSummary;
    public string SourceLink => Model.SourceLink ?? string.Empty;
    public bool HasSourceLink => ScriptUrlResolver.TryGetHttpUri(SourceLink, out _);

    internal void SynchronizeInstalledState(ScriptDTO? installed)
    {
        // The catalog version remains available when the row shows the installed version.
        var isUpdate = installed != null && ScriptVersionComparer.Compare(catalogVersion, installed.Version) > 0;
        var newVersion = isUpdate ? catalogVersion : null;
        var versionChanged = Model.NewVersion != newVersion;
        var updateChanged = Model.IsUpdate != isUpdate;

        Model.LocalId = installed?.LocalId ?? 0;
        Model.NewVersion = newVersion;
        Model.Version = installed?.Version ?? catalogVersion;
        Model.IsExist = installed != null;
        Model.IsUpdate = isUpdate;

        // ScriptDTO currently stores NewVersion without a change notification.
        if (versionChanged && !updateChanged)
            NotifyUpdateVersion();
    }

    void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ScriptDTO.IsExist):
                this.RaisePropertyChanged(nameof(IsInstalled));
                NotifyActionState();
                break;
            case nameof(ScriptDTO.IsUpdate):
                this.RaisePropertyChanged(nameof(IsUpdate));
                NotifyUpdateVersion();
                NotifyActionState();
                break;
            case nameof(ScriptDTO.IsLoading):
                this.RaisePropertyChanged(nameof(IsDownloading));
                NotifyActionState();
                break;
            case nameof(ScriptDTO.Version):
                this.RaisePropertyChanged(nameof(VersionDisplay));
                break;
            case nameof(ScriptDTO.NewVersion):
                NotifyUpdateVersion();
                break;
        }
    }

    void NotifyActionState()
    {
        this.RaisePropertyChanged(nameof(ShowInstalledBadge));
        this.RaisePropertyChanged(nameof(ShowGetButton));
        this.RaisePropertyChanged(nameof(ShowUpdateButton));
    }

    void NotifyUpdateVersion()
    {
        this.RaisePropertyChanged(nameof(NewVersionBadge));
        this.RaisePropertyChanged(nameof(UpdateButtonTooltip));
    }

    public void Dispose() => Model.PropertyChanged -= OnModelPropertyChanged;
}
