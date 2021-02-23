using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IVersionClient
    {
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="platform"></param>
        /// <param name="deviceIdiom"></param>
        /// <param name="supportedAbis"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <returns></returns>
        Task<IApiResponse<AppVersionInfoDTO?>> CheckUpdate(Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            CPUABI.Value supportedAbis,
            int major,
            int minor);
    }
}