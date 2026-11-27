using BD.WTTS.Client.Resources;

namespace BD.WTTS.Services.Implementation;

partial class ScriptManager
{
    public async Task<IApiRsp<ScriptInspectionResult?>> InspectScriptFromUrlAsync(string url,
        IEnumerable<ScriptDTO>? existingScripts = null, CancellationToken cancellationToken = default)
    {
        if (!ScriptUrlResolver.TryGetHttpUri(url, out _))
            return ApiRspHelper.Fail<ScriptInspectionResult?>(Strings.Script_Inspect_Warn_InvalidUrl);

        try
        {
            // The factory owns and may share the client and its handler.
            var client = CreateClient(null, HttpHandlerCategory.Default);
            var resolver = new ScriptUrlResolver(client);
            var resolved = await resolver.ResolveAsync(url, cancellationToken);
            var raw = await ReadRemoteScriptAsync(client, resolved.DownloadUrl, cancellationToken);
            if (ScriptUrlResolver.IsMetadataUrl(resolved.DownloadUrl))
            {
                var metadata = ScriptMetadataParser.Parse(raw);
                var download = ScriptUrlResolver.TryGetHttpUri(metadata.DownloadUrl, out _)
                    ? metadata.DownloadUrl : ScriptUrlResolver.GetCodeUrl(resolved.DownloadUrl);
                var fullScript = await resolver.ResolveAsync(download, cancellationToken);
                if (ScriptUrlResolver.IsMetadataUrl(fullScript.DownloadUrl))
                    return ApiRspHelper.Fail<ScriptInspectionResult?>(Strings.Script_Inspect_Warn_InvalidUrl);

                raw = await ReadRemoteScriptAsync(client, fullScript.DownloadUrl, cancellationToken);
                resolved = resolved with { DownloadUrl = fullScript.DownloadUrl };
                var inspection = ScriptMetadataParser.Parse(raw, resolvedUrl: resolved, existingScripts: existingScripts);
                return ApiRspHelper.Ok<ScriptInspectionResult?>(inspection with
                {
                    UpdateUrl = PreferMetadata(inspection.UpdateUrl, url.Trim())!,
                });
            }

            return ApiRspHelper.Ok<ScriptInspectionResult?>(
                ScriptMetadataParser.Parse(raw, resolvedUrl: resolved, existingScripts: existingScripts));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Unable to inspect script URL");
            return ApiRspHelper.Fail<ScriptInspectionResult?>(Strings.Script_Inspect_Warn_FetchFailed.Format(ex.Message));
        }
    }

    public Task<IApiRsp<ScriptInspectionResult?>> InspectScriptFromFileAsync(string filePath,
        IEnumerable<ScriptDTO>? existingScripts = null, CancellationToken cancellationToken = default)
        => InspectScriptFileAsync(filePath, existingScripts, null, cancellationToken);

    public Task<IApiRsp<ScriptInspectionResult?>> InspectInstalledScriptAsync(ScriptDTO script,
        CancellationToken cancellationToken = default)
        => InspectScriptFileAsync(Path.Combine(Plugin.Instance.AppDataDirectory, script.FilePath), null, script, cancellationToken);

    async Task<IApiRsp<ScriptInspectionResult?>> InspectScriptFileAsync(string filePath,
        IEnumerable<ScriptDTO>? existingScripts, ScriptDTO? installedScript, CancellationToken cancellationToken)
    {
        try
        {
            var raw = await File.ReadAllTextAsync(filePath, cancellationToken);
            return ApiRspHelper.Ok<ScriptInspectionResult?>(ScriptMetadataParser.Parse(raw, filePath,
                existingScripts: existingScripts, installedScript: installedScript));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Unable to inspect script file");
            return ApiRspHelper.Fail<ScriptInspectionResult?>(Strings.Script_ReadFileError_.Format(filePath));
        }
    }

    public Task<IApiRsp<ScriptDTO?>> InstallInspectedScriptAsync(ScriptInspectionResult inspection,
        bool enable = true, bool isCompile = true, CancellationToken cancellationToken = default)
    {
        if (!inspection.HasUserScriptHeader)
            return Task.FromResult<IApiRsp<ScriptDTO?>>(ApiRspHelper.Fail<ScriptDTO?>(Strings.Script_Inspect_Warn_NoHeaderDetail));

        return SaveScriptCoreAsync(inspection.SourceFilePath ?? inspection.DownloadUrl,
            inspection.RawContent, inspection.ToScriptDto(enable, isCompile), inspection.ExistingScript, isCompile,
            ignoreCache: inspection.ExistingScript is { LocalId: > 0 }, preserveEnable: inspection.PreserveExistingEnable,
            cancellationToken: cancellationToken);
    }

    public async Task<ScriptUpdateCheckResult> CheckScriptUpdateAsync(ScriptDTO script,
        CancellationToken cancellationToken = default)
    {
        if (script.Id == null)
            return await CheckExternalUpdateAsync(script, cancellationToken);

        var (workshop, error) = await ReadWorkshopVersionsAsync([script.Id.Value], cancellationToken);
        return CompareWorkshopVersion(script, workshop, error);
    }

