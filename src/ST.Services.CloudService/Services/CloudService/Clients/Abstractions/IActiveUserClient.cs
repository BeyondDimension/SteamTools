using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    /// <summary>
    /// 活跃用户
    /// </summary>
    public interface IActiveUserClient
    {
        /// <summary>
        /// 纪录活跃用户并返回通知(公告，如果有新公告时)
        /// </summary>
        /// <param name="request">匿名收集数据</param>
        /// <param name="lastNotificationRecordId">最后一次收到的通知ID</param>
        /// <returns></returns>
        Task<IApiResponse> Post(ActiveUserRecordDTO request);
    }
}