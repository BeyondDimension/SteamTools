using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IAuthHelper
    {
        ValueTask<string?> GetAuthTokenAsync();

        /// <summary>
        /// 登出
        /// </summary>
        Task SignOutAsync();
    }
}