    public async Task<ScriptBatchUpdateCheckResult> CheckAllScriptUpdatesAsync(IEnumerable<ScriptDTO> scripts,
        CancellationToken cancellationToken = default)
    {
        var items = scripts.ToArray();
        var ids = items.Select(script => script.Id).OfType<Guid>().Distinct().ToArray();
        var (workshop, error) = await ReadWorkshopVersionsAsync(ids, cancellationToken);
        var results = new ScriptUpdateCheckResult[items.Length];

        await Parallel.ForEachAsync(Enumerable.Range(0, items.Length),
            new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 4 },
            async (index, token) =>
            {
                var script = items[index];
                results[index] = script.Id.HasValue
                    ? CompareWorkshopVersion(script, workshop, error)
                    : await CheckExternalUpdateAsync(script, token);
            });

        var skipped = results.Count(result => result.Status == ScriptUpdateCheckStatus.NoUpdateUrl);
        return new(results.Length - skipped, results.Count(result => result.Status == ScriptUpdateCheckStatus.UpdateAvailable),
            results.Count(result => result.Status == ScriptUpdateCheckStatus.Failed), skipped, results);
    }

    async Task<(IReadOnlyDictionary<Guid, ScriptInfoDTO> Versions, string? Error)> ReadWorkshopVersionsAsync(
        Guid[] ids, CancellationToken cancellationToken)
    {
        if (ids.Length == 0)
            return (new Dictionary<Guid, ScriptInfoDTO>(), null);

        try
        {
            var response = await csc.Script.GetInfoByIds(Strings.Script_UpdateError, ids).WaitAsync(cancellationToken);
            if (response.IsSuccess && response.Content != null)
                return (response.Content.GroupBy(item => item.Id).ToDictionary(group => group.Key, group => group.First()), null);

            return (new Dictionary<Guid, ScriptInfoDTO>(), response.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Unable to check Workshop script versions");
            return (new Dictionary<Guid, ScriptInfoDTO>(), ex.Message);
        }
    }

    static ScriptUpdateCheckResult CompareWorkshopVersion(ScriptDTO local,
        IReadOnlyDictionary<Guid, ScriptInfoDTO> versions, string? error)
    {
        if (!versions.TryGetValue(local.Id!.Value, out var remote) || string.IsNullOrWhiteSpace(remote.Version))
            return new(local, ScriptUpdateCheckStatus.Failed, ErrorMessage: error ?? Strings.Script_UpdateStatus_Failed);

        var updateLink = PreferMetadata(remote.UpdateLink, local.UpdateLink);
        var download = ScriptUrlResolver.TryGetHttpUri(updateLink, out _) ? ScriptUrlResolver.GetCodeUrl(updateLink!) : local.DownloadLink;
        return CompareVersion(local, remote.Version, download, updateLink);
    }

    async Task<ScriptUpdateCheckResult> CheckExternalUpdateAsync(ScriptDTO script, CancellationToken cancellationToken)
    {
        var candidates = new[] { script.UpdateLink,
            ScriptUrlResolver.CanResolveScriptLink(script.SourceLink) ? script.SourceLink : null, script.DownloadLink };
        var urls = candidates.Where(url => ScriptUrlResolver.TryGetHttpUri(url, out _)).Distinct(StringComparer.Ordinal);
        var client = CreateClient(null, HttpHandlerCategory.Default);
        var resolver = new ScriptUrlResolver(client);
        string? error = null;
        foreach (var url in urls)
        {
            try
            {
                var resolved = await resolver.ResolveAsync(url!, cancellationToken);
                var raw = await ReadRemoteScriptAsync(client, resolved.DownloadUrl, cancellationToken);
                var remote = ScriptMetadataParser.Parse(raw);
                if (!remote.HasUserScriptHeader || string.IsNullOrWhiteSpace(remote.Version))
                {
                    error = Strings.Script_UpdateStatus_Failed;
                    continue;
                }

                var fromUpdateUrl = url == script.UpdateLink;
                var download = new[] { remote.DownloadUrl, fromUpdateUrl ? script.DownloadLink : null, resolved.DownloadUrl }
                    .First(candidate => ScriptUrlResolver.TryGetHttpUri(candidate, out _)
                        && (candidate == resolved.DownloadUrl || !ScriptUrlResolver.IsMetadataUrl(candidate)));
                return CompareVersion(script, remote.Version, ScriptUrlResolver.GetCodeUrl(download!),
                    PreferMetadata(remote.UpdateUrl, fromUpdateUrl ? url : script.UpdateLink));
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Unable to check script {ScriptId}", script.LocalId);
                error = ex.Message;
            }
        }
        return error == null
            ? new(script, ScriptUpdateCheckStatus.NoUpdateUrl)
            : new(script, ScriptUpdateCheckStatus.Failed, ErrorMessage: error);
    }

    static ScriptUpdateCheckResult CompareVersion(ScriptDTO local, string remoteVersion, string? download, string? update)
        => new(local, ScriptVersionComparer.Compare(remoteVersion, local.Version) > 0
            ? ScriptUpdateCheckStatus.UpdateAvailable : ScriptUpdateCheckStatus.UpToDate, remoteVersion, download, update);

    Task<string> ReadRemoteScriptAsync(string url, CancellationToken cancellationToken)
    {
        var client = CreateClient(null, HttpHandlerCategory.Default);
        return ReadRemoteScriptAsync(client, url, cancellationToken);
    }

    async Task<string> ReadRemoteScriptAsync(HttpClient client, string url, CancellationToken cancellationToken)
    {
        if (!ScriptUrlResolver.TryGetHttpUri(url, out var uri))
            throw new ArgumentException(Strings.Script_Inspect_Warn_InvalidUrl, nameof(url));

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.TryAddWithoutValidation("User-Agent", http_helper.UserAgent);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
