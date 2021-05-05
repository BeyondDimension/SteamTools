using System;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
	public interface IScriptClient
	{
        /// <summary>
        /// 获取基础框架最新版本
        /// </summary>
        /// <returns></returns>
        Task<IApiResponse<ScriptResponse>> Basics(string? errormsg = null);
        /// <summary>
        /// 脚本工坊接口
        /// </summary>
        /// <param name="name">脚本名称</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">数量</param>
        /// <returns></returns>
        Task<IApiResponse<PagedModel<ScriptDTO>>> ScriptTable(string? name = null, int pageIndex = 1, int pageSize = 15, string? errormsg = null);
        /// <summary>
        /// 获取全部脚本更新
        /// </summary>
        /// <param name="ids">脚本IDList</param>
        /// <returns></returns>
        Task<IApiResponse<IList<ScriptResponse>>> ScriptUpdateInfo(IEnumerable<Guid> ids, string? errormsg = null);
        

    }
}
