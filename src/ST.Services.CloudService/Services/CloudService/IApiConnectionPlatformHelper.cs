using System.Application.Columns;
using System.Application.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public interface IApiConnectionPlatformHelper
    {
        #region Authentication

        IAuthHelper Auth { get; }

        /// <summary>
        /// 保存用户登录凭证
        /// </summary>
        /// <param name="authToken"></param>
        Task SaveAuthTokenAsync(JWTEntity authToken);

        /// <summary>
        /// 当登录完成时
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="response"></param>
        Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response);

        AuthenticationHeaderValue? GetAuthenticationHeaderValue(JWTEntity? authToken)
        {
            if (authToken.HasValue())
            {
                var authHeaderValue = new AuthenticationHeaderValue(Constants.Basic, authToken?.AccessToken);
                return authHeaderValue;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 显示响应错误消息
        /// </summary>
        /// <param name="message"></param>
        void ShowResponseErrorMessage(string message);

        void ShowResponseErrorMessage(IApiResponse response, string? errorAppendText = null)
        {
            if (response.Code == ApiResponseCode.Canceled) return;
            var message = ApiResponse.GetMessage(response, errorAppendText);
            ShowResponseErrorMessage(message);
        }

        HttpClient CreateClient();

        RSA RSA { get; }

        Task<IApiResponse<JWTEntity>> RefreshToken(JWTEntity jwt);
    }
}