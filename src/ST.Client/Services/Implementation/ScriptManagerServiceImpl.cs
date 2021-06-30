using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using System.Application.Properties;

namespace System.Application.Services.Implementation
{
    public class ScriptManagerServiceImpl : IScriptManagerService
    {
        public const string TAG = "Scripts";

        protected readonly ILogger logger;
        protected readonly IToast toast;
        protected readonly IHttpService httpService;
        protected readonly IMapper mapper;

        protected readonly IScriptRepository scriptRepository;
        public ScriptManagerServiceImpl(
          IScriptRepository scriptRepository, IMapper mapper, ILoggerFactory loggerFactory, IToast toast, IHttpService httpService)
        {
            this.scriptRepository = scriptRepository;
            this.mapper = mapper;
            this.toast = toast;
            this.httpService = httpService;
            logger = loggerFactory.CreateLogger<ScriptManagerServiceImpl>();
        }
        /// <summary>
        /// 添加脚本
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<(bool state, ScriptDTO? model, string msg)> AddScriptAsync(string filePath, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                ScriptDTO.TryParse(filePath, out ScriptDTO? info);
                return await AddScriptAsync(fileInfo, info, oldInfo, build, order, deleteFile, pid, ignoreCache);
            }
            else
            {
                var msg = SR.Script_NoFile.Format(filePath);// $"文件不存在:{filePath}";
                logger.LogError(msg);
                return (false, null, msg);
            }
        }
        public async Task<(bool state, ScriptDTO? model, string msg)> AddScriptAsync(FileInfo fileInfo, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false)
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
                            if (await scriptRepository.ExistsScript(md5, sha512))
                            {
                                return (false, null, SR.Script_FileRepeat);
                            }
                        var url = Path.Combine(TAG, $"{md5}.js");
                        var savePath = Path.Combine(IOPath.AppDataDirectory, url);
                        var saveInfo = new FileInfo(savePath);
                        var isNoRepeat = saveInfo.FullName != fileInfo.FullName;
                        if (!saveInfo.Directory.Exists)
                        {
                            saveInfo.Directory.Create();
                        }
                        if (saveInfo.Exists)
                        {
                            if (isNoRepeat)
                                saveInfo.Delete();
                        }
                        else
                            fileInfo.CopyTo(savePath);
                        if (oldInfo != null && oldInfo.LocalId > 0)
                        {
                            info.LocalId = oldInfo.LocalId;
                            info.Id = oldInfo.Id;
                            info.Order = oldInfo.Order;
                            if (isNoRepeat)
                            {
                                var state = await DeleteScriptAsync(oldInfo, false);
                                if (!state.state)
                                    return (false, null, SR.Script_FileDeleteError.Format(oldInfo.FilePath));
                            }
                        }
                        if (pid.HasValue)
                            info.Id = pid.Value;
                        var cachePath = Path.Combine(IOPath.CacheDirectory, url);
                        info.FilePath = url;
                        info.IsBuild = build;
                        info.CachePath = url;
                        if (await BuildScriptAsync(info, build))
                        {
                            var db = mapper.Map<Script>(info);
                            db.MD5 = md5;
                            db.SHA512 = sha512;
                            if (db.Pid == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                                info.IsBasics = true;
                            if (order.HasValue)
                                db.Order = order.Value;
                            else if (db.Order == 0)
                                db.Order = 10;
                            try
                            {
                                if (deleteFile)
                                    fileInfo.Delete();
                            }
                            catch (Exception e) { logger.LogError(e.ToString()); }
                            var state = (await scriptRepository.InsertOrUpdateAsync(db)).rowCount > 0;
                            info.LocalId = db.Id;
                            return (state, info, state ? SR.Script_SaveDbSuccess : SR.Script_SaveDBError);
                        }
                        else
                        {
                            var msg = SR.Script_BuildError.Format(fileInfo.FullName);
                            logger.LogError(msg);
                            toast.Show(msg);
                            return (false, null, msg);
                        }
                    }
                    else
                    {
                        var msg = SR.Script_ReadFileError.Format(fileInfo.FullName);
                        logger.LogError(msg);
                        toast.Show(msg);
                        return (false, null, msg);
                    }
                }
                catch (Exception e)
                {
                    var msg = SR.Script_ReadFileError.Format(e.GetAllMessage());
                    logger.LogError(e, msg);
                    return (false, null, msg);
                }
            }
            else
            {
                var msg = string.Format(SR.Script_ReadFileError, fileInfo.FullName); //$"文件解析失败，请检查格式:{filePath}";
                logger.LogError(msg);
                return (false, null, msg);
            }
        }

        public async Task<bool> BuildScriptAsync(ScriptDTO model, bool build = true)
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
                                var errorMsg = SR.Script_BuildDownloadError.Format(model.Name, item);
                                logger.LogError(e, errorMsg);
                                toast.Show(errorMsg);
                            }
                        }
                        scriptContent.AppendLine("var jq = jQuery.noConflict();(($, jQuery) => {");
                        scriptContent.AppendLine(model.Content);
                        scriptContent.AppendLine("})(jq, jq)})()");
                    }
                    else
                    {
                        scriptContent.Append(model.Content);
                    }
                    var cachePath = Path.Combine(IOPath.CacheDirectory, model.CachePath);
                    var fileInfo = new FileInfo(cachePath);
                    if (!fileInfo.Directory.Exists)
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
                    //var scriptRequired = new string[model.RequiredJsArray.Length];
                    //Parallel.ForEach(model.RequiredJsArray, async (item,index)=>
                    //{
                    //	var scriptInfo = await httpService.GetAsync<string>(item);  
                    //});
                }

            }
            catch (Exception e)
            {
                var msg = SR.Script_BuildError.Format(e.GetAllMessage());
                logger.LogError(e, msg);
                toast.Show(msg);
            }
            return false;
        }
        public async Task<(bool state, string msg)> DeleteScriptAsync(ScriptDTO item, bool rmDb = true)
        {
            if (item.LocalId > 0)
            {
                var info = await scriptRepository.FirstOrDefaultAsync(x => x.Id == item.LocalId);
                if (info != null)
                {
                    var url = Path.Combine(TAG, $"{info.MD5}.js");
                    try
                    {
                        var cachePath = Path.Combine(IOPath.CacheDirectory, url);
                        var cacheInfo = new FileInfo(cachePath);
                        if (cacheInfo.Exists)
                            cacheInfo.Delete();
                    }
                    catch (Exception e)
                    {
                        var msg = SR.Script_CacheDeleteError.Format(e.GetAllMessage());
                        logger.LogError(e, msg);
                        return (false, msg);
                    }
                    try
                    {
                        var savePath = Path.Combine(IOPath.AppDataDirectory, url);
                        var fileInfo = new FileInfo(savePath);
                        if (fileInfo.Exists)
                            fileInfo.Delete();
                    }
                    catch (Exception e)
                    {
                        var msg = SR.Script_FileDeleteError.Format(e.GetAllMessage());
                        logger.LogError(e, msg);
                        return (false, msg);
                    }
                    if (rmDb)
                    {
                        var state = (await scriptRepository.DeleteAsync(item.LocalId)) > 0;
                        return (state, state ? SR.Script_DeleteSuccess : SR.Script_DeleteError);
                    }
                    return (true, SR.Script_DeleteSuccess);
                }
                else
                {
                    return (false, SR.Script_DeleteError);
                }
            }
            return (false, SR.Script_NoKey);
        }
        public async Task<ScriptDTO> TryReadFile(ScriptDTO item)
        {
            var cachePath = Path.Combine(IOPath.CacheDirectory, item.CachePath);
            var fileInfo = new FileInfo(cachePath);
            if (fileInfo.Exists)
                item.Content = File.ReadAllText(cachePath);
            else
            {
                var infoPath = Path.Combine(IOPath.AppDataDirectory, item.FilePath);
                var infoFile = new FileInfo(infoPath);
                if (infoFile.Exists)
                {
                    if (await BuildScriptAsync(item, item.IsBuild))
                    {
                        cachePath = Path.Combine(IOPath.CacheDirectory, item.CachePath);
                        fileInfo = new FileInfo(cachePath);
                        if (fileInfo.Exists)
                            item.Content = File.ReadAllText(cachePath);
                    }
                    else
                    {
                        toast.Show(SR.Script_ReadFileError.Format(item.Name));
                    }

                }
                else
                {
                    var temp = await DeleteScriptAsync(item);
                    if (temp.state)//$"脚本:{item.Name}_文件丢失已删除"
                        toast.Show(SR.Script_NoFile.Format(item.Name));
                    else
                        toast.Show(SR.Script_NoFileDeleteError.Format(item.Name));
                    //toast.Show($"脚本:{item.Name}_文件丢失，删除失败去尝试手动删除");
                }
            }
            return item;
        }
        public async Task<IEnumerable<ScriptDTO>> GetAllScript()
        {
            var scriptList = mapper.Map<List<ScriptDTO>>(await scriptRepository.GetAllAsync());

            var basicsId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            if (scriptList.Count(x => x.Id == basicsId) > 1)
            {
                var allpath = scriptList.Where(x => x.Id == basicsId);
                string? savePath = null;
                foreach (var item in allpath)
                {
                    var path = new FileInfo(Path.Combine(IOPath.AppDataDirectory, item.FilePath));
                    if (path.Exists)
                    {
                        if (savePath == null)
                        {
                            savePath = item.FilePath;
                        }
                        else
                        {
                            var state = (await scriptRepository.DeleteAsync(item.LocalId)) > 0;
                        }
                    }
                    else
                    {
                        await DeleteScriptAsync(item);
                    }
                }
                scriptList = mapper.Map<List<ScriptDTO>>(await scriptRepository.GetAllAsync());
            }
            try
            {
                foreach (var item in scriptList)
                {
                    await TryReadFile(item);
                    if (item.Id == basicsId)
                    {
                        item.IsBasics = true;
                        item.Order = 1;
                        if (item.IsBuild)
                        {
                            item.IsBuild = false;
                            var fileInfo = new FileInfo(item.FilePath);
                            if (fileInfo.Exists)
                            {
                                var state = await AddScriptAsync(fileInfo, item, item, false, 1, ignoreCache: true);
                                if (state.state && state.model?.Content != null)
                                    item.Content = state.model!.Content;
                            }
                            else
                            {
                                var basicsInfo = await ICloudServiceClient.Instance.Script.Basics(SR.Script_NoFile.Format(item.FilePath));
                                if (basicsInfo.Code == ApiResponseCode.OK && basicsInfo.Content != null)
                                {
                                    var jspath = await DI.Get<IScriptManagerService>().DownloadScript(basicsInfo.Content.UpdateLink);
                                    if (jspath.state)
                                    {
                                        var build = await DI.Get<IScriptManagerService>().AddScriptAsync(jspath.path,item, build: false, order: 1, deleteFile: true, pid: basicsInfo.Content.Id, ignoreCache: true);
                                        if (build.state && build.model?.Content != null)
                                            item.Content = build.model!.Content;
                                    }
                                }
                            }
                        }
                    }
                    //if (item.Content!= null) { 

                    //}
                }
            }
            catch (Exception e)
            {
                var errorMsg = SR.Script_ReadFileError.Format(e.GetAllMessage());//$"文件读取出错:[{e}]";
                logger.LogError(e, errorMsg);
                toast.Show(errorMsg);
            }
            return scriptList.Where(x => !string.IsNullOrWhiteSpace(x.Content));
        }
        //public async Task<ScriptDTO> BasicsTry(ScriptDTO script) { 

        //}
        public async Task<(bool state, string path)> DownloadScript(string url)
        {
            var scriptInfo = await httpService.GetAsync<string>(url);
            if (scriptInfo != null && scriptInfo.Length > 0)
            {
                try
                {
                    var md5 = Hashs.String.MD5(scriptInfo);
                    var cachePath = Path.Combine(IOPath.CacheDirectory, TAG, $"{md5}{AppUpdateServiceImpl.FileExDownloadCache}");
                    var fileInfo = new FileInfo(cachePath);
                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    if (fileInfo.Exists)
                        fileInfo.Delete();
                    using (var stream = fileInfo.CreateText())
                    {
                        stream.Write(scriptInfo);
                        await stream.FlushAsync();
                        await stream.DisposeAsync();
                    }
                    return (true, cachePath);
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
            return (false, string.Empty);
        }
    }
}
