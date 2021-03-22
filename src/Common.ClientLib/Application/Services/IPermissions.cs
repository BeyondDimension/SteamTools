using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.Services
{
    /// <summary>
    /// 运行时权限
    /// </summary>
    public interface IPermissions
    {
        /// <inheritdoc cref="Permissions.BasePlatformPermission"/>
        public interface IPermission
        {
            /// <inheritdoc cref="Permissions.BasePlatformPermission.CheckStatusAsync"/>
            Task<PermissionStatus> CheckStatusAsync();

            /// <inheritdoc cref="Permissions.BasePlatformPermission.RequestAsync"/>
            Task<PermissionStatus> RequestAsync();

            /// <inheritdoc cref="Permissions.BasePlatformPermission.ShouldShowRationale"/>
            bool ShouldShowRationale();
        }

        /// <summary>
        /// 获取手机号码所需权限
        /// </summary>
        public interface IGetPhoneNumber : IPermission { }

        /// <summary>
        /// 检查并申请一组权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        Task<PermissionStatus> CheckAndRequestAsync(IPermission permission);
    }
}