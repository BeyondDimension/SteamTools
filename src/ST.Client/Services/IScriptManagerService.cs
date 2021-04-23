using System;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
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
		Task<bool> BuildScriptAsync(ScriptDTO model);
		/// <summary>
		/// 添加Js
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<(bool state,ScriptDTO model, string msg)> AddScriptAsync(string path);
		/// <summary>
		/// 获取Sqlite全部脚本
		/// </summary>
		/// <returns></returns>
		Task<IList<ScriptDTO>> GetAllScript();
	}
}
