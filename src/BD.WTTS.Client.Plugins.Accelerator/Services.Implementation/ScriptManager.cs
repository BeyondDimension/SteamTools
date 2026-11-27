using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IScriptManager"/>
public sealed partial class ScriptManager : GeneralHttpClientFactory, IScriptManager
{
    const string TAG = nameof(ScriptManager);
    readonly SemaphoreSlim scriptWriteLock = new(1, 1);

    protected override string? DefaultClientName => TAG;

    readonly IToast toast;
    readonly IScriptRepository scriptRepository;
    readonly IMicroServiceClient csc;

    public ScriptManager(
        IHttpClientFactory clientFactory,
        IScriptRepository scriptRepository,
        ILoggerFactory loggerFactory,
        IToast toast,
        IHttpPlatformHelperService http_helper,
        IMicroServiceClient csc) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper, clientFactory)
    {
        this.scriptRepository = scriptRepository;
        this.toast = toast;
        this.csc = csc;
    }

    public async Task<ScriptDTO?> ReadScriptAsync(string path)
    {
        var inspection = ScriptMetadataParser.Parse(await File.ReadAllTextAsync(path), path);
        return inspection.HasUserScriptHeader ? inspection.ToScriptDto() : null;
    }

    public async Task<IApiRsp<ScriptDTO?>> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool isCompile = true,
        long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
    {
        if (!File.Exists(filePath))
            return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_NoFile_.Format(filePath));

        var rawContent = await File.ReadAllTextAsync(filePath);
        var inspection = ScriptMetadataParser.Parse(rawContent, filePath);
        var info = inspection.HasUserScriptHeader ? inspection.ToScriptDto() : null;
        return await SaveScriptCoreAsync(filePath, rawContent, info, oldInfo, isCompile, order, deleteFile, pid, ignoreCache);
    }

    public async Task<IApiRsp<ScriptDTO?>> SaveScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null,
        bool isCompile = true, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
    {
        if (!fileInfo.Exists)
            return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_NoFile_.Format(fileInfo.FullName));

        var content = await File.ReadAllTextAsync(fileInfo.FullName);
        return await SaveScriptCoreAsync(fileInfo.FullName, content, info, oldInfo, isCompile, order, deleteFile, pid, ignoreCache);
    }

    async Task<IApiRsp<ScriptDTO?>> SaveScriptCoreAsync(string source, string rawContent, ScriptDTO? info, ScriptDTO? oldInfo,
        bool isCompile, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false,
        bool preserveEnable = true, CancellationToken cancellationToken = default)
    {
        if (info == null || string.IsNullOrEmpty(info.Content))
            return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_ReadFileError_.Format(source));

        await scriptWriteLock.WaitAsync(cancellationToken);
        string? stagedCache = null;
        string? stagedSource = null;
        string? newSource = null;
        string? newCache = null;
        try
        {
            var md5 = Hashs.String.MD5(rawContent);
            var sha512 = Hashs.String.SHA512(rawContent);
            if (oldInfo is { LocalId: > 0 }
                && await scriptRepository.FirstOrDefaultAsync(row => row.Id == oldInfo.LocalId, cancellationToken) == null)
                return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_NoFile_.Format(oldInfo.Name));

            if (!ignoreCache && await scriptRepository.ExistsScriptAsync(md5, sha512, cancellationToken))
                return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_FileRepeat);

            info.Id = pid ?? oldInfo?.Id ?? info.Id;
            info.IsBasics = info.Id == ScriptExternalLinks.BasicsScriptId;
            info.IsCompile = isCompile;
            if (oldInfo != null)
            {
                info.LocalId = oldInfo.LocalId;
                info.IconUrl = PreferMetadata(info.IconUrl, oldInfo.IconUrl);
                info.SourceLink = PreferMetadata(info.SourceLink, oldInfo.SourceLink) ?? string.Empty;
                info.DownloadLink = PreferMetadata(info.DownloadLink, oldInfo.DownloadLink) ?? string.Empty;
                info.UpdateLink = PreferMetadata(info.UpdateLink, oldInfo.UpdateLink) ?? string.Empty;
                info.AccelerateProjects = oldInfo.AccelerateProjects;
                info.UpdateTime = oldInfo.UpdateTime;
                if (oldInfo.LocalId == 0 && oldInfo.Id.HasValue)
                {
                    info.Version = PreferMetadata(oldInfo.Version, info.Version) ?? string.Empty;
                    info.Name = PreferMetadata(oldInfo.Name, info.Name) ?? string.Empty;
                    info.Describe = PreferMetadata(oldInfo.Describe, info.Describe);
                }
            }
            info.Order = info.IsBasics ? 1 : Math.Clamp(order ?? oldInfo?.Order ?? 10, int.MinValue, int.MaxValue);
            if (info.Order == 0)
                info.Order = 10;

            var cacheDirectory = Path.Combine(Plugin.Instance.CacheDirectory, IScriptManager.DirName_Build);
            Directory.CreateDirectory(cacheDirectory);
            stagedCache = Path.Combine(cacheDirectory, Guid.NewGuid().ToString("N") + ".tmp");
            if (!await BuildScriptAsync(info, new FileInfo(stagedCache), isCompile, cancellationToken))
                return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_BuildError_.Format(source));

            var cacheName = md5 + "-" + Hashs.String.MD5(info.Content) + FileEx.JS;
            info.FilePath = Path.Combine(IScriptManager.DirName, md5 + FileEx.JS);
            info.CachePath = Path.Combine(IScriptManager.DirName_Build, cacheName);
            var sourcePath = Path.Combine(Plugin.Instance.AppDataDirectory, info.FilePath);
            var cachePath = Path.Combine(cacheDirectory, cacheName);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            if (!File.Exists(sourcePath))
            {
                stagedSource = sourcePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                await File.WriteAllTextAsync(stagedSource, rawContent, cancellationToken);
                File.Move(stagedSource, sourcePath);
                newSource = sourcePath;
            }
            if (!File.Exists(cachePath))
            {
                File.Move(stagedCache, cachePath);
                newCache = cachePath;
            }

            if (preserveEnable && oldInfo is { LocalId: > 0 })
                info.Disable = oldInfo.Disable;

            var entity = ScriptMapper.ToEntity(info);
            entity.MD5 = md5;
            entity.SHA512 = sha512;
            if ((await scriptRepository.InsertOrUpdateAsync(entity, cancellationToken)).rowCount == 0)
                return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_SaveDBError);

            info.LocalId = entity.Id;
            info.IsExist = true;
            newSource = newCache = null; // SQLite now references both files.

            if (oldInfo is { LocalId: > 0 })
                await DeleteUnusedScriptFilesAsync(oldInfo, info.LocalId, info);

            if (deleteFile && !Path.GetFullPath(source).Equals(sourcePath, PathComparison))
                TryDeleteFile(source);

            return ApiRspHelper.Code<ScriptDTO?>(ApiRspCode.OK, AppResources.Script_SaveDbSuccess, info);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var message = AppResources.Script_ReadFileError_.Format(ex.GetAllMessage());
            logger.LogError(ex, "{Message}", message);
            return ApiRspHelper.Code<ScriptDTO?>(ApiRspCode.Fail, message, default, ex);
        }
        finally
        {
            TryDeleteFile(stagedCache);
            TryDeleteFile(stagedSource);
            TryDeleteFile(newSource);
            TryDeleteFile(newCache);
            scriptWriteLock.Release();
        }
    }

    static StringComparison PathComparison => OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    static string? PreferMetadata(string? value, string? fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;

    void TryDeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Unable to remove script file {Path}", path);
        }
    }

    async Task DeleteUnusedScriptFilesAsync(ScriptDTO script, int localId, ScriptDTO? replacement = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(script.CachePath) && script.CachePath != replacement?.CachePath
                && await scriptRepository.FirstOrDefaultAsync(row => row.Id != localId && row.CachePath == script.CachePath) == null)
                TryDeleteFile(Path.Combine(Plugin.Instance.CacheDirectory, script.CachePath));
            if (!string.IsNullOrWhiteSpace(script.FilePath) && script.FilePath != replacement?.FilePath
                && await scriptRepository.FirstOrDefaultAsync(row => row.Id != localId && row.FilePath == script.FilePath) == null)
                TryDeleteFile(Path.Combine(Plugin.Instance.AppDataDirectory, script.FilePath));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to clean up files for script {ScriptId}", localId);
        }
    }

    public Task<bool> BuildScriptAsync(ScriptDTO model, FileInfo fileInfo, bool build = true)
        => BuildScriptAsync(model, fileInfo, build, CancellationToken.None);

    async Task<bool> BuildScriptAsync(ScriptDTO model, FileInfo fileInfo, bool build, CancellationToken cancellationToken)
    {
        try
        {
            var scriptContent = model.Content;
            if (build && model.RequiredJsArray is { Length: > 0 } dependencies)
            {
                var bundle = new StringBuilder();
                bundle.AppendLine("(function () {");
                foreach (var dependency in dependencies)
                {
                    try
                    {
                        var dependencyContent = await ReadRemoteScriptAsync(dependency, cancellationToken);
                        bundle.AppendLine(dependencyContent);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
                    {
                        var message = AppResources.Script_BuildDownloadError__.Format(model.Name, dependency);
                        logger.LogError(ex, "{Message}", message);
                        toast.Show(ToastIcon.Error, message);
                        return false;
                    }
                }
                bundle.AppendLine("try{var jq2 = $.noConflict(true);}catch{};(($, jQuery) => {");
                bundle.AppendLine(scriptContent);
                bundle.AppendLine("})(jq2, jq2)})()");
                scriptContent = bundle.ToString();
            }

            fileInfo.Directory?.Create();
            await File.WriteAllTextAsync(fileInfo.FullName, scriptContent, cancellationToken);
            model.Content = scriptContent;
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "{Message}", AppResources.Script_BuildError_.Format(fileInfo.FullName));
            return false;
        }
    }

    public async Task<IApiRsp> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true)
    {
        if (item.LocalId <= 0)
            return ApiRspHelper.Ok(AppResources.Script_DeleteSuccess);

        await scriptWriteLock.WaitAsync();
        try
        {
            var row = await scriptRepository.FirstOrDefaultAsync(script => script.Id == item.LocalId);
            if (row == null)
                return ApiRspHelper.Ok(AppResources.Script_DeleteSuccess);

            if (removeByDataBase)
                await scriptRepository.DeleteAsync(row.Id);

            await DeleteUnusedScriptFilesAsync(ScriptMapper.ToDto(row), row.Id);
            return ApiRspHelper.Ok(AppResources.Script_DeleteSuccess);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to delete script {ScriptId}", item.LocalId);
            return ApiRspHelper.Fail(AppResources.Script_DeleteError);
        }
        finally
        {
            scriptWriteLock.Release();
        }
    }

    /// <summary>
    /// 版本升级 检查数据库 数据 是否正确 不正确删除重新打包缓存文件
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ScriptDTO>> CheckFiles(IEnumerable<ScriptDTO> list)
    {
        var scripts = list.ToList();
        foreach (var item in list)
        {
            if (!CheckFile(item))
            {
                var temp = await DeleteScriptAsync(item);
                scripts.Remove(item);
                if (temp.IsSuccess)
                {
                    //$"脚本:{item.Name}_文件丢失已删除"
                    Toast.Show(ToastIcon.Warning, Strings.Script_NoFile_.Format(item.Name));
                }
                else
                {
                    //toast.Show($"脚本:{item.Name}_文件丢失，删除失败去尝试手动删除");
                    Toast.Show(ToastIcon.Error, Strings.Script_NoFileDeleteError_.Format(item.Name));
                }
                continue;
            }

            //检查缓存文件夹如果不是  IScriptManager.DirName_Build 替换成 IScriptManager.DirName_Build 开头
            if (!item.CachePath.StartsWith($"{IScriptManager.DirName_Build}{Path.DirectorySeparatorChar}"))
            {
                var oldCachePath = Path.Combine(Plugin.Instance.CacheDirectory, item.CachePath);
                item.CachePath = Path.Combine(IScriptManager.DirName_Build, item.FileName!);
                if (File.Exists(oldCachePath))
                {
                    File.Delete(oldCachePath);
                }
                await TryReadFileAsync(item, true);
                //清理 脚本内容 后续 IPC 传递 插件进程读取。
                //item.Content = string.Empty;
                await scriptRepository.SaveScriptCachePathAsync(item, default);
            }
            else
            {
                var cachePath = Path.Combine(Plugin.Instance.CacheDirectory, item.CachePath);
                var file = new FileInfo(cachePath);
                //检查缓存文件是否为空 为空则 刷新
                if (file.Exists)
                {
                    if (file.Length == 0)
                    {
                        file.Delete();
                        await TryReadFileAsync(item, true);
                        //清理 脚本内容 后续 IPC 传递 插件进程读取。
                        //item.Content = string.Empty;
                    }
                }
                else
                {
                    await TryReadFileAsync(item, true);
                    //item.Content = string.Empty;

                }
            }
        }
        return scripts;
    }

    public bool CheckFile(ScriptDTO item)
    {
        return File.Exists(Path.Combine(Plugin.Instance.AppDataDirectory, item.FilePath));
    }

    /// <summary>
    /// 修改为仅尝试判断文件是否存在
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isReadContent"></param>
    /// <returns></returns>
    public async Task<ScriptDTO> TryReadFileAsync(ScriptDTO item, bool isReadContent = false)
    {
        var cachePath = Path.Combine(Plugin.Instance.CacheDirectory, item.CachePath);
        if (File.Exists(cachePath))
        {
            item.Content = File.ReadAllText(cachePath);
        }
        else
        {
            var fileInfo = new FileInfo(cachePath);
            var infoPath = Path.Combine(Plugin.Instance.AppDataDirectory, item.FilePath);
            if (File.Exists(infoPath))
            {
                item.Content = (await File.ReadAllTextAsync(infoPath))
                    .Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase);
                if (!await BuildScriptAsync(item, fileInfo, item.IsCompile))
                {
                    toast.Show(ToastIcon.Error, AppResources.Script_ReadFileError_.Format(item.Name));
                }
            }
            else
            {
                var temp = await DeleteScriptAsync(item);
                if (temp.IsSuccess)
                {
                    //$"脚本:{item.Name}_文件丢失已删除"
                    toast.Show(ToastIcon.Warning, AppResources.Script_NoFile_.Format(item.Name));
                }
                else
                {
                    //toast.Show($"脚本:{item.Name}_文件丢失，删除失败去尝试手动删除");
                    toast.Show(ToastIcon.Error, AppResources.Script_NoFileDeleteError_.Format(item.Name));
                }
            }
        }
        return item;
    }

    public async Task SaveEnableScriptAsync(ScriptDTO item)
    {
        await scriptRepository.SaveScriptEnableAsync(item);
    }

    public async Task<IEnumerable<ScriptDTO>?> LoadingScriptContentAsync(IEnumerable<ScriptDTO>? all)
    {
        if (all.Any_Nullable())
        {
            try
            {
                foreach (var item in all)
                {
                    if (string.IsNullOrEmpty(item.Content))
                    {
                        await TryReadFileAsync(item);
                    }
                }
            }
            catch (Exception e)
            {
                var errorMsg = AppResources.Script_ReadFileError_.Format(e.GetAllMessage()); //$"文件读取出错:[{e}]";
                logger.LogError(e, errorMsg);
                toast.Show(ToastIcon.Error, errorMsg);
            }
            return all;
        }
        return null;
    }

    public async Task<IEnumerable<ScriptDTO>> GetAllScriptAsync() =>
        (await scriptRepository.GetAllAsync()).Select(ScriptMapper.ToDto).ToList();

    public async Task<IApiRsp<string?>> DownloadScriptAsync(string url)
    {
        string scriptStr;
        try
        {
            scriptStr = await ReadRemoteScriptAsync(url, CancellationToken.None);
        }
        catch (Exception ex)
        {
            return ApiRspHelper.Exception<string?>(ex);
        }
        if (string.IsNullOrWhiteSpace(scriptStr))
        {
            logger.LogError("DownloadScript IsNullOrWhiteSpace, url: {url}", url);
            return ApiRspHelper.Code(ApiRspCode.NoResponseContentValue, null, string.Empty);
        }
        else
        {
            string? cachePath = null;
            try
            {
                var md5 = Hashs.String.MD5(scriptStr);
                cachePath = Path.Combine(Plugin.Instance.CacheDirectory,
                    IScriptManager.DirName_Build, md5 + FileEx.DownloadCache);
                var fileInfo = new FileInfo(cachePath);
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                else if (File.Exists(fileInfo.FullName))
                {
                    if (!IOPath.FileTryDelete(fileInfo.FullName))
                    {
                        logger.LogError(
                            "DownloadScriptDeleteCatch Error, url: {url}, cachePath: {cachePath}",
                            url, cachePath);
                        return ApiRspHelper.Fail<string>(
                            AppResources.Script_CacheDeleteError_.Format(cachePath));
                    }
                }
                using (var stream = fileInfo.CreateText())
                {
                    stream.Write(scriptStr);
                    await stream.FlushAsync();
                }
                return ApiRspHelper.Ok(cachePath);
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "DownloadScript catch, url: {url}, cachePath: {cachePath}", url, cachePath);
                return ApiRspHelper.Exception<string>(e);
            }
        }
    }
}
