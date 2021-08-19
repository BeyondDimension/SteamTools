using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    public abstract class TelephonyServiceImpl : ITelephonyService
    {
        readonly IPermissions p;

        public TelephonyServiceImpl(IPermissions p)
        {
            this.p = p;
        }

        /// <summary>
        /// 由特定平台实现的获取手机号码
        /// </summary>
        /// <returns></returns>
        protected abstract string? PlatformGetPhoneNumber();

        /// <summary>
        /// 由特定平台实现的检查权限后获取手机号码
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<string?> PlatformGetPhoneNumberAsync()
        {
            var status = await p.CheckAndRequestAsync(IPermissions.IGetPhoneNumber.Instance);
            if (status.IsOk())
            {
                return PlatformGetPhoneNumber();
            }
            return null;
        }

        public async Task<string?> GetPhoneNumberAsync()
        {
            var phoneNumber = await PlatformGetPhoneNumberAsync();
            return phoneNumber;
        }
    }
}