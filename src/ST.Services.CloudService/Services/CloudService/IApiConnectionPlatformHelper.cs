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
        const string TAG = "ApiConnectionPH";

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

        void ShowResponseErrorMessage(string? requestUri, IApiResponse response, string? errorAppendText = null)
        {
            if (response.Code == ApiResponseCode.Canceled) return;
            var message = response.GetMessageByAppendText(errorAppendText);
            ShowResponseErrorMessage(message);
            Exception? exception = null;
            if (response is ApiRsp apiRsp) exception = apiRsp.ClientException;
            Log.Error(TAG, exception, $"requestUri: {(string.IsNullOrWhiteSpace(requestUri) ? string.Empty : requestUri.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())}, message: {message}");
        }

        HttpClient CreateClient();

        RSA RSA { get; }

        Task<IApiResponse<JWTEntity>> RefreshToken(JWTEntity jwt);
    }
}