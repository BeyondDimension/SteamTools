using System;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 脚本管理
    /// </summary>
    public interface IScriptManager
    {
        public const string DirName = "Scripts"; 
        static IScriptManager Instance => DI.Get<IScriptManager>();

        /// <summary>
        /// 绑定JS
        /// </summary>
        /// <returns></returns>
        Task<bool> BuildScriptAsync(ScriptDTO model, FileInfo fileInfo, bool build = true);

        /// <summary>
        /// 添加Js
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IApiResponse<ScriptDTO?>> AddScriptAsync(string path, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

        /// <summary>
        /// 添加Js
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IApiResponse<ScriptDTO?>> AddScriptAsync(FileInfo path, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

        /// <summary>
        /// 获取Sqlite全部脚本
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScriptDTO>> GetAllScriptAsync();

        /// <summary>
        /// 删除脚本
        /// </summary>
        /// <param name="item"></param>
        /// <param name="removeByDataBase"></param>
        /// <returns></returns>
        Task<IApiResponse> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true);

        /// <summary>
        /// 通过Url下载js
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<IApiResponse<string>> DownloadScriptAsync(string url);
        /// <summary>
        /// 保存脚本启用状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SaveEnableScript(ScriptDTO item);
    }
}
