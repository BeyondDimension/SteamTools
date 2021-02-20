using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 电话服务
    /// </summary>
    public interface ITelephonyService
    {
        /// <summary>
        /// 获取当前设备的手机号码
        /// </summary>
        /// <returns></returns>
        Task<string?> GetPhoneNumberAsync();
    }
}