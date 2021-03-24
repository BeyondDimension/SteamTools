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
        Task<IApiResponse<List<ScriptDTO>>> Scripts();

        /// <summary>
        /// 获取所有加速项目组数据
        /// </summary>
        /// <returns></returns>
        Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All();
    }
}