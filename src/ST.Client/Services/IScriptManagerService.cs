using System;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
	public interface IScriptManagerService
	{
		/// <summary>
		/// 绑定JS
		/// </summary>
		/// <returns></returns>
		Task<bool> BuildScriptAsync(ScriptDTO model, bool build = true);
		/// <summary>
		/// 添加Js
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<(bool state, ScriptDTO? model, string msg)> AddScriptAsync(string path, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);
        /// <summary>
        /// 添加Js
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(bool state, ScriptDTO? model, string msg)> AddScriptAsync(FileInfo path, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);
        /// <summary>
        /// 获取Sqlite全部脚本
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScriptDTO>> GetAllScript();
		/// <summary>
		/// 删除指定ID的脚本
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<(bool state, string msg)> DeleteScriptAsync(ScriptDTO item, bool rmDb = true);
		/// <summary>
		/// 下载指定Urljs
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		Task<(bool state, string path)> DownloadScript(string url);
	}
}
