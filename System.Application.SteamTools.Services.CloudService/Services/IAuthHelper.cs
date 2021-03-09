using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IAuthHelper
    {
        ValueTask<JWTEntity?> GetAuthTokenAsync();

        /// <summary>
        /// 登出
        /// </summary>
        Task SignOutAsync();
    }
}