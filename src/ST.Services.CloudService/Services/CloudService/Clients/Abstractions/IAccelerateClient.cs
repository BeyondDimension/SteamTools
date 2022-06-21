using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IAccelerateClient
    {
        /// <summary>
        /// 获取所有脚本数据
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        Task<IApiResponse<List<ScriptDTO>>> Scripts();

        /// <summary>
        /// 获取所有加速项目组数据
        /// </summary>
        /// <returns></returns>
        [Obsolete("use All(EReverseProxyEngine)")]
        Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All();

        /// <summary>
        /// 获取所有加速项目组数据
        /// </summary>
        /// <param name="reverseProxyEngine">当前反向代理引擎</param>
        /// <returns></returns>
        Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All(EReverseProxyEngine reverseProxyEngine) => All();
    }
}