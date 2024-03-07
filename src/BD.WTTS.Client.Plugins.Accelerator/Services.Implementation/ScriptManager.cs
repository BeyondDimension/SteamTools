using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IScriptManager"/>
public sealed class ScriptManager : GeneralHttpClientFactory, IScriptManager
{
    const string TAG = nameof(ScriptManager);
    const string HomepageURL = "HomepageURL";
    const string DownloadURL = "DownloadURL";
    const string UpdateURL = "UpdateURL";
    const string Exclude = "Exclude";
    const string Grant = "Grant";
    const string Require = "Require";
    const string Include = "Include";
    const string DescRegex = @"(?<={0})[\s\S]*?(?=\n)";

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
        var client = CreateClient(null, HttpHandlerCategory.Default);
        return client.GetAsync<T>(logger, requestUri, accept,
            cancellationToken: cancellationToken, userAgent: http_helper.UserAgent);
    }

    public async Task<ScriptDTO?> ReadScriptAsync(string path)
    {

        var content = await File.ReadAllTextAsync(path);
        if (!string.IsNullOrEmpty(content))
        {
            var userScript = content.Substring("==UserScript==", "==/UserScript==");
            if (!string.IsNullOrEmpty(userScript))
            {
                var script = new ScriptDTO
                {
                    FilePath = path,
                    Content = content.Replace("</script>", "<\\/script>"),//不读取至 Content
                    //Content = content.Replace("</script>", "<\\/script>").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", ""),
                    Name = Regex.Match(userScript, string.Format(DescRegex, "@Name"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    //NameSpace = Regex.Matches(userScript, string.Format(DescRegex, $"@NameSpace"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).ToArray(),
                    Version = Regex.Match(userScript, string.Format(DescRegex, "@Version"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    Describe = Regex.Match(userScript, string.Format(DescRegex, "@Description"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    AuthorName = Regex.Match(userScript, string.Format(DescRegex, "@Author"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    SourceLink = Regex.Match(userScript, string.Format(DescRegex, "@HomepageURL"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    //SupportURL = Regex.Match(userScript, string.Format(DescRegex, $"@SupportURL"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    DownloadLink = Regex.Match(userScript, string.Format(DescRegex, $"@{DownloadURL}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    UpdateLink = Regex.Match(userScript, string.Format(DescRegex, $"@{UpdateURL}"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true),
                    ExcludeDomainNames = string.Join(ApiConstants.GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Exclude}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true)),
                    DependentGreasyForkFunction = Regex.Matches(userScript, string.Format(DescRegex, $"@{Grant}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true).Any_Nullable(),
                    RequiredJs = string.Join(ApiConstants.GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Require}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true)),
                };
                var matchs = string.Join(ApiConstants.GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@Match"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true));
                var includes = string.Join(ApiConstants.GeneralSeparator, Regex.Matches(userScript, string.Format(DescRegex, $"@{Include}"), RegexOptions.IgnoreCase).GetValues(s => s.Success == true));
                script.MatchDomainNames = string.IsNullOrEmpty(matchs) ? includes : matchs;
                // 忽略脚本 Enable 启动标签默认启动
                //var enable = Regex.Match(userScript, string.Format(DescRegex, "@Enable"), RegexOptions.IgnoreCase).GetValue(s => s.Success == true);
                script.Disable = false;
                //script.Enable = bool.TryParse(enable, out var e) && e;
                return script;
            }
        }
        return null;
    }

    public async Task<IApiRsp<ScriptDTO?>> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool isCompile = true, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists)
        {
            var info = await ReadScriptAsync(filePath);
            return await SaveScriptAsync(fileInfo, info, oldInfo, isCompile, order, deleteFile, pid, ignoreCache);
        }
        else
        {
            var msg = AppResources.Script_NoFile_.Format(filePath); // $"文件不存在:{filePath}";
            logger.LogError(msg);
            return ApiRspHelper.Fail<ScriptDTO?>(msg);
        }
    }

    public async Task<IApiRsp<ScriptDTO?>> SaveScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null, bool isCompile = true, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
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
                    var jsFileName = md5 + FileEx.JS;
                    var jsRelativePath = Path.Combine(IScriptManager.DirName, jsFileName);
                    var jsBuildRelativePath = Path.Combine(IScriptManager.DirName_Build, jsFileName);
                    var jsSavePath = Path.Combine(Plugin.Instance.AppDataDirectory, IScriptManager.DirName, jsFileName);
                    var jsSaveInfo = new FileInfo(jsSavePath);
                    var isNoRepeat = jsSaveInfo.FullName != fileInfo.FullName;
                    if (jsSaveInfo.Directory != null && !jsSaveInfo.Directory.Exists)
                    {
                        jsSaveInfo.Directory.Create();
                    }
                    if (jsSaveInfo.Exists)
                    {
                        if (isNoRepeat)
                        {
                            jsSaveInfo.Delete();
                            fileInfo.CopyTo(jsSavePath);
                        }
                    }
                    else
                    {
                        fileInfo.CopyTo(jsSavePath);
                    }
                    if (oldInfo != null)
                    {
                        //本地DTO
                        if (oldInfo.LocalId > 0)
                        {
                            info.LocalId = oldInfo.LocalId;
                            info.Id = oldInfo.Id;
                            info.Order = oldInfo.Order;
                            info.IconUrl = oldInfo.IconUrl;
                            if (isNoRepeat)
                            {
                                var state = await DeleteScriptAsync(oldInfo, false);
                                if (!state.IsSuccess)
                                {
                                    return ApiRspHelper.Fail<ScriptDTO?>(AppResources.Script_FileDeleteError_.Format(oldInfo.FilePath));
                                }
                            }
                        }
                        else
                        {
                            //在线 DTO 返回值
                            info.SourceLink = oldInfo.SourceLink;
                            info.UpdateLink = oldInfo.UpdateLink;
                            info.DownloadLink = oldInfo.DownloadLink;
                            info.IconUrl = oldInfo.IconUrl;
                            info.Describe = oldInfo.Describe;
                            info.UpdateTime = oldInfo.UpdateTime;
                            info.AccelerateProjects = oldInfo.AccelerateProjects;
                            info.Version = oldInfo.Version;
                            info.Name = oldInfo.Name;
                        }
                    }
                    if (pid.HasValue)
                        info.Id = pid.Value;
                    info.FilePath = jsRelativePath;
                    info.IsCompile = isCompile;
                    info.CachePath = jsBuildRelativePath;
                    var jsBuildFullPath = Path.Combine(Plugin.Instance.CacheDirectory, IScriptManager.DirName_Build, jsFileName);
                    jsSaveInfo = new FileInfo(jsBuildFullPath);
                    if (await BuildScriptAsync(info, jsSaveInfo, isCompile))
                    {
                        var entity = mapper.Map<Script>(info);
                        entity.MD5 = md5;
                        entity.SHA512 = sha512;
                        if (entity.Pid != null && entity.Pid == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                        {
                            info.IsBasics = true;
                            order = 1;
                        }
                        if (order.HasValue)
                            entity.Order = order.Value > int.MaxValue ? int.MaxValue : (order.Value < int.MinValue ? int.MinValue : ((int)order.Value));
                        else if (entity.Order == 0)
                            entity.Order = 10;
                        info.Order = entity.Order;
                        try
                        {
                            if (deleteFile)
                                fileInfo.Delete();
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e.ToString());
                        }
                        var isSuccess = (await scriptRepository.InsertOrUpdateAsync(entity, CancellationToken.None)).rowCount > 0;
                        info.LocalId = entity.Id;
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
                        var msg = AppResources.Script_BuildError_.Format(fileInfo.FullName);
                        logger.LogError(msg);
                        toast.Show(ToastIcon.Error, msg);
                        return ApiRspHelper.Fail<ScriptDTO?>(msg);
                    }
                }
                else
                {
                    var msg = AppResources.Script_ReadFileError_.Format(fileInfo.FullName);
                    logger.LogError(msg);
                    toast.Show(ToastIcon.Error, msg);
                    return ApiRspHelper.Fail<ScriptDTO?>(msg);
                }
            }
            catch (Exception e)
            {
                var msg = AppResources.Script_ReadFileError_.Format(e.GetAllMessage());
                logger.LogError(e, msg);
                return ApiRspHelper.Code<ScriptDTO?>(ApiRspCode.Fail, msg, default, e);
            }
        }
        else
        {
            var msg = string.Format(AppResources.Script_ReadFileError_, fileInfo.FullName); //$"文件解析失败，请检查格式:{filePath}";
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
                            var errorMsg = AppResources.Script_BuildDownloadError__.Format(model.Name, item);
                            logger.LogError(e, errorMsg);
                            toast.Show(ToastIcon.Error, errorMsg);
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
            var msg = AppResources.Script_BuildError_.Format(e.GetAllMessage());
            logger.LogError(e, msg);
            toast.Show(ToastIcon.Error, msg);
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
                var cachePath = Path.Combine(Plugin.Instance.CacheDirectory, IScriptManager.DirName_Build, fileName);
                try
                {
                    var cacheInfo = new FileInfo(cachePath);
                    if (cacheInfo.Exists)
                        cacheInfo.Delete();
                }
                catch (Exception e)
                {
                    var msg = AppResources.Script_CacheDeleteError_.Format(e.GetAllMessage());
                    logger.LogError(e, "cachePath: {cachePath}, msg: {msg}", cachePath, msg);
                    return ApiRspHelper.Fail(msg);
                }

                var savePath = Path.Combine(Plugin.Instance.AppDataDirectory, IScriptManager.DirName, fileName);
                try
                {
                    var fileInfo = new FileInfo(savePath);
                    if (fileInfo.Exists)
                        fileInfo.Delete();
                }
                catch (Exception e)
                {
                    var msg = AppResources.Script_FileDeleteError_.Format(e.GetAllMessage());
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
                item.Content = string.Empty;
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
                        item.Content = string.Empty;
                    }
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
            //item.Content = File.ReadAllText(cachePath);
        }
        else
        {
            var fileInfo = new FileInfo(cachePath);
            var infoPath = Path.Combine(Plugin.Instance.AppDataDirectory, item.FilePath);
            if (File.Exists(infoPath))
            {
                if (isReadContent)
                {
                    item.Content = File.ReadAllText(infoPath);
                }
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