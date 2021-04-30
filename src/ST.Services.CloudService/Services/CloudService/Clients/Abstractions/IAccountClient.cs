using System.Application.Models;
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
    }
}