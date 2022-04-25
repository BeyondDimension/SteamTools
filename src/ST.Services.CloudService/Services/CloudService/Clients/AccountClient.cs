using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AccountClient : ApiClient, IAccountClient
    {
        public AccountClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<LoginOrRegisterResponse>> LoginOrRegister(LoginOrRegisterRequest request)
         => conn.SendAsync<LoginOrRegisterRequest, LoginOrRegisterResponse>(
             isAnonymous: true,
             isSecurity: true,
             method: HttpMethod.Post,
             requestUri: "api/Account/LoginOrRegister",
             request: request,
             cancellationToken: default,
             responseContentMaybeNull: false);

        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<IApiResponse<ClockInResponse>> ClockIn(ClockInRequest request)
            => conn.SendAsync<ClockInRequest, ClockInResponse>(
                isAnonymous: false,
                isSecurity: true,
                method: HttpMethod.Post,
                requestUri: "api/Manage/ClockIn",
                request: request,
                cancellationToken: default,
                responseContentMaybeNull: false);

        /// <summary>
        /// 获取签到历史记录
        /// </summary>
        /// <param name="time">时间 服务器会自动取该时间本月的所有签到记录</param>
        /// <returns></returns>
        public Task<IApiResponse<IEnumerable<DateTimeOffset>>> ClockInLogs(DateTimeOffset? time)
          => conn.SendAsync<IEnumerable<DateTimeOffset>>(
              isAnonymous: true,
              isSecurity: true,
              method: HttpMethod.Get,
              requestUri: $"api/Manage/ClockInLogs?time={(time.HasValue ? time : DateTimeOffset.Now)}",
              cancellationToken: default,
              responseContentMaybeNull: false);

        public Task<IApiResponse<JWTEntity>> RefreshToken(string refresh_token)
            => conn.SendAsync<JWTEntity>(
                isAnonymous: true, // 刷新Token必须匿名身份，否则将递归死循环
                isSecurity: true,
                method: HttpMethod.Get,
                requestUri: $"api/Account/RefreshToken/{refresh_token}",
                cancellationToken: default,
                responseContentMaybeNull: true);
    }
}