using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Repositories;
using System.Application.UI.Resx;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IScriptManager"/>
    public sealed class ScriptManager : IScriptManager
    {
        readonly ILogger logger;
        readonly IToast toast;
        readonly IHttpService httpService;
        readonly IMapper mapper;
        readonly IScriptRepository scriptRepository;
        readonly ICloudServiceClient csc;

        public ScriptManager(
            IScriptRepository scriptRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IToast toast,
            IHttpService httpService,
            ICloudServiceClient csc)
        {
            this.scriptRepository = scriptRepository;
            this.mapper = mapper;
            this.toast = toast;
            this.httpService = httpService;
            logger = loggerFactory.CreateLogger<ScriptManager>();
            this.csc = csc;
        }

        /// <summary>
        /// 添加脚本
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<IApiResponse<ScriptDTO?>> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
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
                return ApiResponse.Fail<ScriptDTO?>(msg);
            }
        }

        public async Task<IApiResponse<ScriptDTO?>> AddScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
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
                            if (await scriptRepository.ExistsScript(md5, sha512))
                            {
                                return ApiResponse.Fail<ScriptDTO?>(AppResources.Script_FileRepeat);
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
                                    return ApiResponse.Fail<ScriptDTO?>(AppResources.Script_FileDeleteError.Format(oldInfo.FilePath));
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
                                return ApiResponse.Code<ScriptDTO?>(ApiResponseCode.OK, AppResources.Script_SaveDbSuccess, info);
                            }
                            else
                            {
                                return ApiResponse.Fail<ScriptDTO?>(AppResources.Script_SaveDBError);
                            }
                        }
                        else
                        {
                            var msg = AppResources.Script_BuildError.Format(fileInfo.FullName);
                            logger.LogError(msg);
                            toast.Show(msg);
                            return ApiResponse.Fail<ScriptDTO?>(msg);
                        }
                    }
                    else
                    {
                        var msg = AppResources.Script_ReadFileError.Format(fileInfo.FullName);
                        logger.LogError(msg);
                        toast.Show(msg);
                        return ApiResponse.Fail<ScriptDTO?>(msg);
                    }
                }
                catch (Exception e)
                {
                    var msg = AppResources.Script_ReadFileError.Format(e.GetAllMessage());
                    logger.LogError(e, msg);
                    return ApiResponse.Code<ScriptDTO?>(ApiResponseCode.Fail, msg, default, e);
                }
            }
            else
            {
                var msg = string.Format(AppResources.Script_ReadFileError, fileInfo.FullName); //$"文件解析失败，请检查格式:{filePath}";
                logger.LogError(msg);
                return ApiResponse.Fail<ScriptDTO?>(msg);
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
                                var scriptInfo = await httpService.GetAsync<string>(item);
                                scriptContent.AppendLine(scriptInfo);
                            }
                            catch (Exception e)
                            {
                                var errorMsg = AppResources.Script_BuildDownloadError.Format(model.Name, item);
                                logger.LogError(e, errorMsg);
                                toast.Show(errorMsg);
                            }
                        }
                        scriptContent.AppendLine("var jq = jQuery.noConflict(true);(($, jQuery) => {");
                        scriptContent.AppendLine(model.Content);
                        scriptContent.AppendLine("})(jq, jq)})()");
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

        public async Task<IApiResponse> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true)
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
                        logger.LogError(e, "path:{0}, msg: {1}", cachePath, msg);
                        return ApiResponse.Fail(msg);
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
                        logger.LogError(e, "path:{0}, msg: {1}", savePath, msg);
                        return ApiResponse.Fail(msg);
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
            static IApiResponse OK_Script_DeleteSuccess() => ApiResponse.Ok(AppResources.Script_DeleteSuccess);
        }

        public async Task<ScriptDTO> TryReadFile(ScriptDTO item)
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

        public async Task SaveEnableScript(ScriptDTO item)
        {
            await scriptRepository.SaveScriptEnable(item);
        }

        public async Task<IEnumerable<ScriptDTO>?> LoadingScriptContent(IEnumerable<ScriptDTO>? all)
        {
            if (all.Any_Nullable())
            {
                try
                {
                    foreach (var item in all)
                    {
                        if (string.IsNullOrEmpty(item.Content))
                        {
                            await TryReadFile(item);
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

        public async Task<IApiResponse<string>> DownloadScriptAsync(string url)
        {
            var scriptStr = await httpService.GetAsync<string>(url, MediaTypeNames.JS);
            if (string.IsNullOrWhiteSpace(scriptStr))
            {
                logger.LogError("DownloadScript IsNullOrWhiteSpace, url:{0}", url);
                return ApiResponse.Code(ApiResponseCode.NoResponseContentValue, null, string.Empty);
            }
            else
            {
                string? cachePath = null;
                try
                {
                    var md5 = Hashs.String.MD5(scriptStr);
                    cachePath = Path.Combine(IOPath.CacheDirectory, IScriptManager.DirName, md5 + FileEx.DownloadCache);
                    var fileInfo = new FileInfo(cachePath);
                    if (fileInfo.Directory != null && !fileInfo.Directory.Exists) fileInfo.Directory.Create();
                    else if (File.Exists(fileInfo.FullName))
                    {
                        if (!IOPath.FileTryDelete(fileInfo.FullName))
                        {
                            logger.LogError("DownloadScriptDeleteCatch Error, url:{0}, cachePath:{1}", url, cachePath);
                            return ApiResponse.Fail<string>(AppResources.Script_CacheDeleteError.Format(cachePath));
                        }
                    }
                    using (var stream = fileInfo.CreateText())
                    {
                        stream.Write(scriptStr);
                        await stream.FlushAsync();
                    }
                    return ApiResponse.Ok(cachePath);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "DownloadScript catch, url:{0}, cachePath:{1}", url, cachePath);
                    return ApiResponse.Exception<string>(e);
                }
            }
        }
    }
}
