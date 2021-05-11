using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IManageClient
    {
        /// <summary>
        /// 换绑手机（安全验证）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IApiResponse<string>> ChangeBindPhoneNumber(ChangePhoneNumberRequest.Validation request);

        /// <summary>
        /// 绑定新手机号
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IApiResponse> ChangeBindPhoneNumber(ChangePhoneNumberRequest.New request);

        /// <summary>
        /// 删除账号
        /// </summary>
        /// <returns></returns>
        Task<IApiResponse> DeleteAccount();

        /// <summary>
        /// 每日签到
        /// </summary>
        /// <returns></returns>
        Task<IApiResponse<ClockInResponse>> ClockIn();

        /// <summary>
        /// 编辑用户资料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IApiResponse> EditUserProfile(EditUserProfileRequest request);

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        Task<IApiResponse> SignOut();
    }
}