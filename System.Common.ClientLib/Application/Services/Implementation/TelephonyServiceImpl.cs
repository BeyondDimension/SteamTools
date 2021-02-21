using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    public abstract class TelephonyServiceImpl : ITelephonyService
    {
        readonly IPermissions pf;
        readonly IPermissions.IGetPhoneNumber p1;

        public TelephonyServiceImpl(IPermissions pf, IPermissions.IGetPhoneNumber p1)
        {
            this.pf = pf;
            this.p1 = p1;
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
            var status = await pf.CheckAndRequestAsync(p1);
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