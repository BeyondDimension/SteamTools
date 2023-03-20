using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IScriptManager"/>
public sealed class ScriptManager : GeneralHttpClientFactory, IScriptManager
{
    const string TAG = nameof(ScriptManager);

    protected override string? DefaultClientName => TAG;

    readonly IToast toast;
    readonly IMapper mapper;
    readonly IScriptRepository scriptRepository;
    readonly IMicroServiceClient csc;

    public ScriptManager(
        IHttpClientFactory clientFactory,
        IScriptRepository scriptRepository,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IToast toast,
        IHttpPlatformHelperService http_helper,
        IMicroServiceClient csc) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper, clientFactory)
    {
        this.scriptRepository = scriptRepository;
        this.mapper = mapper;
        this.toast = toast;
        this.csc = csc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        var client = CreateClient();
        return client.GetAsync<T>(logger, requestUri, accept,
            cancellationToken: cancellationToken, userAgent: http_helper.UserAgent);
    }

    public async Task<IApiRsp<ScriptDTO?>> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists)
        {
            _ = ScriptDTO.TryParse(filePath, out ScriptDTO? info);
            return await AddScriptAsync(fileInfo, info, oldInfo, build, order, deleteFile, pid, ignoreCache);
        }
        else
        {
            var msg = AppResources.Script_NoFile.Format(filePath); // $"文件不存在:{filePath}";
            logger.LogError(msg);
            return ApiRspHelper.Fail<ScriptDTO?>(msg);
        }
    }

    public async Task<IApiRsp<ScriptDTO?>> AddScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
    {
        if (info != null)
        {
            try
            {
                if (info.Content != null)
                {
                    var md5 = Hashs.String.MD5(info.Content);
                    var sha512 = Hashs.String.SHA512(info.Content);
                    if (!ignoreCache)
                    {
                        if (await scriptRepository.ExistsScriptAsync(md5, sha512))
                        {
                            return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_FileRepeat);
                        }
                    }
                    var fileName = md5 + FileEx.JS;
                    var path = Path.Combine(IScriptManager.DirName, fileName);
                    var savePath = Path.Combine(IOPath.AppDataDirectory, IScriptManager.DirName, fileName);
                    var saveInfo = new FileInfo(savePath);
                    var isNoRepeat = saveInfo.FullName != fileInfo.FullName;
                    if (saveInfo.Directory != null && !saveInfo.Directory.Exists)
                    {
                        saveInfo.Directory.Create();
                    }
                    if (saveInfo.Exists)
                    {
                        if (isNoRepeat)
                            saveInfo.Delete();
                    }
                    else
                    {
                        fileInfo.CopyTo(savePath);
                    }
                    if (oldInfo != null && oldInfo.LocalId > 0)
                    {
                        info.LocalId = oldInfo.LocalId;
                        info.Id = oldInfo.Id;
                        info.Order = oldInfo.Order;
                        if (isNoRepeat)
                        {
                            var state = await DeleteScriptAsync(oldInfo, false);
                            if (!state.IsSuccess)
                            {
                                return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_FileDeleteError.Format(oldInfo.FilePath));
                            }
                        }
                    }
                    if (pid.HasValue)
                        info.Id = pid.Value;
                    var cachePath = Path.Combine(IOPath.CacheDirectory, IScriptManager.DirName, fileName);
                    info.FilePath = path;
                    info.IsBuild = build;
                    info.CachePath = path;
                    saveInfo = new FileInfo(cachePath);
                    if (await BuildScriptAsync(info, saveInfo, build))
                    {
                        var db = mapper.Map<Script>(info);
                        db.MD5 = md5;
                        db.SHA512 = sha512;
                        if (db.Pid != null && db.Pid == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                        {
                            info.IsBasics = true;
                            order = 1;
                        }
                        if (order.HasValue)
                            db.Order = order.Value;
                        else if (db.Order == 0)
                            db.Order = 10;
                        info.Order = db.Order;
                        try
                        {
                            if (deleteFile)
                                fileInfo.Delete();
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e.ToString());
                        }
                        var isSuccess = (await scriptRepository.InsertOrUpdateAsync(db)).rowCount > 0;
                        info.LocalId = db.Id;
                        if (isSuccess)
                        {
                            return ApiRspHelper.Code<ScriptDTO?>(ApiRspCode.OK, AppResources.Script_SaveDbSuccess, info);
                        }
                        else
                        {
                            return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_SaveDBError);
                        }
                    }
                    else
                    {
                        var msg = AppResources.Script_BuildError.Format(fileInfo.FullName);
                        logger.LogError(msg);
                        toast.Show(msg);
                        return ApiRspHelper.Fail<ScriptDTO?>(msg);
                    }
                }
                else
                {
                    var msg = AppResources.Script_ReadFileError.Format(fileInfo.FullName);
                    logger.LogError(msg);
                    toast.Show(msg);
                    return ApiRspHelper.Fail<ScriptDTO?>(msg);
                }
            }
            catch (Exception e)
            {
                var msg = AppResources.Script_ReadFileError.Format(e.GetAllMessage());
                logger.LogError(e, msg);
                return ApiRspHelper.Code<ScriptDTO?>(ApiRspCode.Fail, msg, default, e);
            }
        }
        else
        {
            var msg = string.Format(AppResources.Script_ReadFileError, fileInfo.FullName); //$"文件解析失败，请检查格式:{filePath}";
            logger.LogError(msg);
            return ApiRspHelper.Fail<ScriptDTO?>(msg);
        }
    }

    public async Task<bool> BuildScriptAsync(ScriptDTO model, FileInfo fileInfo, bool build = true)
    {
        try
        {
            if (model.RequiredJsArray != null)
            {
                var scriptContent = new StringBuilder();
                if (build)
                {
                    scriptContent.AppendLine("(function () {");
                    foreach (var item in model.RequiredJsArray)
                    {
                        try
                        {
                            var scriptInfo = await GetAsync<string>(item);
                            scriptContent.AppendLine(scriptInfo);
                        }
                        catch (Exception e)
                        {
                            var errorMsg = AppResources.Script_BuildDownloadError.Format(model.Name, item);
                            logger.LogError(e, errorMsg);
                            toast.Show(errorMsg);
                        }
                    }
                    scriptContent.AppendLine("try{var jq2 = $.noConflict(true);}catch{};(($, jQuery) => {");
                    scriptContent.AppendLine(model.Content);
                    scriptContent.AppendLine("})(jq2, jq2)})()");
                }
                else
                {
                    scriptContent.Append(model.Content);
                }
                fileInfo.Refresh();
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                if (fileInfo.Exists)
                    fileInfo.Delete();
                model.Content = scriptContent.ToString();
                using (var stream = fileInfo.CreateText())
                {
                    stream.Write(scriptContent);
                    await stream.FlushAsync();
                    await stream.DisposeAsync();
                    //stream
                }
                return true;
            }

        }
        catch (Exception e)
        {
            var msg = AppResources.Script_BuildError.Format(e.GetAllMessage());
            logger.LogError(e, msg);
            toast.Show(msg);
        }
        return false;
    }

    public async Task<IApiRsp> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true)
    {
        // 对于删除操作，应当为幂等，当不存在时候应当返回成功，除非删除失败否则不应该有错误
        if (item.LocalId > 0)
        {
            var info = await scriptRepository.FirstOrDefaultAsync(x => x.Id == item.LocalId);
            if (info != null)
            {
                var fileName = info.MD5 + FileEx.JS;
                var cachePath = Path.Combine(IOPath.CacheDirectory, IScriptManager.DirName, fileName);
                try
                {
                    var cacheInfo = new FileInfo(cachePath);
                    if (cacheInfo.Exists)
                        cacheInfo.Delete();
                }
                catch (Exception e)
                {
                    var msg = AppResources.Script_CacheDeleteError.Format(e.GetAllMessage());
                    logger.LogError(e, "cachePath: {cachePath}, msg: {msg}", cachePath, msg);
                    return ApiRspHelper.Fail(msg);
                }

                var savePath = Path.Combine(IOPath.AppDataDirectory, IScriptManager.DirName, fileName);
                try
                {
                    var fileInfo = new FileInfo(savePath);
                    if (fileInfo.Exists)
                        fileInfo.Delete();
                }
                catch (Exception e)
                {
                    var msg = AppResources.Script_FileDeleteError.Format(e.GetAllMessage());
                    logger.LogError(e, "savePath: {savePath}, msg: {msg}", savePath, msg);
                    return ApiRspHelper.Fail(msg);
                }

                if (removeByDataBase)
                {
                    await scriptRepository.DeleteAsync(item.LocalId);
                }

                return OK_Script_DeleteSuccess();
            }
            else
            {
                return OK_Script_DeleteSuccess();
                //logger.LogError("DeleteScriptAsync not found, localId:{0}", item.LocalId);
                //return ApiResponse.Code(ApiResponseCode.NotFound, AppResources.Script_DeleteError);
            }
        }
        else
        {
            // 此类情况可忽略
            return OK_Script_DeleteSuccess();
            //logger.LogError("DeleteScriptAsync not key, localId:{0}", item.LocalId);
            //return ApiResponse.Fail(AppResources.Script_NoKey);
        }
        static IApiRsp OK_Script_DeleteSuccess() => ApiRspHelper.Ok(AppResources.Script_DeleteSuccess);
    }

    public async Task<ScriptDTO> TryReadFileAsync(ScriptDTO item)
    {
        var cachePath = Path.Combine(IOPath.CacheDirectory, item.CachePath);
        if (File.Exists(cachePath))
        {
            item.Content = File.ReadAllText(cachePath);
        }
        else
        {
            var fileInfo = new FileInfo(cachePath);
            var infoPath = Path.Combine(IOPath.AppDataDirectory, item.FilePath);
            if (File.Exists(infoPath))
            {
                item.Content = File.ReadAllText(infoPath);
                if (!await BuildScriptAsync(item, fileInfo, item.IsBuild))
                {
                    toast.Show(AppResources.Script_ReadFileError.Format(item.Name));
                }

            }
            else
            {
                var temp = await DeleteScriptAsync(item);
                if (temp.IsSuccess)
                {
                    //$"脚本:{item.Name}_文件丢失已删除"
                    toast.Show(AppResources.Script_NoFile.Format(item.Name));
                }
                else
                {
                    //toast.Show($"脚本:{item.Name}_文件丢失，删除失败去尝试手动删除");
                    toast.Show(AppResources.Script_NoFileDeleteError.Format(item.Name));
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
                var errorMsg = AppResources.Script_ReadFileError.Format(e.GetAllMessage()); //$"文件读取出错:[{e}]";
                logger.LogError(e, errorMsg);
                toast.Show(errorMsg);
            }
            return all;
        }
        return null;
    }

    public async Task<IEnumerable<ScriptDTO>> GetAllScriptAsync() =>
        mapper.Map<List<ScriptDTO>>(await scriptRepository.GetAllAsync());

    public async Task<IApiRsp<string?>> DownloadScriptAsync(string url)
    {
        var scriptStr = await GetAsync<string>(url, MediaTypeNames.JS);
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
                cachePath = Path.Combine(IOPath.CacheDirectory,
                    IScriptManager.DirName, md5 + FileEx.DownloadCache);
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
                            AppResources.Script_CacheDeleteError.Format(cachePath));
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