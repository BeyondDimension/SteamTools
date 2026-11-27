using System.Collections.Concurrent;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.Services;

partial class ProxyService
{
    readonly SemaphoreSlim scriptOperation = new(1, 1);
    readonly CompositeDisposable scriptSubscriptions = new();
    readonly ConcurrentDictionary<int, ScriptUpdateCheckResult> scriptUpdateResults = new();
    IReadOnlyList<ScriptDTO>? activeProxyScripts;
    long scriptConfigurationRevision;
    long activeScriptConfigurationRevision;

    bool isScriptConfigDirtyWhileProxyRunning;
    public bool IsScriptConfigDirtyWhileProxyRunning
    {
        get => isScriptConfigDirtyWhileProxyRunning;
        private set => this.RaiseAndSetIfChanged(ref isScriptConfigDirtyWhileProxyRunning, value);
    }

    internal event Action<ScriptUpdateCheckResult>? ScriptUpdateResultChanged;

    void ObserveScriptConfiguration()
    {
        scriptSubscriptions.Add(ProxyScripts.Connect()
            .MergeMany(item => item.ObserveValue(nameof(item.Disable), static script => script.Disable).Skip(1).Select(_ => item))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Do(_ => MarkScriptConfigurationChanged())
            .Select(item => Observable.FromAsync(async () =>
            {
                await scriptOperation.WaitAsync();
                try
                {
                    if (ReferenceEquals(FindInstalledScript(item), item))
                        await scriptManager.SaveEnableScriptAsync(item);
                }
                catch (Exception ex)
                {
                    ex.LogAndShowT();
                }
                finally
                {
                    scriptOperation.Release();
                }
            }))
            .Concat()
            .Subscribe());

        scriptSubscriptions.Add(ProxyScripts.Connect().ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => MarkScriptConfigurationChanged()));
        scriptSubscriptions.Add(this.ObserveValue(nameof(ProxyStatus), static service => service.ProxyStatus)
            .DistinctUntilChanged().Subscribe(running =>
            {
                if (!running)
                    activeProxyScripts = null;
                IsScriptConfigDirtyWhileProxyRunning = running
                    && Interlocked.Read(ref scriptConfigurationRevision) != activeScriptConfigurationRevision;
                this.RaisePropertyChanged(nameof(EnableProxyScripts));
            }));
        scriptSubscriptions.Add(ProxySettings.IsEnableScript.Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(IsEnableScript));
            MarkScriptConfigurationChanged();
        }, false));
        scriptSubscriptions.Add(ProxySettings.OnlyEnableProxyScript.Subscribe(_ => MarkScriptConfigurationChanged(), false));
        scriptSubscriptions.Add(ProxySettings.ScriptInjectScopeMode.Subscribe(_ => MarkScriptConfigurationChanged(), false));
        scriptSubscriptions.Add(ProxySettings.ScriptUserAgentPresets.Subscribe(_ => MarkScriptConfigurationChanged(), false));
        scriptSubscriptions.Add(ProxySettings.ScriptCustomUserAgents.Subscribe(_ => MarkScriptConfigurationChanged(), false));
    }

    void MarkScriptConfigurationChanged()
    {
        Interlocked.Increment(ref scriptConfigurationRevision);
        if (ProxyStatus)
            IsScriptConfigDirtyWhileProxyRunning = true;
    }

    public async Task InitializeScriptAsync()
    {
        ProxySettings.MigrateScriptScope();

        await scriptOperation.WaitAsync();
        try
        {
            var scripts = await scriptManager.GetAllScriptAsync();
            var validated = await scriptManager.CheckFiles(scripts);
            await MainThread2.InvokeOnMainThreadAsync(() => ReplaceScriptList(validated));
        }
        finally
        {
            scriptOperation.Release();
        }

        await BasicsInfoAsync();
        if (ProxySettings.IsAutoCheckScriptUpdate.Value)
            _ = CheckInitialScriptUpdatesAsync();
    }

    async Task CheckInitialScriptUpdatesAsync()
    {
        try
        {
            await CheckScriptUpdatesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(nameof(ProxyService), ex, "Script update check failed during startup.");
        }
    }

    public async Task BasicsInfoAsync()
    {
        var basics = ProxyScripts.Items.Where(script => script.Id == ScriptExternalLinks.BasicsScriptId).ToArray();
        if (basics.Length == 1)
            return;
        foreach (var duplicate in basics)
            await DeleteScriptAsync(duplicate);

        var response = await IMicroServiceClient.Instance.Script.GM(Strings.Script_UpdateError);
        if (response.IsSuccess && response.Content != null)
            await UpdateScriptAsync(response.Content);
    }

    public async Task RefreshScript()
    {
        await scriptOperation.WaitAsync();
        try
        {
            var scripts = await scriptManager.GetAllScriptAsync();
            await MainThread2.InvokeOnMainThreadAsync(() => ReplaceScriptList(scripts));
        }
        finally
        {
            scriptOperation.Release();
        }
    }

    void ReplaceScriptList(IEnumerable<ScriptDTO> scripts)
    {
        var previous = ProxyScripts.Items.ToDictionary(script => script.LocalId);
        var updated = scripts.Select(script =>
        {
            if (previous.TryGetValue(script.LocalId, out var current) && SameScript(current, script))
                return current;

            scriptUpdateResults.TryRemove(script.LocalId, out _);
            return script;
        }).ToArray();
        foreach (var removed in previous.Keys.Except(updated.Select(script => script.LocalId)))
            scriptUpdateResults.TryRemove(removed, out _);

        if (ProxyScripts.Items.SequenceEqual(updated))
            return;

        ProxyScripts.Edit(list =>
        {
            list.Clear();
            list.AddRange(updated);
        });
    }

    static bool SameScript(ScriptDTO a, ScriptDTO b)
        => a.Id == b.Id && a.Version == b.Version && a.FilePath == b.FilePath && a.CachePath == b.CachePath
            && a.Disable == b.Disable && a.Order == b.Order && a.IsCompile == b.IsCompile
            && a.Name == b.Name && a.AuthorName == b.AuthorName && a.Describe == b.Describe && a.IconUrl == b.IconUrl
            && a.DownloadLink == b.DownloadLink && a.UpdateLink == b.UpdateLink && a.SourceLink == b.SourceLink
            && a.MatchDomainNames == b.MatchDomainNames && a.ExcludeDomainNames == b.ExcludeDomainNames
            && a.RequiredJs == b.RequiredJs;

    ScriptDTO? FindInstalledScript(ScriptDTO script)
        => ProxyScripts.Items.FirstOrDefault(item => script.LocalId > 0 && item.LocalId == script.LocalId)
        ?? (script.Id.HasValue ? ProxyScripts.Items.FirstOrDefault(item => item.Id == script.Id) : null);

    void UpsertScript(ScriptDTO script)
    {
        scriptUpdateResults.TryRemove(script.LocalId, out _);
        var current = FindInstalledScript(script);
        if (current == null)
            ProxyScripts.Add(script);
        else
            ProxyScripts.Replace(current, script);
    }

    public async Task<IApiRsp<ScriptDTO?>> InstallInspectedScriptAsync(ScriptInspectionResult inspection,
        bool enable = true, bool isCompile = true, CancellationToken cancellationToken = default)
    {
        await scriptOperation.WaitAsync(cancellationToken);
        try
        {
            if (inspection.ExistingScript is { LocalId: > 0 } existing)
            {
                var current = FindInstalledScript(existing);
                if (current == null || current.Version != existing.Version || current.FilePath != existing.FilePath)
                    return ApiRspHelper.Fail<ScriptDTO?>(Strings.Refresh);
                inspection = inspection with { ExistingScript = current };
            }

            var result = await scriptManager.InstallInspectedScriptAsync(inspection, enable, isCompile, cancellationToken);
            if (result.IsSuccess && result.Content != null)
                await MainThread2.InvokeOnMainThreadAsync(() => UpsertScript(result.Content));
            return result;
        }
        finally
        {
            scriptOperation.Release();
        }
    }

    public async Task<IApiRsp<ScriptDTO?>> RefreshInstalledScriptAsync(ScriptDTO script)
    {
        await scriptOperation.WaitAsync();
        try
        {
            var current = FindInstalledScript(script);
            if (current == null)
                return ApiRspHelper.Fail<ScriptDTO?>(Strings.Script_NoFile_.Format(script.Name));

            var path = Path.Combine(Plugin.Instance.AppDataDirectory, current.FilePath);
            var result = await scriptManager.AddScriptAsync(path, current, current.IsCompile, ignoreCache: true);
            if (result.IsSuccess && result.Content != null)
                await MainThread2.InvokeOnMainThreadAsync(() => UpsertScript(result.Content));
            return result;
        }
        finally
        {
            scriptOperation.Release();
        }
    }

    public async Task<IApiRsp> DeleteScriptAsync(ScriptDTO script)
    {
        await scriptOperation.WaitAsync();
        try
        {
            var current = FindInstalledScript(script);
            if (current == null)
                return ApiRspHelper.Ok(Strings.Script_DeleteSuccess);

            var result = await scriptManager.DeleteScriptAsync(current);
            if (result.IsSuccess)
            {
                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    scriptUpdateResults.TryRemove(current.LocalId, out _);
                    ProxyScripts.Remove(current);
                });
            }
            return result;
        }
        finally
        {
            scriptOperation.Release();
        }
    }

    public async void DownloadScript(ScriptDTO model)
    {
        var updated = await UpdateScriptAsync(model);
        Toast.Show(updated ? ToastIcon.Success : ToastIcon.Error,
            updated ? Strings.Download_ScriptOk : Strings.Script_UpdateStatus_Failed);
    }

    public async Task<bool> UpdateScriptAsync(ScriptDTO model, CancellationToken cancellationToken = default)
    {
        await scriptOperation.WaitAsync(cancellationToken);
        ScriptDTO? current = null;
        try
        {
            current = FindInstalledScript(model);
            var isNewWorkshopScript = model.Id.HasValue && model.LocalId == 0 && !model.IsExist;
            if (current == null && !isNewWorkshopScript)
                return false;

            await MainThread2.InvokeOnMainThreadAsync(() =>
            {
                model.IsLoading = true;
                if (current != null)
                    current.IsLoading = true;
            });

            var check = current == null ? null : GetScriptUpdateResult(current);
            if (current != null && check is not { Status: ScriptUpdateCheckStatus.UpdateAvailable })
                check = await CheckSingleScriptUpdateAsync(current, cancellationToken);
            if (check is { Status: not ScriptUpdateCheckStatus.UpdateAvailable })
                return false;

            var urls = new[] { check?.DownloadUrl, model.DownloadLink, check?.UpdateUrl, model.UpdateLink,
                ScriptUrlResolver.CanResolveScriptLink(model.SourceLink) ? model.SourceLink : null };
            foreach (var url in urls.Distinct(StringComparer.Ordinal))
            {
                if (!ScriptUrlResolver.TryGetHttpUri(url, out _))
                    continue;
                var inspected = await scriptManager.InspectScriptFromUrlAsync(url!, cancellationToken: cancellationToken);
                if (!inspected.IsSuccess || inspected.Content is not { HasUserScriptHeader: true } inspection)
                    continue;

                if (current?.Id == null && current != null &&
                    ScriptVersionComparer.Compare(inspection.Version, current.Version) <= 0)
                    continue;

                var original = current ?? model;
                if (original.Id.HasValue)
                {
                    var version = check?.RemoteVersion ?? model.NewVersion ?? model.Version;
                    var updateUrl = check?.UpdateUrl ?? model.UpdateLink;
                    inspection = inspection with
                    {
                        Version = string.IsNullOrWhiteSpace(version) ? inspection.Version : version,
                        UpdateUrl = string.IsNullOrWhiteSpace(updateUrl) ? inspection.UpdateUrl : updateUrl,
                    };
                }
                else if (string.IsNullOrWhiteSpace(inspection.UpdateUrl) && !string.IsNullOrWhiteSpace(check?.UpdateUrl))
                {
                    inspection = inspection with { UpdateUrl = check.UpdateUrl };
                }
                inspection = inspection with { ExistingScript = original, PreserveExistingEnable = true };
                var result = await scriptManager.InstallInspectedScriptAsync(inspection, !original.Disable,
                    original.Id != ScriptExternalLinks.BasicsScriptId && original.IsCompile, cancellationToken);
                if (!result.IsSuccess || result.Content == null)
                    return false;

                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    UpsertScript(result.Content);
                    model.LocalId = result.Content.LocalId;
                    if (!ReferenceEquals(model, current))
                        model.Version = result.Content.Version;
                    model.NewVersion = null;
                    model.IsUpdate = false;
                    model.IsExist = true;
                });
                return true;
            }

            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            Log.Error(nameof(ProxyService), ex, "Unable to update script.");
            return false;
        }
        finally
        {
            try
            {
                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    model.IsLoading = false;
                    if (current != null)
                        current.IsLoading = false;
                });
            }
            finally
            {
                scriptOperation.Release();
            }
        }
    }

    public async Task<ScriptBatchUpdateCheckResult> CheckScriptUpdatesAsync(bool showToast = false,
        CancellationToken cancellationToken = default)
    {
        var result = await scriptManager.CheckAllScriptUpdatesAsync(ProxyScripts.Items.ToArray(), cancellationToken);
        await MainThread2.InvokeOnMainThreadAsync(() =>
        {
            foreach (var item in result.Results)
                ApplyScriptUpdateResult(item);

            if (showToast)
                Toast.Show(result.FailedCount == 0 ? ToastIcon.Info : ToastIcon.Warning, GetScriptUpdateSummary(result));
        });
        return result;
    }

    public async Task<ScriptUpdateCheckResult> CheckSingleScriptUpdateAsync(ScriptDTO script,
        CancellationToken cancellationToken = default)
    {
        var result = await scriptManager.CheckScriptUpdateAsync(FindInstalledScript(script) ?? script, cancellationToken);
        await MainThread2.InvokeOnMainThreadAsync(() => ApplyScriptUpdateResult(result));
        return result;
    }

    internal static string GetScriptUpdateSummary(ScriptBatchUpdateCheckResult result)
        => result.FailedCount > 0 ? Strings.Script_UpdateCheckSummary_PartialFail.Format(result.UpdateAvailableCount, result.FailedCount)
        : result.CheckedCount == 0 ? Strings.Script_UpdateCheckSummary_NoUpdatable
        : result.UpdateAvailableCount > 0 ? Strings.Script_UpdateCheckSummary_Found.Format(result.UpdateAvailableCount)
        : Strings.Script_UpdateCheckSummary_AllLatest;

    internal ScriptUpdateCheckResult? GetScriptUpdateResult(ScriptDTO script)
        => scriptUpdateResults.TryGetValue(script.LocalId, out var result) && ReferenceEquals(result.Script, script)
            ? result : null;

    void ApplyScriptUpdateResult(ScriptUpdateCheckResult result)
    {
        var script = FindInstalledScript(result.Script);
        if (!ReferenceEquals(script, result.Script))
            return;

        scriptUpdateResults[script!.LocalId] = result;
        script.NewVersion = result.Status == ScriptUpdateCheckStatus.UpdateAvailable ? result.RemoteVersion : null;
        script.IsUpdate = result.Status == ScriptUpdateCheckStatus.UpdateAvailable;
        ScriptUpdateResultChanged?.Invoke(result);
    }
}
