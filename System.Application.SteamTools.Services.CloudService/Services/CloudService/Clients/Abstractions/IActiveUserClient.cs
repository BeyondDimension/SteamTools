using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    /// <summary>
    /// 活跃用户
    /// </summary>
    public interface IActiveUserClient
    {
        Task<IApiResponse> Post(ActiveUserRecordDTO record);
    }
}