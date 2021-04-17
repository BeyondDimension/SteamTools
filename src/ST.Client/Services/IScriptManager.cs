using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
	public interface IScriptManager
	{
		/// <summary>
		/// 绑定JS
		/// </summary>
		/// <returns></returns>
		Task<bool> BuildScriptAsync(ScriptDTO model);
	}
}
