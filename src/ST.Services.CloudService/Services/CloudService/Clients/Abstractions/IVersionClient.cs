using System.Application.Models;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IVersionClient
    {
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="id">当前应用版本Id</param>
        /// <param name="platform">(单选)当前设备所属平台</param>
        /// <param name="deviceIdiom">(单选)当前设备所属类型</param>
        /// <param name="version">当前设备运行的操作系统版本号</param>
        /// <param name="architecture">当前OS支持的CPU架构</param>
        /// <param name="deploymentMode">当前应用部署模型</param>
        /// <returns></returns>
        Task<IApiResponse<AppVersionDTO?>> CheckUpdate2(Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            Version version,
            Architecture architecture,
            DeploymentMode deploymentMode);
    }
}