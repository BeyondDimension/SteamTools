using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IAccountClient
    {
        Task<IApiResponse<LoginOrRegisterResponse>> LoginOrRegister(LoginOrRegisterRequest request);

        Task<IApiResponse<JWTEntity>> RefreshToken(string refresh_token);

        Task<IApiResponse<JWTEntity>> RefreshToken(JWTEntity jWT)
        {
            var refresh_token = jWT.RefreshToken;
            return RefreshToken(refresh_token.ThrowIsNull(nameof(refresh_token)));
        }

        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IApiResponse<ClockInResponse>> ClockIn(ClockInRequest request);

        /// <summary>
        /// 获取签到历史记录
        /// </summary>
        /// <param name="time">时间 服务器会自动取该时间本月的所有签到记录</param>
        /// <returns></returns>
        Task<IApiResponse<IEnumerable<DateTimeOffset>>> ClockInLogs(DateTimeOffset? time);
    }
}